using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using iFSA;
using iFSA.AgendaModule;
using iFSA.AgendaModule.Data;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;
using iFSA.LoginModule.Data;
using iFSA.ReplicationModule.Objects;

namespace UIDemo
{
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

		public void Dispatch(Action operation)
		{
			this.CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => operation());
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
		private AgendaScreenParam _args;
		public AgendaScreenViewModel ViewModel { get; }

		public MainPage()
		{
			this.InitializeComponent();

			this.ViewModel = new AgendaScreenViewModel(GlobalAppContext.MainContext, new Agenda(DataCreator.CreateAgendaData()), GlobalAppContext.AppNavigator, new UIThreadDispatcher(this.Dispatcher));
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			_args = e.Parameter as AgendaScreenParam;

			_args = _args ?? new AgendaScreenParam(new User(1, @"PPetrov", string.Empty), DateTime.Today);
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				this.ViewModel.LoadDay(_args.User, _args.DateTime);

				this.ViewModel.DisplayAddActivityCommand.Execute(this);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}
	}
}
