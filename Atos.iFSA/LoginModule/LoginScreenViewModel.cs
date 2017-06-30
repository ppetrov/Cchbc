using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.iFSA.AgendaModule;
using Atos.iFSA.AgendaModule.Data;
using Atos.iFSA.AgendaModule.Objects;
using Atos.iFSA.LoginModule.Data;
using Atos.iFSA.LoginModule.Objects;
using Atos.iFSA.Objects;
using Atos.iFSA.ReplicationModule.Objects;
using iFSA.AgendaModule.Objects;

namespace Atos.iFSA.LoginModule
{
	public sealed class LoginScreenViewModel : ViewModel
	{
		private MainContext MainContext { get; }
		private IAppNavigator AppNavigator { get; }
		private LoginScreenDataProvider DataProvider { get; }

		private User[] _users;
		private User _dataUser;
		private Task<List<AgendaOutlet>> _dataLoader;

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
				this.MainContext.Save(feature);

				// Load users
				using (var ctx = this.MainContext.CreateFeatureContext(feature))
				{
					_users = this.DataProvider.GetUsers(ctx);
					ctx.Complete();
				}

				// Prepopulate with last successfully logged-in user
				var userSettings = this.DataProvider.GetUserSettings();
				if (userSettings != null)
				{
					this.Username = userSettings.Username;

					// Find the user for which to load the data
					foreach (var user in _users)
					{
						if (user.Name.Equals(this.Username, StringComparison.OrdinalIgnoreCase))
						{
							_dataUser = user;
							_dataLoader = Task.Run(() =>
							{
								using (var ctx = this.MainContext.CreateFeatureContext(new Feature(nameof(LoginScreenViewModel), @"LoadAgenda")))
								{
									var outlets = new AgendaDataProvider().GetAgendaOutlets(ctx, _dataUser, DateTime.Today);
									ctx.Complete();

									return outlets;
								}
							});
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private async void Login()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Login));
			try
			{
				this.MainContext.Save(feature);

				var username = (this.Username ?? string.Empty).Trim().ToUpperInvariant();
				var password = (this.Password ?? string.Empty);
				var user = default(User);

				foreach (var u in _users)
				{
					if (u.Name.Equals(username, StringComparison.OrdinalIgnoreCase) &&
						false)
					{
						user = u;
						break;
					}
				}
				if (user == null)
				{
					await this.MainContext.ShowMessageAsync(new LocalizationKey(nameof(LoginScreenViewModel), @"WrongCredentials"));
					return;
				}

				// Save the current username
				var userSettings = this.DataProvider.GetUserSettings();
				if (userSettings != null)
				{
					userSettings.Username = username;
					this.DataProvider.SaveUserSettings(userSettings);
				}

				// Display agenda
				var outlets = default(List<AgendaOutlet>);
				if (_dataLoader != null)
				{
					if (user.Id == _dataUser.Id)
					{
						outlets = _dataLoader.Result;
					}
				}
				this.AppNavigator.NavigateTo(AppScreen.Agenda, new AgendaDay(user, DateTime.Today, outlets));
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
				this.MainContext.Save(feature);

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
			return this.MainContext.GetLocalized(new LocalizationKey(nameof(LoginScreenViewModel), name));
		}
	}
}