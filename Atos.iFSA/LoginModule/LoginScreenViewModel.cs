using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.Client.Validation;
using Atos.iFSA.AgendaModule;
using Atos.iFSA.LoginModule.Data;
using Atos.iFSA.LoginModule.Objects;
using Atos.iFSA.ReplicationModule.Objects;

namespace Atos.iFSA.LoginModule
{
	public sealed class LoginScreenViewModel : ViewModel
	{
		private MainContext MainContext { get; }
		private IAppNavigator AppNavigator { get; }
		private LoginScreenDataProvider DataProvider { get; }

		public string UsernameCaption { get; }
		public string PasswordCaption { get; }
		public string LoginCaption { get; }
		public string AdvancedCaption { get; }

		private string _username = string.Empty;
		public string Username
		{
			get { return _username; }
			set { this.SetProperty(ref _username, value); }
		}
		private string _password = string.Empty;
		public string Password
		{
			get { return _password; }
			set { this.SetProperty(ref _password, value); }
		}

		public ICommand AdvancedCommand { get; }
		public ICommand LoginCommand { get; }

		public LoginScreenViewModel(MainContext mainContext, IAppNavigator appNavigator, LoginScreenDataProvider dataProvider)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.MainContext = mainContext;
			this.AppNavigator = appNavigator;
			this.DataProvider = dataProvider;

			this.UsernameCaption = this.GetLocalizedValue(@"Name");
			this.PasswordCaption = this.GetLocalizedValue(@"Password");
			this.LoginCaption = this.GetLocalizedValue(@"Login");
			this.AdvancedCaption = this.GetLocalizedValue(@"Advanced");

			this.AdvancedCommand = new RelayCommand(this.Advanced);
			this.LoginCommand = new RelayCommand(this.Login);
		}

		public void LoadData()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(LoadData));
			try
			{
				this.MainContext.FeatureManager.Save(feature);

				// Prepopulate with last successfully logged-in user
				var userSettings = this.DataProvider.GetUserSettings();
				if (userSettings != null)
				{
					this.Username = userSettings.Username;
				}
			}
			catch (Exception ex)
			{
				this.MainContext.FeatureManager.Save(feature, ex);
			}
		}

		private async void Login()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Login));
			try
			{
				this.MainContext.FeatureManager.Save(feature);

				using (var ctx = this.MainContext.CreateFeatureContext(feature))
				{
					var username = (this.Username ?? string.Empty).Trim().ToUpperInvariant();
					var password = (this.Password ?? string.Empty);
					var user = this.DataProvider.GetUser(ctx, username, password);
					if (user != null)
					{
						// Save the current username
						var userSettings = this.DataProvider.GetUserSettings();
						if (userSettings != null)
						{
							userSettings.Username = username;
							this.DataProvider.SaveUserSettings(userSettings);
						}

						// Display agenda
						this.AppNavigator.NavigateTo(AppScreen.Agenda, new AgendaScreenParam(user, DateTime.Today));
					}
					else
					{
						var message = PermissionResult.Deny(this.MainContext.LocalizationManager.Get(new LocalizationKey(nameof(LoginScreenViewModel), @"WrongCredentials")));
						await this.MainContext.ModalDialog.ShowAsync(message);
					}
					ctx.Complete();
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private void Advanced()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Advanced));
			try
			{
				this.MainContext.FeatureManager.Save(feature);

				var username = string.Empty;
				var replicaHost = string.Empty;
				var replicaPort = 0;

				var userSettings = this.DataProvider.GetUserSettings();
				if (userSettings != null)
				{
					username = userSettings.Username;
					replicaHost = userSettings.ReplicationHost;
					replicaPort = userSettings.ReplicationPort;
				}

				var login = new Login(username, string.Empty);
				var settings = new ReplicationSettings(new ReplicationConfig(replicaHost, replicaPort), login);

				this.AppNavigator.NavigateTo(AppScreen.Replication, settings);
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private string GetLocalizedValue(string name)
		{
			return this.MainContext.LocalizationManager.Get(new LocalizationKey(nameof(LoginScreenViewModel), name));
		}
	}
}