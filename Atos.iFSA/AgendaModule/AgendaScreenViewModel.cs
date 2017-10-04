using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Selectors;
using Atos.iFSA.AgendaModule.Objects;
using Atos.iFSA.AgendaModule.ViewModels;
using Atos.iFSA.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule
{
	public sealed class AgendaScreenViewModel : ScreenViewModel
	{
		private Agenda Agenda { get; }
		private AgendaDay AgendaDay { get; set; }
		private ConcurrentDictionary<long, AgendaOutletViewModel> AllOutlets { get; } = new ConcurrentDictionary<long, AgendaOutletViewModel>();

		private string _search = string.Empty;
		public string Search
		{
			get { return _search; }
			set
			{
				this.SetProperty(ref _search, value);
				this.MainContext.Save(new Feature(nameof(AgendaScreenViewModel), nameof(Search)));
				this.ApplyCurrentTextSearch();
			}
		}

		public ICommand PreviousDayCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand DisplayCalendarCommand { get; }
		public ICommand DisplayAddActivityCommand { get; }

		public ObservableCollection<AgendaOutletViewModel> Outlets { get; } = new ObservableCollection<AgendaOutletViewModel>();

		public AgendaScreenViewModel(MainContext mainContext) : base(mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.PreviousDayCommand = new RelayCommand(this.PreviousDay);
			this.NextDayCommand = new RelayCommand(this.NextDay);
			this.DisplayCalendarCommand = new RelayCommand(this.DisplayCalendar);
			this.DisplayAddActivityCommand = new RelayCommand(this.DisplayAddActivity);

			this.Agenda = new Agenda(mainContext);
			this.Agenda.ActivityInserted += this.ActivityInserted;
			this.Agenda.ActivityUpdated += this.ActivityUpdated;
			this.Agenda.ActivityDeleted += this.ActivityDeleted;
		}

		public override Task InitializeAsync(object parameter)
		{
			this.LoadData(parameter as AgendaDay);

			return base.InitializeAsync(parameter);
		}

		public async void ChangeStartTime(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(ChangeStartTime));
			try
			{
				this.MainContext.Save(feature);

				var activity = activityViewModel.Model;
				var timeSelector = this.MainContext.GetService<ITimeSelector>();
				await timeSelector.ShowAsync(
					dateTime => this.Agenda.CanChangeStartTime(activity, dateTime),
					dateTime => this.Agenda.ChangeStartTime(activity, dateTime));
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		public async void CancelAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(CancelAsync));
			try
			{
				this.MainContext.Save(feature);

				var cancelReasonSelector = this.MainContext.GetService<IActivityCancelReasonSelector>();

				var activity = activityViewModel.Model;
				await cancelReasonSelector.ShowAsync(activity,
					cancelReason => this.Agenda.CanCancel(activity, cancelReason),
					cancelReason => this.Agenda.Cancel(activity, cancelReason));
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		public async void CloseAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		private void NextDay()
		{
			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(NextDay));
			try
			{
				this.MainContext.Save(feature);

				using (var ctx = this.MainContext.CreateDataQueryContext())
				{
					this.LoadData(ctx, TimeSpan.FromDays(1));
					ctx.Complete();
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private void PreviousDay()
		{
			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(PreviousDay));
			try
			{
				this.MainContext.Save(feature);

				using (var ctx = this.MainContext.CreateDataQueryContext())
				{
					this.LoadData(ctx, TimeSpan.FromDays(-1));
					ctx.Complete();
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private void ApplyCurrentTextSearch()
		{
			var search = this.Search;

			this.Outlets.Clear();
			foreach (var viewModel in this.AllOutlets.Values)
			{
				if (viewModel.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					this.Outlets.Add(viewModel);
				}
			}
		}

		private void DisplayCalendar()
		{
			//this.NavigationService.NavigateTo(AppScreen.Calendar, this.AgendaDay);
		}

		private void DisplayAddActivity()
		{
			//this.NavigationService.NavigateTo(AppScreen.AddActivity, this.AgendaDay);
		}

		private void LoadData(DataQueryContext context, TimeSpan timeOffset)
		{
			var user = this.AgendaDay.User;
			var date = this.AgendaDay.Date.Add(timeOffset);
			var agendaDay = new AgendaDay(user, date, this.MainContext.GetService<IAgendaDataProvider>().GetAgendaOutlets(context, user, date));
			this.LoadData(agendaDay);
		}

		private void LoadData(AgendaDay agendaDay)
		{
			this.AgendaDay = agendaDay;

			this.Outlets.Clear();
			this.AllOutlets.Clear();
			foreach (var outlet in this.AgendaDay.Outlets)
			{
				var viewModel = new AgendaOutletViewModel(this, outlet);
				this.Outlets.Add(viewModel);
				this.AllOutlets.TryAdd(outlet.Id, viewModel);
			}
		}

		private void Sort()
		{
			throw new NotImplementedException();
		}

		private void ActivityInserted(object sender, ActivityEventArgs e)
		{
			var activity = e.Activity;
			var outlet = activity.Outlet;

			var outletViewModel = FindOutletViewModel(outlet);
			if (outletViewModel == null)
			{
				// Create new OutletViewModel
				outletViewModel = new AgendaOutletViewModel(this, new AgendaOutlet(outlet, new List<Activity>()));

				// Add it to the list
				this.AllOutlets.TryAdd(outlet.Id, outletViewModel);
			}

			// Add the new activity
			outletViewModel.Activities.Add(new ActivityViewModel(outletViewModel, activity));

			this.Sort();
			this.ApplyCurrentTextSearch();
		}

		private void ActivityUpdated(object sender, ActivityEventArgs e)
		{
			var activity = e.Activity;
			var outlet = activity.Outlet;

			var outletViewModel = FindOutletViewModel(outlet);
			if (outletViewModel != null)
			{
				var acitivityId = activity.Id;
				var activityViewModels = outletViewModel.Activities;
				for (var index = 0; index < activityViewModels.Count; index++)
				{
					var activityViewModel = activityViewModels[index];
					if (activityViewModel.Model.Id == acitivityId)
					{
						activityViewModels[index] = new ActivityViewModel(outletViewModel, activity);
						break;
					}
				}

				this.Sort();
				this.ApplyCurrentTextSearch();
			}
		}

		private void ActivityDeleted(object sender, ActivityEventArgs e)
		{
			throw new NotImplementedException();
		}

		private AgendaOutletViewModel FindOutletViewModel(Outlet outlet)
		{
			var outletId = outlet.Id;

			foreach (var viewModel in this.AllOutlets.Values)
			{
				if (viewModel.Outlet.Id == outletId)
				{
					return viewModel;
				}
			}

			return null;
		}
	}
}