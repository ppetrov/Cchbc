using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Logs;
using iFSA.Common.Objects;
using iFSA.LoginModule.Objects;
using iFSA.LoginModule.ViewModels;
using iFSA.ReplicationModule.Objects;

namespace iFSA.LoginModule
{
	public sealed class LoginScreenViewModel : ViewModel
	{
		private string GetCustomized(string name)
		{
			return this.MainContext.LocalizationManager.Get(new LocalizationKey(@"LoginScreen", name));
		}

		private MainContext MainContext { get; }
		private IAppNavigator AppNavigator { get; }
		private LoginScreenDataProvider DataProvider { get; }

		private List<User> Users { get; set; }
		private List<Visit> Visits { get; set; }

		public string NameCaption { get; }
		public string PasswordCaption { get; }
		public string LoginCaption { get; }
		public string AdvancedCaption { get; }

		private string _progress = string.Empty;
		public string Progress
		{
			get { return _progress; }
			set { this.SetProperty(ref _progress, value); }
		}
		private bool _isLoadingData;
		public bool IsLoadingData
		{
			get { return _isLoadingData; }
			set { this.SetProperty(ref _isLoadingData, value); }
		}
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
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));

			this.DataProvider = dataProvider;
			this.MainContext = mainContext;
			this.AppNavigator = appNavigator;

			this.NameCaption = this.GetCustomized(@"Name");
			this.PasswordCaption = this.GetCustomized(@"Password");
			this.LoginCaption = this.GetCustomized(@"Login");
			this.AdvancedCaption = this.GetCustomized(@"Advanced");

			this.Systems.Add(new SystemViewModel(new AppSystem(this.GetCustomized(@"Production"), SystemSource.Production)));
			this.Systems.Add(new SystemViewModel(new AppSystem(this.GetCustomized(@"Quality"), SystemSource.Quality)));
			this.Systems.Add(new SystemViewModel(new AppSystem(this.GetCustomized(@"Development"), SystemSource.Development)));

			foreach (var country in this.DataProvider.CountriesProvider())
			{
				this.Countries.Add(new CountryViewModel(country));
			}

			this.AdvancedCommand = new RelayCommand(this.Advanced);
			this.LoginCommand = new RelayCommand(this.Login);
		}

		public Task LoadDataAsync()
		{
			this.IsLoadingData = true;
			this.Progress = this.GetCustomized(@"LoadData");

			var userSettings = this.DataProvider.UserSettingsProvider.Load();
			if (userSettings != null)
			{
				this.Username = userSettings.User.Name;
			}
			this.Users = this.DataProvider.UsersProvider(this.MainContext);

			if (this.Users.Count == 0) return Task.CompletedTask;

			var currentUser = this.Users[0];

			if (this.Username != string.Empty)
			{
				foreach (var login in this.Users)
				{
					if (login.Name.Equals(this.Username, StringComparison.OrdinalIgnoreCase))
					{
						currentUser = login;
						break;
					}
				}
			}

			return Task.Run(() =>
			{
				var feature = Feature.StartNew(nameof(LoginScreenViewModel), nameof(LoadDataAsync));
				try
				{
					this.Visits = this.DataProvider.VisitsProvider(this.MainContext, currentUser, DateTime.Today);
				}
				catch (Exception ex)
				{
					this.MainContext.Log(ex.ToString(), LogLevel.Error);
				}
				finally
				{
					this.MainContext.FeatureManager.Save(feature);
				}
			}).ContinueWith(t =>
			{
				this.Progress = this.GetCustomized(@"DataLoadCompleted");
				this.IsLoadingData = false;
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private async void Login()
		{
			var feature = Feature.StartNew(nameof(LoginScreenViewModel), nameof(Login));
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
						var userSettings = this.DataProvider.UserSettingsProvider.Load();
						if (userSettings != null)
						{
							userSettings.User = user;
							this.DataProvider.UserSettingsProvider.Save(userSettings);
						}

						// Wait while the data is loaded
						while (this.IsLoadingData)
						{
							await Task.Delay(TimeSpan.FromMilliseconds(25));
						}

						// Display agenda with the list of visits
						this.AppNavigator.NavigateTo(AppScreen.Agenda, this.Visits);
					}
					else
					{
						await this.MainContext.ModalDialog.ShowAsync(this.GetCustomized(@"WrongCredentials"), feature);
					}
					return;
				}

				var systemViewModel = this.SelectedSystem;
				if (systemViewModel == null)
				{
					await this.MainContext.ModalDialog.ShowAsync(this.GetCustomized(@"NoSystemSelected"), feature);
					return;
				}
				var countryViewModel = this.SelectedCountry;
				if (countryViewModel == null)
				{
					await this.MainContext.ModalDialog.ShowAsync(this.GetCustomized(@"NoCountrySelected"), feature);
					return;
				}

				var system = systemViewModel.AppSystem;
				var country = countryViewModel.Country;
				var config = await this.DataProvider.ReplicationConfigProvider(system, country);
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
							var userSettings = this.DataProvider.UserSettingsProvider.Load();
							if (userSettings != null)
							{
								userSettings.ReplicationConfig = config;
								this.DataProvider.UserSettingsProvider.Save(userSettings);
							}

							await LoadDataAsync();

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
			var feature = Feature.StartNew(nameof(LoginScreenViewModel), nameof(Advanced));
			try
			{
				var config = new ReplicationConfig(string.Empty, 0);
				var userSettings = this.DataProvider.UserSettingsProvider.Load();
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
	}
}