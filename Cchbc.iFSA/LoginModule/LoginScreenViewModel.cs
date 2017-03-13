using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc.Common;
using Cchbc.Features;
using Cchbc.iFSA.LoginModule.Objects;
using Cchbc.iFSA.LoginModule.ViewModels;
using Cchbc.iFSA.Objects;
using Cchbc.iFSA.ReplicationModule.Objects;

namespace Cchbc.iFSA.LoginModule
{
	public sealed class LoginScreenViewModel : ViewModel
	{
		private List<User> Users { get; set; }
		private List<Visit> Visits { get; set; }

		private AppContext AppContext { get; }
		private IAppNavigator AppNavigator { get; }

		// TODO : Unify this into a separate class
		private IUserSettingsProvider UserSettingsProvider { get; }
		private Func<AppContext, List<User>> UsersProvider { get; }
		private Func<AppContext, User, DateTime, List<Visit>> VisitsProvider { get; }
		private Func<AppSystem, Country, Task<ReplicationConfig>> ReplicationConfigProvider { get; }

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

		public LoginScreenViewModel(AppContext appContext, IAppNavigator appNavigator, Func<IEnumerable<Country>> countriesProvider, Func<AppContext, List<User>> usersProvider, Func<UserSettings> userSettingsProvider, Action<UserSettings> userSettingsSaver)
		{
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));
			if (countriesProvider == null) throw new ArgumentNullException(nameof(countriesProvider));

			this.AppNavigator = appNavigator;
			this.UsersProvider = usersProvider;
			this.AppContext = appContext;

			// TODO : Customize
			this.NameCaption = @"Name";
			this.PasswordCaption = @"Password";
			this.LoginCaption = @"Login";
			this.AdvancedCaption = @"Advanced";

			// TODO : Customize
			this.Systems.Add(new SystemViewModel(new AppSystem(@"Production", SystemSource.Production)));
			this.Systems.Add(new SystemViewModel(new AppSystem(@"Quality", SystemSource.Quality)));
			this.Systems.Add(new SystemViewModel(new AppSystem(@"Development", SystemSource.Development)));

			foreach (var country in countriesProvider())
			{
				this.Countries.Add(new CountryViewModel(country));
			}

			this.AdvancedCommand = new RelayCommand(this.AdvancedAction);
			this.LoginCommand = new RelayCommand(this.LoginAction);
		}

		public string Context => @"Login";

		public Task LoadDataAsync()
		{
			// TODO : !!! Customize
			this.Progress = @"Load data";
			this.IsLoadingData = true;

			var userSettings = this.UserSettingsProvider.Load();
			if (userSettings != null)
			{
				var user = userSettings.User;
				this.Username = user.Name;
				this.Password = user.Password;
			}
			this.Users = this.UsersProvider(this.AppContext);

			if (this.Users.Count > 0)
			{
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
					var feature = Feature.StartNew(this.Context, @"LoadData");

					this.Visits = this.VisitsProvider(this.AppContext, currentUser, DateTime.Today);

					this.AppContext.FeatureManager.Write(feature);
				}).ContinueWith(t =>
				{
					// TODO : !!! Customize
					this.Progress = @"Completed";
					this.IsLoadingData = false;
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
			return Task.FromResult(true);
		}

		private async void LoginAction()
		{
			var username = (this.Username ?? string.Empty).Trim();
			var password = (this.Password ?? string.Empty);

			if (this.Users.Count > 0)
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
					// Load & Save the current Login
					var userSettings = this.UserSettingsProvider.Load();
					if (userSettings != null)
					{
						userSettings.User = user;
						this.UserSettingsProvider.Save(userSettings);
					}

					// Wait while the data is loaded
					while (this.IsLoadingData)
					{
						await Task.Delay(TimeSpan.FromMilliseconds(100));
					}

					// We can load agenda with a date or with a list of visits
					this.AppNavigator.NavigateTo(AppScreen.Agenda, this.Visits);
				}
				else
				{
					// TODO : Customize
					await this.AppContext.ModalDialog.ShowAsync(@"Wrong user/pass", Feature.None);
				}
				return;
			}

			var systemViewModel = this.SelectedSystem;
			if (systemViewModel == null)
			{
				// TODO : Customize
				await this.AppContext.ModalDialog.ShowAsync(@"No AppSystem selected", Feature.None);
				return;
			}
			var countryViewModel = this.SelectedCountry;
			if (countryViewModel == null)
			{
				// TODO : Customize
				await this.AppContext.ModalDialog.ShowAsync(@"No Country selected", Feature.None);
				return;
			}

			var config = await this.ReplicationConfigProvider(systemViewModel.AppSystem, countryViewModel.Country);
			var settings = new ReplicationSettings(config, new Login(this.Username, this.Password));

			// TODO : Replicate !!!
		}

		private void AdvancedAction()
		{
			var config = new ReplicationConfig(string.Empty, 0);
			var userSettings = this.UserSettingsProvider.Load();
			if (userSettings != null)
			{
				config = userSettings.ReplicationConfig;
			}
			var settings = new ReplicationSettings(config, new Login(this.Username, this.Password));
			this.AppNavigator.NavigateTo(AppScreen.Replication, settings);
		}
	}
}