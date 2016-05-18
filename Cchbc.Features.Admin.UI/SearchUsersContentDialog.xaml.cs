using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Input;
using Cchbc.Features.Admin.DashboardModule;

namespace Cchbc.Features.Admin.UI
{
	public sealed partial class SearchUsersContentDialog
	{
		private Action<DashboardUserViewModel> AcceptAction { get; }

		public ObservableCollection<DashboardUserViewModel> Users { get; } = new ObservableCollection<DashboardUserViewModel>();

		public SearchUsersContentDialog(Action<DashboardUserViewModel> acceptAction)
		{
			if (acceptAction == null) throw new ArgumentNullException(nameof(acceptAction));

			this.AcceptAction = acceptAction;

			this.InitializeComponent();
		}

		private void BtnAcceptTapped(object sender, TappedRoutedEventArgs e)
		{
			this.AcceptAction(null);
		}

		private void BtnCancelTapped(object sender, TappedRoutedEventArgs e)
		{
			this.Hide();
		}
	}
}
