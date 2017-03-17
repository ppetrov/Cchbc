using System;
using System.Collections.ObjectModel;
using System.Threading;
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
		private ManualResetEventSlim DataLoadedEvent { get; } = new ManualResetEventSlim(false);

		private Agenda Agenda { get; }
		private IAppNavigator AppNavigator { get; }
		private IUIThreadDispatcher UIThreadDispatcher { get; }
		private MainContext MainContext { get; }

		private DateTime _currentDate;
		public DateTime CurrentDate
		{
			get { return _currentDate; }
			set { this.SetProperty(ref _currentDate, value); }
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
			this.PreviousDayCommand = new RelayCommand(() =>
			{
				this.LoadDay(this.Agenda.LoadPreviousDay);
			});
			this.NextDayCommand = new RelayCommand(() =>
			{
				this.LoadDay(this.Agenda.LoadNextDay);
			});
			this.DisplayCalendarCommand = new RelayCommand(() =>
			{
				// TODO : Probably it's better to pass Agenda as reference
				// Close/Cancel is performed via Agenda ONLY

				// How to close/cancel multiple days ???
				this.AppNavigator.NavigateTo(AppScreen.Calendar, this.CurrentDate);
			});
			this.AddActvityCommand = new RelayCommand(() =>
			{
				// TODO : !!! Add support for Add
				lock (this.Outlets)
				{
					this.Outlets.Add(new AgendaOutletViewModel(this.MainContext, null));
				}
			});
			this.RemoveActivityCommand = new RelayCommand(() =>
			{
				// TODO : !!! Add support for Delete
				lock (this.Outlets)
				{
					this.Outlets.RemoveAt(0);
				}
			});

			Task.Run(() =>
			{
				foreach (var outletImage in this.Agenda.OutletImages.GetConsumingEnumerable())
				{
					// We need to wait until the data is loaded
					// before tring to setup the image
					// It's possible that the image comes from a thread before the rest of the data
					this.DataLoadedEvent.Wait();

					var match = default(AgendaOutletViewModel);

					lock (this.Outlets)
					{
						var number = outletImage.Outlet;
						foreach (var viewModel in this.Outlets)
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
			});
		}

		public void LoadDay(DateTime dateTime)
		{
			this.LoadDay(_ =>
			{
				this.Agenda.LoadDay(_, dateTime);
			});
		}

		private void LoadDay(Action<MainContext> dayLoader)
		{
			try
			{
				this.DataLoadedEvent.Reset();

				dayLoader(this.MainContext);

				lock (this.Outlets)
				{
					this.Outlets.Clear();
					foreach (var outlet in this.Agenda.Outlets)
					{
						this.Outlets.Add(new AgendaOutletViewModel(this.MainContext, outlet));
					}
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
			finally
			{
				this.DataLoadedEvent.Set();
			}
		}
	}
}