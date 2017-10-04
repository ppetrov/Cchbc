using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Localization;
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

		private User _dataUser;
		private User[] _users;
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
				using (var ctx = this.MainContext.CreateDataQueryContext())
				{
					_users = this.MainContext.GetService<IUserDataProvider>().GetUsers(ctx);
					ctx.Complete();
				}

				// Prepopulate with last successfully logged-in user
				var userSettings = this.GetUserSettings();
				if (userSettings != null)
				{
					this.Username = userSettings.Username;
				}

				// Find the user for which to load the data
				_dataUser = this.FindCurrentUser();
				if (_dataUser != null)
				{
					// Load agenda in background while the user is typing the password
					_dataLoader = Task.Run(() => GetAgendaOutlets(_dataUser, _dataDate));
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex);
			}

			return base.InitializeAsync(parameter);
		}

		private User FindCurrentUser()
		{
			foreach (var user in _users)
			{
				if (user.Name.Equals(this.Username, StringComparison.OrdinalIgnoreCase))
				{
					return user;
				}
			}
			return null;
		}

		private List<AgendaOutlet> GetAgendaOutlets(User user, DateTime dataDate)
		{
			List<AgendaOutlet> outlets = null;

			try
			{
				using (var ctx = this.MainContext.CreateDataQueryContext())
				{
					var dataProvider = this.MainContext.GetService<IAgendaDataProvider>();
					outlets = dataProvider.GetAgendaOutlets(ctx, user, dataDate);
					ctx.Complete();
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex);
			}

			return outlets ?? new List<AgendaOutlet>(0);
		}

		private async void Login()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Login));
			try
			{
				this.MainContext.Save(feature);

				var username = this.GetCurrentUsername();
				var password = (this.Password ?? string.Empty);

				var user = FindCurrentUser();
				if (user != null && !user.Password.Equals(password))
				{
					user = null;
				}
				if (user == null)
				{
					await this.MainContext.ShowMessageAsync(GetLocalizationKey(@"WrongCredentials"));
					return;
				}

				this.SaveLoginSuccess(username);

				var outlets = default(List<AgendaOutlet>);

				// Check if the logged user is the same as the user the data is loaded in
				if (_dataLoader != null && user.Id == _dataUser.Id)
				{
					// wait while the data is loaded				
					outlets = await _dataLoader;
				}
				outlets = outlets ?? GetAgendaOutlets(user, _dataDate);

				await this.NavigationService.NavigateToAsync<AgendaScreenViewModel>(new AgendaDay(user, _dataDate, outlets));
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
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
				this.MainContext.Save(feature, ex);
			}
		}

		private string GetLocalized(string name)
		{
			return this.MainContext.GetLocalized(GetLocalizationKey(name));
		}

		private static LocalizationKey GetLocalizationKey(string name)
		{
			return new LocalizationKey(nameof(LoginScreenViewModel), name);
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