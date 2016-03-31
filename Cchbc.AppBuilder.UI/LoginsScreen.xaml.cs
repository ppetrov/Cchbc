using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Cchbc.AppBuilder.UI.ViewModels;

namespace Cchbc.AppBuilder.UI
{
	public sealed partial class LoginsScreen
	{
		public LoginsViewModel ViewModel { get; } = new LoginsViewModel(Context.Core, new LoginAdapter());

		public LoginsScreen()
		{
			this.InitializeComponent();
		}

		private async void LoginsScreenLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.ViewModel.LoadDataAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		private async void AddLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var dialog = new AddLoginContentDialog(this.ViewModel, this.ViewModel.Add, _ => { });
			await dialog.ShowAsync();
		}
	}

	public sealed class Context
	{
		public static Core Core { get; } = new Core { ModalDialog = new ModalDialog() };


	}

}
