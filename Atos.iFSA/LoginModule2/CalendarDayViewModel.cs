using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;

namespace Atos.iFSA.LoginModule2
{
	public sealed class CalendarDayViewModel : ViewModel
	{
		public CalendarDay CalendarDay { get; }
		private CalendarViewModel ViewModel { get; }
		public DateTime Date { get; }
		public DayStatus Status { get; }

		public ICommand CloseDayCommand { get; }
		public ICommand CancelDayCommand { get; }
		public ICommand ViewAgendaCommand { get; }

		public CalendarDayViewModel(CalendarDay calendarDay, CalendarViewModel viewModel)
		{
			if (calendarDay == null) throw new ArgumentNullException(nameof(calendarDay));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.CalendarDay = calendarDay;
			this.ViewModel = viewModel;
			this.Date = calendarDay.Date;
			this.Status = calendarDay.Status;
			this.CloseDayCommand = new RelayCommand(() =>
			{
				throw new NotImplementedException();
			});
			this.CancelDayCommand = new RelayCommand(() =>
			{
				this.ViewModel.CancelDays(new[] { this });
			});
			this.ViewAgendaCommand = new RelayCommand(() =>
			{
				this.ViewModel.ViewAgenda(this);
			});
		}
	}
}