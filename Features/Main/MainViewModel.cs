using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Features.Diary.Views;
using ChatBotClient.Features.PrivacyPolicy.Views;
using ChatBotClient.Features.Services;
using ChatBotClient.Features.Settings.Views;
using ChatBotClient.Features.Tree.Views;
using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatBotClient.Features.Main
{
	public partial class MainViewModel : ObservableObject
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly NavigationService _navigationService;
		private readonly NotificationSettingsViewModel _notificationSettings;
		private readonly AnalyticsService _analyticsService;
		private readonly LocalStorageService _localStorageService;
		private bool _isNavMenuVisible;
		private bool _isMaximized;
		private ObservableCollection<string> _notifications;
		private int _totalPoints;

		public bool IsNavMenuVisible
		{
			get => _isNavMenuVisible;
			set => SetProperty(ref _isNavMenuVisible, value);
		}

		public bool IsMaximized
		{
			get => _isMaximized;
			set => SetProperty(ref _isMaximized, value);
		}

		public ObservableCollection<string> Notifications
		{
			get => _notifications;
			set => SetProperty(ref _notifications, value);
		}

		public bool HasNotifications => Notifications != null && Notifications.Count > 0;

		public int TotalPoints
		{
			get => _totalPoints;
			set => SetProperty(ref _totalPoints, value);
		}

		public MainViewModel(IServiceProvider serviceProvider, NavigationService navigationService)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
			_notificationSettings = serviceProvider.GetService<NotificationSettingsViewModel>() ?? throw new InvalidOperationException("NotificationSettingsViewModel not registered");
			_analyticsService = serviceProvider.GetService<AnalyticsService>() ?? throw new InvalidOperationException("AnalyticsService not registered");
			_localStorageService = serviceProvider.GetService<LocalStorageService>() ?? throw new InvalidOperationException("LocalStorageService not registered");
			Notifications = new ObservableCollection<string>();
			InitializeAsync();
			Log.Information("MainViewModel initialized");
		}

		[RelayCommand]
		private async Task InitializeAsync()
		{
			try
			{
				var (userIds, _) = _localStorageService.LoadUserData();
				var userId = userIds?.Count > 0 ? userIds[0] : null;
				if (string.IsNullOrEmpty(userId))
				{
					AddNotification("Требуется авторизация");
					NavigateToLogin();
					return;
				}
				TotalPoints = await _analyticsService.GetTotalPointsAsync(userId);
				NavigateToChat();
				Log.Information("MainViewModel initialized for user {UserId}", userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize MainViewModel");
				AddNotification($"Ошибка инициализации: {ex.Message}");
			}
		}

		[RelayCommand]
		private void NavigateToChat()
		{
			_navigationService.NavigateTo<ChatPage>();
			IsNavMenuVisible = false;
			Log.Information("Navigated to ChatPage");
		}

		[RelayCommand]
		private void NavigateToDiary()
		{
			_navigationService.NavigateTo<DiaryPage>();
			IsNavMenuVisible = false;
			Log.Information("Navigated to DiaryPage");
		}

		[RelayCommand]
		private void NavigateToSettings()
		{
			_navigationService.NavigateTo<SettingsPage>();
			IsNavMenuVisible = false;
			Log.Information("Navigated to SettingsPage");
		}

		[RelayCommand]
		private void NavigateToTree()
		{
			_navigationService.NavigateTo<TreePage>();
			IsNavMenuVisible = false;
			Log.Information("Navigated to TreePage");
		}

		[RelayCommand]
		private void NavigateToPrivacyPolicy()
		{
			_navigationService.NavigateTo<PrivacyPolicyPage>();
			IsNavMenuVisible = false;
			Log.Information("Navigated to PrivacyPolicyPage");
		}

		[RelayCommand]
		private void ToggleNavMenu()
		{
			IsNavMenuVisible = !IsNavMenuVisible;
			Log.Information("Navigation menu visibility changed to {IsVisible}", IsNavMenuVisible);
		}

		[RelayCommand]
		private void Minimize()
		{
			System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
			Log.Information("Window minimized");
		}

		[RelayCommand]
		private void MaximizeRestore()
		{
			System.Windows.Application.Current.MainWindow.WindowState = IsMaximized ? WindowState.Normal : WindowState.Maximized;
			IsMaximized = !IsMaximized;
			Log.Information("Window state changed to {State}", System.Windows.Application.Current.MainWindow.WindowState);
		}

		[RelayCommand]
		private void Close()
		{
			System.Windows.Application.Current.Shutdown();
			Log.Information("Application closed");
		}

		public void NavigateToLogin()
		{
			AddNotification("Требуется вход в систему");
			Log.Warning("Navigated to Login (placeholder)");
			// Здесь должна быть навигация на страницу логина, если реализована
		}

		private void AddNotification(string message)
		{
			Notifications.Add($"{DateTime.Now:HH:mm:ss}: {message}");
			OnPropertyChanged(nameof(HasNotifications));
			_notificationSettings?.NotifyAsync("Notification received");
			Log.Information("Added notification: {Message}", message);
		}
	}
}