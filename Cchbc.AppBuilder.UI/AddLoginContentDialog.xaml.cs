using System;
using Cchbc.AppBuilder.UI.ViewModels;
using Cchbc.Common;
using Cchbc.Features;
using Cchbc.Objects;

namespace Cchbc.AppBuilder.UI
{
	public sealed partial class AddLoginContentDialog
	{
		public AddLoginViewModel ViewModel { get; }

		public AddLoginContentDialog(LoginsViewModel viewModel, Action<Login> acceptAction, Action<Login> cancelAction)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (acceptAction == null) throw new ArgumentNullException(nameof(acceptAction));
			if (cancelAction == null) throw new ArgumentNullException(nameof(cancelAction));

			this.InitializeComponent();

			this.DataContext = this;

			this.ViewModel = new AddLoginViewModel(viewModel,
				_ =>
			{
				this.Hide();
				acceptAction(_);
			}, _ =>
			{
				this.Hide();
				cancelAction(_);
			});
		}
	}

	public sealed class AddLoginViewModel : ViewModel
	{
		public LoginsViewModel ViewModel { get; set; }
		public RelayCommand CreateCommand { get; }
		public RelayCommand CancelCommand { get; }

		public string Caption { get; }
		public string CreateCaption { get; }
		public string CancelCaption { get; }
		public string NameCaption { get; }
		public string PasswordCaption { get; }

		private string _name = string.Empty;
		public string Name
		{
			get { return _name; }
			set
			{
				this.SetField(ref _name, value);
				this.CreateCommand.RaiseCanExecuteChanged();
			}
		}

		private string _nameValidationError = string.Empty;
		public string NameValidationError
		{
			get { return _nameValidationError; }
			private set { this.SetField(ref _nameValidationError, value); }
		}

		private string _password = string.Empty;
		public string Password
		{
			get { return _password; }
			set
			{
				this.SetField(ref _password, value);
				this.CreateCommand.RaiseCanExecuteChanged();
			}
		}

		private string _passwordValidationError = string.Empty;
		public string PasswordValidationError
		{
			get { return _passwordValidationError; }
			private set { this.SetField(ref _passwordValidationError, value); }
		}

		public AddLoginViewModel(LoginsViewModel viewModel, Action<Login> acceptAction, Action<Login> cancelAction)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (acceptAction == null) throw new ArgumentNullException(nameof(acceptAction));
			if (cancelAction == null) throw new ArgumentNullException(nameof(cancelAction));

			this.ViewModel = viewModel;
			this.CreateCommand = new RelayCommand(() =>
			{
				acceptAction(new Login(0, this.Name, this.Password, DateTime.Now, false));
			}, this.CanCreate);
			this.CancelCommand = new RelayCommand(() =>
			{
				cancelAction(null);
			});

			this.Caption = @"Add Login";
			this.CreateCaption = @"Create";
			this.CancelCaption = @"Cancel";
			this.NameCaption = @"Name";
			this.PasswordCaption = @"Password";
		}

		private bool CanCreate()
		{
			var canCreate = true;
			var nameValidation = string.Empty;
			var passwordValidation = string.Empty;

			var loginViewModel = new LoginViewModel(new Login(0, this.Name, this.Password, DateTime.Now, false));

			foreach (var result in this.ViewModel.Module.ValidateProperties(loginViewModel, Feature.None))
			{
				canCreate = false;

				var property = result.Property;
				switch (property)
				{
					case nameof(Login.Name):
						nameValidation = result.ErrorMessage;
						break;
					case nameof(Login.Password):
						passwordValidation = result.ErrorMessage;
						break;
				}
			}

			this.NameValidationError = nameValidation;
			this.PasswordValidationError = passwordValidation;

			return canCreate;
		}
	}
}
