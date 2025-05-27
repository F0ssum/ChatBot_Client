using ChatBotClient.Features.Chat.Views;
using ChatBotClient.Features.Diary.Views;
using ChatBotClient.Features.Services;
using ChatBotClient.Features.Settings.Views;
using ChatBotClient.Features.Tree.Views;
using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
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
		private bool _isWelcomeModalVisible;
		private string _username;
		private string _avatarPath;
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

		public bool IsWelcomeModalVisible
		{
			get => _isWelcomeModalVisible;
			set => SetProperty(ref _isWelcomeModalVisible, value);
		}

		public string Username
		{
			get => _username;
			set => SetProperty(ref _username, value);
		}

		public string AvatarPath
		{
			get => _avatarPath;
			set => SetProperty(ref _avatarPath, value);
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
			Log.Information("MainViewModel initialized");
		}

		[RelayCommand]
		public async Task InitializeAsync()
		{
			try
			{
				var (userIds, _) = _localStorageService.LoadUserData();
				var userId = userIds?.Count > 0 ? userIds[0] : null;
				if (string.IsNullOrEmpty(userId))
				{
					IsWelcomeModalVisible = true;
					AddNotification("Добро пожаловать! Укажите имя пользователя.");
					Log.Information("First-time user detected, showing welcome modal");
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
		private void SelectAvatar()
		{
			try
			{
				var openFileDialog = new OpenFileDialog
				{
					Title = "Выберите аватар",
					Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
				};
				if (openFileDialog.ShowDialog() == true)
				{
					AvatarPath = openFileDialog.FileName;
					Log.Information("Avatar selected: {AvatarPath}", AvatarPath);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to select avatar");
				AddNotification($"Ошибка выбора аватара: {ex.Message}");
			}
		}

		[RelayCommand]
		private async Task SaveWelcomeAsync()
		{
			if (string.IsNullOrWhiteSpace(Username))
			{
				AddNotification("Имя пользователя не может быть пустым.");
				Log.Warning("Attempted to save welcome without username");
				return;
			}

			try
			{
				// Create UserId via LocalStorageService
				string userId = await _localStorageService.CreateUserIdAsync(Username);

				// Save avatar if selected
				if (!string.IsNullOrEmpty(AvatarPath))
				{
					await _localStorageService.SaveAvatarAsync(userId, AvatarPath);
				}

				IsWelcomeModalVisible = false;
				TotalPoints = await _analyticsService.GetTotalPointsAsync(userId);
				NavigateToChat();
				await _notificationSettings.NotifyAsync("Профиль создан!");
				Log.Information("User profile created with UserId: {UserId}", userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save welcome data");
				AddNotification($"Ошибка сохранения профиля: {ex.Message}");
			}
		}

		[RelayCommand]
		private void CancelWelcome()
		{
			Username = string.Empty;
			AvatarPath = null;
			IsWelcomeModalVisible = false;
			AddNotification("Создание профиля отменено. Требуется авторизация.");
			Log.Information("Welcome modal cancelled");
			NavigateToLogin();
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
			IsWelcomeModalVisible = true; // Reopen welcome modal instead of login
			AddNotification("Требуется создать профиль");
			Log.Information("Navigated to Welcome modal (login)");
		}

		private void AddNotification(string message)
		{
			Notifications.Add($"{DateTime.Now:HH:mm:ss}: {message}");
			OnPropertyChanged(nameof(HasNotifications));
			_notificationSettings?.NotifyAsync(message);
			Log.Information("Notification added: {Message}", message);
		}
	}
}