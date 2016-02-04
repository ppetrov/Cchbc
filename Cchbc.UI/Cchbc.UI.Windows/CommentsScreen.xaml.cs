﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Features.Db;

namespace Cchbc.UI
{
	public sealed partial class CommentsScreen
	{
		private readonly LoginsViewModel _viewModel;

		public CommentsScreen()
		{
			this.InitializeComponent();

			var core = new Core();
			core.FeatureManager = new FeatureManager(new DbFeaturesManager(new DbFeaturesAdapter(new QueryExecutor(null, null))));
			_viewModel = new LoginsViewModel(core);
			this.DataContext = _viewModel;
		}

		private void CommentsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_viewModel.LoadData();
			}
			catch (Exception ex)
			{
				
			}
		}

		private async void AddLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				try
				{
					await _viewModel.AddAsync(new LoginViewModel(new Login(2, @"ZDoctor@", @"123456789", DateTime.Now, false)), new ModalDialog());
				}
				catch (Exception ex)
				{

				}
			}
		}

		private async void DeleteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				var viewModel = btn.DataContext as LoginViewModel;
				if (viewModel != null)
				{
					var dialog = new ModalDialog();

					var r = await dialog.ShowAsync(@"Are you sure you want to delete this user?", Feature.None, DialogType.AcceptDecline);
					if (r == DialogResult.Accept)
					{
						await _viewModel.RemoveAsync(viewModel, dialog);
					}
				}
			}
		}

		private async void PromoteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				var viewModel = btn.DataContext as LoginViewModel;
				if (viewModel != null)
				{
					var dialog = new ModalDialog();
					var r = await dialog.ShowAsync(@"Are you sure you want to promote this user?", Feature.None, DialogType.AcceptDecline);
					if (r == DialogResult.Accept)
					{
						//await _viewModel.PromoteUserAsync(viewModel, dialog);
					}
				}
			}
		}

		private void UIElement_OnTapped3(object sender, TappedRoutedEventArgs e)
		{
			//var dialog = new WinRtModalDialog();
			//dialog.AcceptAction = async () => { await _viewModel.MarkAsync(dialog); };
			//await dialog.ShowAsync(@"Are you sure you want to mark as read ?");
		}
	}
}
