using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.Client.Settings;
using Atos.iFSA.AgendaModule;
using Atos.iFSA.AgendaModule.Objects;
using Atos.iFSA.Data;
using Atos.iFSA.Objects;
using Atos.iFSA.ReplicationModule;

namespace Atos.iFSA.LoginModule
{
	public sealed class LoginScreenViewModel : ScreenViewModel
	{
		private readonly DateTime _dataDate = DateTime.Today;

		private User[] _users;
		private User _dataUser;
		private Task<List<AgendaOutlet>> _dataLoader;

		public INavigationService NavigationService => this.MainContext.GetService<INavigationService>();

		public string Title { get; }
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

		public LoginScreenViewModel(MainContext mainContext) : base(mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.Title = this.GetLocalized(@"Title");
			this.UsernameCaption = this.GetLocalized(@"Name");
			this.PasswordCaption = this.GetLocalized(@"Password");
			this.LoginCaption = this.GetLocalized(@"Login");
			this.AdvancedCaption = this.GetLocalized(@"Advanced");

			this.AdvancedCommand = new RelayCommand(this.Advanced);
			this.LoginCommand = new RelayCommand(this.Login);
		}

		public override Task InitializeAsync(object parameter)
		{
			try
			{
				// Load all users
				using (var ctx = this.MainContext.DbContextCreator())
				{
					_users = this.MainContext.GetService<IUserDataProvider>().GetUsers(ctx);
					ctx.Complete();
				}

				// Prepopulate with last successfully logged-in user
				var userSettings = this.GetUserSettings();
				if (userSettings != null)
				{
					this.Username = userSettings.Username;

					// Find the user for which to load the data
					foreach (var user in _users)
					{
						if (user.Name.Equals(this.Username, StringComparison.OrdinalIgnoreCase))
						{
							_dataUser = user;
							break;
						}
					}
				}

				// Load data(agenda) in background while the user is typing the password
				if (_dataUser != null)
				{
					_dataLoader = Task.Run(() =>
					{
						List<AgendaOutlet> outlets;

						using (var ctx = this.MainContext.CreateFeatureContext())
						{
							var dataProvider = this.MainContext.GetService<IAgendaDataProvider>();
							outlets = dataProvider.GetAgendaOutlets(ctx, _dataUser, _dataDate);
							ctx.Complete();
						}

						return outlets;
					});
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}

			return base.InitializeAsync(parameter);
		}

		private async void Login()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Login));
			try
			{
				this.MainContext.Save(feature);

				var username = this.GetCurrentUsername();
				var password = (this.Password ?? string.Empty);
				var user = default(User);

				foreach (var u in _users)
				{
					if (u.Name.Equals(username, StringComparison.OrdinalIgnoreCase) &&
						u.Password.Equals(password))
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

				this.SaveLoginSuccess(username);

				var outlets = default(List<AgendaOutlet>);
				if (_dataLoader != null)
				{
					// Check if the logged user is the same as the user the data is loaded in
					if (user.Id == _dataUser.Id)
					{
						// wait while the data is loaded
						outlets = await _dataLoader;
					}
				}
				await this.NavigationService.NavigateToAsync<AgendaScreenViewModel>(new AgendaDay(user, _dataDate, outlets));
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private async void Advanced()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Advanced));
			try
			{
				this.MainContext.Save(feature);

				var username = GetCurrentUsername();
				if (username == string.Empty)
				{
					var userSettings = this.GetUserSettings();
					if (userSettings != null)
					{
						username = userSettings.Username;
					}
				}

				await this.NavigationService.NavigateToAsync<ReplicationScreenViewModel>(username);
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private string GetLocalized(string name)
		{
			return this.MainContext.GetLocalized(new LocalizationKey(nameof(LoginScreenViewModel), name));
		}

		private string GetCurrentUsername()
		{
			return (this.Username ?? string.Empty).Trim().ToUpperInvariant();
		}

		private UserSettings GetUserSettings()
		{
			return this.MainContext.GetService<IUserSettingsProvider>().GetValue(nameof(UserSettings)) as UserSettings;
		}

		private void SaveLoginSuccess(string username)
		{
			//Save the current username as the last successfully logged-in user
			var settingsProvider = this.MainContext.GetService<IUserSettingsProvider>();
			var userSettings = this.GetUserSettings() ?? new UserSettings();
			userSettings.Username = username;
			settingsProvider.Save(nameof(UserSettings), userSettings);
		}
	}
}