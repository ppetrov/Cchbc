using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Logs;
using iFSA.AgendaModule.Objects;
using iFSA.AgendaModule.ViewModels;

namespace iFSA.AgendaModule
{
	public sealed class AgendaScreenViewModel : ViewModel
	{
		private Agenda Agenda { get; }
		private IAppNavigator AppNavigator { get; }
		private IUIThreadDispatcher UIThreadDispatcher { get; }
		private MainContext MainContext { get; }
		private List<AgendaOutletViewModel> AllOutlets { get; } = new List<AgendaOutletViewModel>();

		private DateTime _currentDate = DateTime.Today;
		public DateTime CurrentDate
		{
			get { return _currentDate; }
			set { this.SetProperty(ref _currentDate, value); }
		}

		private string _search = string.Empty;
		public string Search
		{
			get { return _search; }
			set
			{
				this.SetProperty(ref _search, value);
				this.ApplyCurrentTextSearch();
			}
		}

		public ICommand PreviousDayCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand DisplayCalendarCommand { get; }
		public ICommand AddActvityCommand { get; }
		public ICommand RemoveActivityCommand { get; }

		public ObservableCollection<AgendaOutletViewModel> Outlets { get; } = new ObservableCollection<AgendaOutletViewModel>();

		public AgendaScreenViewModel(MainContext mainContext, Agenda agenda, IAppNavigator appNavigator, IUIThreadDispatcher uiThreadDispatcher)
		{
			if (agenda == null) throw new ArgumentNullException(nameof(agenda));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));
			if (uiThreadDispatcher == null) throw new ArgumentNullException(nameof(uiThreadDispatcher));

			this.Agenda = agenda;
			this.AppNavigator = appNavigator;
			this.UIThreadDispatcher = uiThreadDispatcher;
			this.MainContext = mainContext;
			this.PreviousDayCommand = new RelayCommand(this.LoadPreviousDay);
			this.NextDayCommand = new RelayCommand(this.LoadNextDay);
			this.DisplayCalendarCommand = new RelayCommand(this.DisplayCalendar);
			this.AddActvityCommand = new RelayCommand(this.AddActivity);
			this.RemoveActivityCommand = new RelayCommand(this.RemoveActivity);
		}

		public void LoadCurrentDay()
		{
			this.LoadDay(_ =>
			{
				this.Agenda.LoadDay(_, this.CurrentDate);
			});
		}

		private void ApplyCurrentTextSearch()
		{
			var search = this.Search;

			lock (this)
			{
				this.Outlets.Clear();
				foreach (var viewModel in this.AllOutlets)
				{
					if (viewModel.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						this.Outlets.Add(viewModel);
					}
				}
			}
		}

		private void LoadNextDay()
		{
			this.LoadDay(this.Agenda.LoadNextDay);
		}

		private void LoadPreviousDay()
		{
			this.LoadDay(this.Agenda.LoadPreviousDay);
		}

		private void DisplayCalendar()
		{
			// TODO : Probably it's better to pass Agenda as reference
			// Close/Cancel is performed via Agenda ONLY, or maybe not
			// How to close/cancel multiple days ???
			this.AppNavigator.NavigateTo(AppScreen.Calendar, this.CurrentDate);
		}

		private void AddActivity()
		{
			this.AppNavigator.NavigateTo(AppScreen.Outlets, this.Agenda);
			// TODO : !!! Add support for Add
			//lock (this.Outlets)
			//{
			//	this.Outlets.Add(new AgendaOutletViewModel(this.MainContext, null));
			//}
		}

		private void RemoveActivity()
		{
			// TODO : !!! Add support for Delete
			//lock (this.Outlets)
			//{
			//	this.Outlets.RemoveAt(0);
			//}
		}

		private void LoadDay(Action<MainContext> dayLoader)
		{
			dayLoader(this.MainContext);

			lock (this)
			{
				this.Outlets.Clear();
				this.AllOutlets.Clear();
				foreach (var outlet in this.Agenda.Outlets)
				{
					var viewModel = new AgendaOutletViewModel(this.MainContext, outlet);
					this.Outlets.Add(viewModel);
					this.AllOutlets.Add(viewModel);
				}
			}

			Task.Run(() =>
			{
				try
				{
					while (!this.Agenda.ImagesLoadedEvent.IsSet)
					{
						OutletImage outletImage;
						if (this.Agenda.OutletImages.TryDequeue(out outletImage))
						{
							var match = default(AgendaOutletViewModel);

							lock (this)
							{
								var number = outletImage.Outlet;
								foreach (var viewModel in this.AllOutlets)
								{
									if (viewModel.Number == number)
									{
										match = viewModel;
										break;
									}
								}
							}

							if (match != null)
							{
								this.UIThreadDispatcher.Dispatch(() =>
								{
									match.OutletImage = DateTime.Now.ToString(@"G");
								});
							}
						}
						Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
					}
				}
				catch (Exception ex)
				{
					this.MainContext.Log(ex.ToString(), LogLevel.Error);
				}
			});
		}
	}
}