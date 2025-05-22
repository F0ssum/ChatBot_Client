using ChatBotClient.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.ViewModels
{
	public partial class MainViewModel : ObservableObject
	{
		private readonly IServiceProvider _serviceProvider;
		private Frame _mainFrame;

		[ObservableProperty]
		private Visibility isNavMenuVisible = Visibility.Collapsed;

		public MainViewModel(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public void InitializeNavigation(Main mainWindow)
		{
			try
			{
				Log.Information("Initializing navigation with Main window: {Window}", mainWindow.GetType().Name);
				_mainFrame = mainWindow.FindName("MainFrame") as Frame
					?? throw new InvalidOperationException("MainFrame not found in Main window");
				Log.Information("MainFrame found: {Frame}", _mainFrame.GetType().Name);
				_mainFrame.Navigate(_serviceProvider.GetRequiredService<ChatPage>());
				Log.Information("Navigated to ChatPage on startup");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to navigate to ChatPage on startup: {Message}", ex.Message);
				MessageBox.Show($"Ошибка навигации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void ToggleNavMenu()
		{
			try
			{
				IsNavMenuVisible = IsNavMenuVisible == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
				Log.Information("Toggled navigation menu visibility: {Visibility}", IsNavMenuVisible);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to toggle navigation menu: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void NavigateToChat()
		{
			try
			{
				_mainFrame.Navigate(_serviceProvider.GetRequiredService<ChatPage>());
				IsNavMenuVisible = Visibility.Collapsed;
				Log.Information("Navigated to ChatPage");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to navigate to ChatPage: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void NavigateToDiary()
		{
			try
			{
				_mainFrame.Navigate(_serviceProvider.GetRequiredService<DiaryPage>());
				IsNavMenuVisible = Visibility.Collapsed;
				Log.Information("Navigated to DiaryPage");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to navigate to DiaryPage: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void NavigateToSettings()
		{
			try
			{
				_mainFrame.Navigate(_serviceProvider.GetRequiredService<SettingsPage>());
				IsNavMenuVisible = Visibility.Collapsed;
				Log.Information("Navigated to SettingsPage");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to navigate to SettingsPage: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void Minimize()
		{
			try
			{
				Application.Current.MainWindow.WindowState = WindowState.Minimized;
				Log.Information("Window minimized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to minimize window: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void MaximizeRestore()
		{
			try
			{
				Application.Current.MainWindow.WindowState =
					Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
				Log.Information("Window maximized/restored");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to maximize/restore window: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void Close()
		{
			try
			{
				Application.Current.MainWindow.Close();
				Log.Information("Window closed");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to close window: {Message}", ex.Message);
			}
		}
	}
}