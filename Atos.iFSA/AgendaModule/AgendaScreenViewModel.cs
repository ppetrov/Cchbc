using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Logs;
using Atos.Client.Selectors;
using Atos.iFSA.AgendaModule.Objects;
using Atos.iFSA.AgendaModule.ViewModels;
using Atos.iFSA.Data;
using Atos.iFSA.Objects;
using iFSA;

namespace Atos.iFSA.AgendaModule
{
	public sealed class AgendaScreenViewModel : ScreenViewModel
	{
		private AgendaDay AgendaDay { get; set; }
		private CancellationTokenSource _cts = new CancellationTokenSource();

		private ActivityManager ActivityManager { get; }
		private List<AgendaOutletViewModel> AllOutlets { get; } = new List<AgendaOutletViewModel>();

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

			//this.Agenda = new Agenda(null, this.OutletImageDownloaded);
			//ITimeSelector timeSelector, INavigationService navigationService, IUIThreadDispatcher uiThreadDispatcher
			//this.TimeSelector = timeSelector;
			//this.UIThreadDispatcher = uiThreadDispatcher;

			this.PreviousDayCommand = new RelayCommand(this.LoadPreviousDay);
			this.NextDayCommand = new RelayCommand(this.LoadNextDay);
			this.DisplayCalendarCommand = new RelayCommand(this.DisplayCalendar);
			this.DisplayAddActivityCommand = new RelayCommand(this.DisplayAddActivity);

			this.ActivityManager = new ActivityManager(mainContext);
			this.ActivityManager.ActivityInserted += this.ActivityInserted;
			this.ActivityManager.ActivityUpdated += this.ActivityUpdated;
			this.ActivityManager.ActivityDeleted += this.ActivityDeleted;
		}

		public void LoadDay(User user, DateTime dateTime, List<AgendaOutlet> outlets = null)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(LoadDay));
			try
			{
				this.MainContext.Save(feature, dateTime.ToString(@"O"));

				using (var ctx = this.MainContext.CreateFeatureContext())
				{
					this.LoadData(ctx, user, dateTime, outlets);
					ctx.Complete();
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private void LoadNextDay()
		{
			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(LoadNextDay));
			try
			{
				this.MainContext.Save(feature);

				using (var ctx = this.MainContext.CreateFeatureContext())
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

		private void LoadPreviousDay()
		{
			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(LoadPreviousDay));
			try
			{
				this.MainContext.Save(feature);

				using (var ctx = this.MainContext.CreateFeatureContext())
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
			lock (this)
			{
				foreach (var viewModel in this.AllOutlets)
				{
					if (viewModel.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						this.Outlets.Add(viewModel);
					}
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

		private void LoadData(FeatureContext context, TimeSpan timeOffset)
		{
			this.LoadData(context, this.AgendaDay.User, this.AgendaDay.Date.Add(timeOffset));
		}

		private void LoadData(FeatureContext context, User user, DateTime dateTime, List<AgendaOutlet> agendaOutlets = null)
		{
			this.AgendaDay = this.GetAgendaDay(context, user, dateTime, agendaOutlets);

			this.Outlets.Clear();
			foreach (var outlet in this.AgendaDay.Outlets)
			{
				this.Outlets.Add(new AgendaOutletViewModel(this, outlet));
			}

			lock (this)
			{
				this.AllOutlets.Clear();
				this.AllOutlets.AddRange(this.Outlets);
			}
		}

		private AgendaDay GetAgendaDay(FeatureContext context, User user, DateTime dateTime, List<AgendaOutlet> agendaOutlets = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (user == null) throw new ArgumentNullException(nameof(user));

			agendaOutlets = agendaOutlets ?? this.MainContext.GetService<IAgendaDataProvider>().GetAgendaOutlets(context, user, dateTime);
			var outlets = new Outlet[agendaOutlets.Count];
			for (var index = 0; index < agendaOutlets.Count; index++)
			{
				outlets[index] = agendaOutlets[index].Outlet;
			}

			// Cancel any pending Images Load
			this._cts.Cancel();

			// Start new Images Load
			this._cts = new CancellationTokenSource();

			Task.Run(() =>
			{
				var dispatcher = this.MainContext.GetService<IUIThreadDispatcher>();
				try
				{
					var cts = this._cts;
					foreach (var outlet in outlets)
					{
						if (cts.IsCancellationRequested)
						{
							break;
						}
						var outletImage = this.MainContext.GetService<IOutletImageDataProvider>().GetDefaultOutletImage(context.MainContext, outlet);
						if (outletImage == null) continue;

						var match = FindOutletViewModel(outletImage.Outlet);
						if (match != null)
						{
							dispatcher.Dispatch(() =>
							{
								match.OutletImage = DateTime.Now.ToString(@"G");
							});
						}
					}
				}
				catch (Exception ex)
				{
					context.MainContext.Log(ex.ToString(), LogLevel.Error);
				}
			}, this._cts.Token);

			return new AgendaDay(user, dateTime, agendaOutlets);
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
					dateTime => this.ActivityManager.CanChangeStartTime(activity, dateTime),
					dateTime => this.ActivityManager.ChangeStartTime(activity, dateTime));
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		public async Task CancelAsync(ActivityViewModel activityViewModel)
		{
			// TODO : From constructor
			var cancelReasonSelector = default(IActivityCancelReasonSelector);

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(CancelAsync));
			try
			{
				this.MainContext.Save(feature);

				//var activity = activityViewModel.Model;
				//await cancelReasonSelector.ShowAsync(activity,
				//	cancelReason => this.Agenda.CanCancel(activity, cancelReason),
				//	cancelReason => this.Agenda.Cancel(activity, cancelReason));
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		public async Task CloseAsync(ActivityViewModel activityViewModel)
		{
			throw new NotImplementedException();
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
				lock (this)
				{
					this.AllOutlets.Add(outletViewModel);
				}
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
			}

			this.Sort();
			this.ApplyCurrentTextSearch();
		}

		private void ActivityDeleted(object sender, ActivityEventArgs e)
		{
			throw new NotImplementedException();
		}

		private AgendaOutletViewModel FindOutletViewModel(Outlet outlet)
		{
			return FindOutletViewModel(outlet.Id);
		}

		private AgendaOutletViewModel FindOutletViewModel(long outletNumber)
		{
			// Search in all outlets
			lock (this)
			{
				foreach (var viewModel in this.AllOutlets)
				{
					if (viewModel.Outlet.Id == outletNumber)
					{
						return viewModel;
					}
				}
			}

			return null;
		}
	}
}