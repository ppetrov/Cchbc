using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Dialog;
using Atos.Client.Navigation;
using Atos.iFSA.Objects;

namespace Atos.iFSA.LoginModule2
{
	public sealed class CalendarViewModel : ViewModel
	{
		private DateTime CurrentMonth { get; set; }

		private IModalDialog ModalDialog { get; }
		private INavigationService NavigationService { get; }
		private Func<string, string, string> LocalizationManager { get; }
		private ICalendarDataProvider DataProvider { get; }
		private Func<Task<CancelReason>> CancelReasonSelector { get; }
		private DataCache DataCache { get; }

		public ObservableCollection<CalendarDayViewModel> Days { get; } = new ObservableCollection<CalendarDayViewModel>();
		public ObservableCollection<CalendarDayViewModel> SelectedDays { get; } = new ObservableCollection<CalendarDayViewModel>();

		public ICommand CloseDaysCommand { get; }
		public ICommand CancelDaysCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand PreviousDayCommand { get; }

		public CalendarViewModel(IModalDialog modalDialog, Func<Task<CancelReason>> cancelReasonSelector, INavigationService navigationService, ICalendarDataProvider dataProvider, Func<string, string, string> localizationManager, DataCache dataCache)
		{
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));
			if (cancelReasonSelector == null) throw new ArgumentNullException(nameof(cancelReasonSelector));


			this.ModalDialog = modalDialog;
			this.CancelReasonSelector = cancelReasonSelector;
			this.NavigationService = navigationService;
			this.DataProvider = dataProvider;
			this.LocalizationManager = localizationManager;
			this.DataCache = dataCache;
			this.CloseDaysCommand = new ActionCommand(this.CloseSelectedDays);
			this.CancelDaysCommand = new ActionCommand(this.CancelSelectedDays);
			this.CurrentMonth = DateTime.Today;
			this.NextDayCommand = new ActionCommand(() =>
			{
				this.Load(this.CurrentMonth.AddMonths(1));
			});
			this.PreviousDayCommand = new ActionCommand(() =>
			{
				this.Load(this.CurrentMonth.AddMonths(-1));
			});
		}

		public void Load(DateTime date)
		{
			this.CurrentMonth = date;

			//foreach (var day in this.DataProvider.GetCalendar(this.Agenda.User, this.CurrentMonth))
			//{
			//	this.Days.Add(new CalendarDayViewModel(day, this));
			//}
		}

		public void ViewAgenda(CalendarDayViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			// TODO : How to navigate to a different screen
			//this.NavigationService.NavigateTo(null, viewModel.Date);
		}

		public async void CancelDays(ICollection<CalendarDayViewModel> viewModels)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));

			//var confirmationMessage = this.LocalizationManager(@"Calendar", @"ConfirmCancelDay");
			//var isConfirmed = (await this.ModalDialog.ShowAsync(confirmationMessage, Feature.None, PermissionType.Confirm)) == DialogResult.Accept;
			//if (!isConfirmed) return;

			// We don't have any selected days
			if (viewModels.Count == 0) return;

			CancelReason cancelReason = null;

			var operation = new CalendarCancelOperation(this.DataProvider);

			var daysChecker = new CalendarDaysChecker(this.DataProvider.GetPreviousDays);

			foreach (var day in GetDays(viewModels))
			{
				var activeDay = daysChecker.GetActiveDayBefore(GetCurrentDays(this.Days), day);

				var hasActiveDayBefore = activeDay != null;
				if (hasActiveDayBefore)
				{
					//var message = this.LocalizationManager(@"Calendar", @"ActiveDayBefore") + activeDay.Date.ToString(@"D");
					//await this.ModalDialog.ShowAsync(message, Feature.None);
					break;
				}

				if (cancelReason == null)
				{
					// Load all visit/activities ??? => no
					// TODO :
					var activities = this.GetCurrentDayActivities();
					var hasActivitiesForCancel = activities.Any();
					if (hasActivitiesForCancel)
					{

					}
					if (hasActivitiesForCancel || this.DataProvider.HasActivitiesForCancel(day))
					{
						// Prompt for a cancel cancelReason selection	
						cancelReason = await this.CancelReasonSelector();

						// Cancel the operation
						if (cancelReason == null) return;
					}
				}

				// Execute Cancel day
				operation.CancelDays(this.DataCache, day, cancelReason, this.UpdateDayStatus);
			}
		}

		private IEnumerable<Activity> GetCurrentDayActivities()
		{
			throw new NotImplementedException();
		}

		private static IEnumerable<CalendarDay> GetCurrentDays(IReadOnlyList<CalendarDayViewModel> days)
		{
			for (var i = days.Count - 1; i >= 0; i--)
			{
				yield return days[i].CalendarDay;
			}
		}

		private void CloseSelectedDays()
		{
			throw new NotImplementedException();
		}

		private void CancelSelectedDays()
		{
			this.CancelDays(this.SelectedDays);
		}

		private void UpdateDayStatus(CalendarDay day)
		{
			for (var i = 0; i < this.Days.Count; i++)
			{
				var viewModel = this.Days[i];
				if (viewModel.CalendarDay.Date == day.Date)
				{
					this.Days[i] = new CalendarDayViewModel(new CalendarDay(day.Date, day.Status), this);
					break;
				}
			}
		}

		private static CalendarDay[] GetDays(ICollection<CalendarDayViewModel> viewModels)
		{
			var days = new CalendarDay[viewModels.Count];

			var index = 0;
			foreach (var viewModel in viewModels)
			{
				days[index++] = viewModel.CalendarDay;
			}

			Array.Sort(days, (x, y) => x.Date.CompareTo(y.Date));

			return days;
		}
	}
}