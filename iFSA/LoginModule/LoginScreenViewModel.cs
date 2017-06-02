using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.Client.Validation;
using iFSA.AgendaModule;
using iFSA.Common.Objects;
using iFSA.LoginModule.Data;
using iFSA.LoginModule.Objects;
using iFSA.LoginModule.ViewModels;
using iFSA.ReplicationModule.Objects;

namespace iFSA.LoginModule
{
	public sealed class LoginScreenViewModel : ViewModel
	{
		private MainContext MainContext { get; }
		private IAppNavigator AppNavigator { get; }
		private LoginScreenDataProvider DataProvider { get; }
		private List<User> Users { get; } = new List<User>();

		public string NameCaption { get; }
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

		private SystemViewModel _selectedSystem;
		public SystemViewModel SelectedSystem
		{
			get { return _selectedSystem; }
			set { this.SetProperty(ref _selectedSystem, value); }
		}
		public ObservableCollection<SystemViewModel> Systems { get; } = new ObservableCollection<SystemViewModel>();

		private CountryViewModel _selectedCountry;
		public CountryViewModel SelectedCountry
		{
			get { return _selectedCountry; }
			set { this.SetProperty(ref _selectedCountry, value); }
		}
		public ObservableCollection<CountryViewModel> Countries { get; } = new ObservableCollection<CountryViewModel>();

		public LoginScreenViewModel(MainContext mainContext, IAppNavigator appNavigator, LoginScreenDataProvider dataProvider)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.MainContext = mainContext;
			this.AppNavigator = appNavigator;
			this.DataProvider = dataProvider;

			this.NameCaption = this.GetLocalizedValue(@"Name");
			this.PasswordCaption = this.GetLocalizedValue(@"Password");
			this.LoginCaption = this.GetLocalizedValue(@"Login");
			this.AdvancedCaption = this.GetLocalizedValue(@"Advanced");

			this.Systems.Add(new SystemViewModel(new AppSystem(this.GetLocalizedValue(@"Production"), SystemSource.Production)));
			this.Systems.Add(new SystemViewModel(new AppSystem(this.GetLocalizedValue(@"Quality"), SystemSource.Quality)));
			this.Systems.Add(new SystemViewModel(new AppSystem(this.GetLocalizedValue(@"Development"), SystemSource.Development)));

			foreach (var country in this.DataProvider.GetCountries())
			{
				this.Countries.Add(new CountryViewModel(country));
			}

			this.AdvancedCommand = new RelayCommand(this.Advanced);
			this.LoginCommand = new RelayCommand(this.Login);
		}

		public void LoadData()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(LoadData));
			try
			{
				this.MainContext.FeatureManager.Save(feature);

				var userSettings = this.DataProvider.GetUserSettings();
				if (userSettings != null)
				{
					this.Username = userSettings.User.Name;
				}
				using (var ctx = new FeatureContext(this.MainContext, feature))
				{
					this.Users.Clear();
					this.Users.AddRange(this.DataProvider.GetUsers(ctx));
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
				var username = (this.Username ?? string.Empty).Trim();
				var password = (this.Password ?? string.Empty);

				var hasUsers = this.Users.Count > 0;
				if (hasUsers)
				{
					var user = default(User);

					foreach (var current in this.Users)
					{
						if (username.Equals(current.Name, StringComparison.OrdinalIgnoreCase) &&
							password.Equals(current.Password))
						{
							user = current;
							break;
						}
					}

					if (user != null)
					{
						// Save the current username
						var userSettings = this.DataProvider.GetUserSettings();
						if (userSettings != null)
						{
							userSettings.User = user;
							this.DataProvider.SaveUserSettings(userSettings);
						}

						// Display agenda
						this.AppNavigator.NavigateTo(AppScreen.Agenda, new AgendaScreenParam(user, DateTime.Today));
					}
					else
					{
						await this.MainContext.ModalDialog.ShowAsync(this.GetLocalizationMessage(@"WrongCredentials"));
					}
					return;
				}

				var systemViewModel = this.SelectedSystem;
				if (systemViewModel == null)
				{
					await this.MainContext.ModalDialog.ShowAsync(this.GetLocalizationMessage(@"NoSystemSelected"));
					return;
				}
				var countryViewModel = this.SelectedCountry;
				if (countryViewModel == null)
				{
					await this.MainContext.ModalDialog.ShowAsync(this.GetLocalizationMessage(@"NoCountrySelected"));
					return;
				}

				var system = systemViewModel.AppSystem;
				var country = countryViewModel.Country;

				var config = await this.DataProvider.GetReplicationConfig(system, country);
				if (config != null)
				{
					var login = new Login(this.Username, this.Password);
					var settings = new ReplicationSettings(config, login);

					// TODO : Replicate !!!	

					var success = true;
					if (success)
					{
						try
						{
							// Save the current replication config
							var userSettings = this.DataProvider.GetUserSettings();
							if (userSettings != null)
							{
								userSettings.ReplicationConfig = config;
								this.DataProvider.SaveUserSettings(userSettings);
							}

							this.LoadData();

							this.LoginCommand.Execute(null);
						}
						catch (Exception ex)
						{
							this.MainContext.Log(ex.ToString(), LogLevel.Error);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
		}

		private void Advanced()
		{
			var feature = new Feature(nameof(LoginScreenViewModel), nameof(Advanced));
			try
			{
				var config = new ReplicationConfig(string.Empty, 0);
				var userSettings = this.DataProvider.GetUserSettings();
				if (userSettings != null)
				{
					config = userSettings.ReplicationConfig;
				}
				var login = new Login(this.Username, this.Password);
				var settings = new ReplicationSettings(config, login);
				this.AppNavigator.NavigateTo(AppScreen.Replication, settings);
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
		}

		private string GetLocalizedValue(string name)
		{
			return this.MainContext.LocalizationManager.Get(new LocalizationKey(nameof(LoginScreenViewModel), name));
		}

		private PermissionResult GetLocalizationMessage(string name)
		{
			return PermissionResult.Deny(this.MainContext.LocalizationManager.Get(new LocalizationKey(nameof(LoginScreenViewModel), name)));
		}
	}
}