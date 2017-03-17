using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Cchbc;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using iFSA;
using iFSA.AgendaModule;
using iFSA.AgendaModule.Data;
using iFSA.AgendaModule.Objects;
using iFSA.LoginModule;
using iFSA.LoginModule.Data;
using iFSA.ReplicationModule.Objects;

namespace UIDemo
{
	public static class GlobalAppContext
	{
		public static MainContext MainContext { get; set; }
		public static IAppNavigator AppNavigator { get; set; }

		static GlobalAppContext()
		{
			MainContext = new MainContext((message, logLevel) =>
			{
				Debug.WriteLine(logLevel.ToString() + ":" + message);
			},
			() => default(IDbContext), new ModalDialog());

			AppNavigator = new AppNavigator(false);
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
		public AppNavigator(bool _)
		{

		}
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

	public static class DataCreator
	{
		public static AgendaData CreateAgendaData()
		{
			return new AgendaData(AgendaDataProvider.GetAgendaOutlets, AgendaDataProvider.GetDefaultOutletImage);
		}

		public static LoginScreenData CreateLoginScreenData()
		{
			return new LoginScreenData(
				UserSettingsProvider.Load,
				UserSettingsProvider.Save,
				LoginScreenDataProvider.GetCountries,
				LoginScreenDataProvider.GetUsers,
				CreateAgendaData(),
				(s, c) =>
				{
					var config = new ReplicationConfig(string.Empty, 0);
					return Task.FromResult(config);
				});
		}
	}

	public sealed partial class MainPage
	{
		public AgendaScreenParam ScreenParam { get; set; }
		public AgendaScreenViewModel ViewModel { get; set; }

		public LoginScreenViewModel LoginScreenViewModel { get; set; }

		public MainPage()
		{
			this.InitializeComponent();

			this.LoginScreenViewModel = new LoginScreenViewModel(GlobalAppContext.MainContext, GlobalAppContext.AppNavigator, DataCreator.CreateLoginScreenData());
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			// TODO : !!! Get from the parameter
			var screenParam = this.ScreenParam;

			var agenda = screenParam.Agenda;

			var loadData = (agenda == null);
			if (agenda == null)
			{
				agenda = new Agenda(screenParam.User, DataCreator.CreateAgendaData());
			}
			this.ViewModel = new AgendaScreenViewModel(GlobalAppContext.MainContext, agenda, GlobalAppContext.AppNavigator, new UIThreadDispatcher(this.Dispatcher));
			this.DataContext = this.ViewModel;

			if (loadData)
			{
				this.ViewModel.LoadDay(screenParam.Date);
			}
		}
	}
}
