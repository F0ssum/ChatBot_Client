using ChatBotClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.Views
{
	public partial class SettingsPage : Page
	{
		private readonly SettingsViewModel _viewModel;

		public SettingsPage(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			try
			{
				_viewModel = serviceProvider.GetRequiredService<SettingsViewModel>();
				DataContext = _viewModel;
				Log.Information("SettingsPage initialized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize SettingsPage: {Message}", ex.Message);
				MessageBox.Show($"Failed to initialize settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}