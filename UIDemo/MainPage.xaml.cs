using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using iFSA;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;

namespace UIDemo
{
	public static class GlobalAppContext
	{
		public static Agenda Agenda { get; set; }
		public static MainContext MainContext { get; set; }

		static GlobalAppContext()
		{
			Agenda = new Agenda((m, u, d) =>
			{
				return new List<Visit>();
			});
			MainContext = new MainContext((m, l) =>
			{
				Debug.WriteLine(l.ToString() + ":" + m);
			},
			() => default(IDbContext), new ModalDialog());
		}
	}

	public sealed class ModalDialog : IModalDialog
	{
		public Task<DialogResult> ShowAsync(string message, Feature feature, DialogType? type = null)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AppNavigator : IAppNavigator
	{
		public void NavigateTo(AppScreen screen, object args)
		{
			throw new System.NotImplementedException();
		}

		public void GoBack()
		{
			throw new System.NotImplementedException();
		}
	}

	public sealed class UIThreadDispatcher : IUIThreadDispatcher
	{
		private CoreDispatcher CoreDispatcher { get; }

		public UIThreadDispatcher(CoreDispatcher coreDispatcher)
		{
			if (coreDispatcher == null) throw new ArgumentNullException(nameof(coreDispatcher));

			this.CoreDispatcher = coreDispatcher;
		}

		public void Dispatch(Action action)
		{
			this.CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
		}
	}

	public sealed partial class MainPage
	{
		public AgendaScreenViewModel ViewModel { get; }

		public MainPage()
		{
			this.InitializeComponent();

			this.ViewModel = new AgendaScreenViewModel(GlobalAppContext.MainContext, GlobalAppContext.Agenda, new AppNavigator(), new UIThreadDispatcher(this.Dispatcher));
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{			
			this.ViewModel.LoadData(new User(1, @"PPetrov", string.Empty), DateTime.Today);
		}
	}
}
