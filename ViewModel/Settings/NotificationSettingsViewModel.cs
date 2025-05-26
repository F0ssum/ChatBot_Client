using ChatBotClient.Infrastructure.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Windows;

namespace ChatBotClient.ViewModel.Settings
{
	public partial class NotificationSettingsViewModel : ObservableObject
	{
		private readonly NotificationService _notificationService;

		// Настройки уведомлений
		private bool _notificationsEnabled = true;
		private int _notificationSoundIndex;
		private double _notificationVolume = 50;
		private int _reminderFrequency;
		private TimeSpan? _reminderTime = TimeSpan.FromHours(9);

		// ⬇️ Новые настройки для модели ИИ
		private int _langIndex;
		private string _maxResponseLength = "200";
		private string _customPrompt = "Ответь кратко и по делу.";
		private double _temperature = 0.7;
		private double _topP = 0.9;

		public bool NotificationsEnabled
		{
			get => _notificationsEnabled;
			set => SetProperty(ref _notificationsEnabled, value);
		}

		public int NotificationSoundIndex
		{
			get => _notificationSoundIndex;
			set => SetProperty(ref _notificationSoundIndex, value);
		}

		public double NotificationVolume
		{
			get => _notificationVolume;
			set => SetProperty(ref _notificationVolume, Math.Clamp(value, 0, 100));
		}

		public int ReminderFrequency
		{
			get => _reminderFrequency;
			set => SetProperty(ref _reminderFrequency, value);
		}

		public TimeSpan? ReminderTime
		{
			get => _reminderTime;
			set => SetProperty(ref _reminderTime, value);
		}

		// ⬇️ Новые свойства
		public int LangIndex
		{
			get => _langIndex;
			set => SetProperty(ref _langIndex, value);
		}

		public string MaxResponseLength
		{
			get => _maxResponseLength;
			set => SetProperty(ref _maxResponseLength, value);
		}

		public string CustomPrompt
		{
			get => _customPrompt;
			set => SetProperty(ref _customPrompt, value);
		}

		public double Temperature
		{
			get => _temperature;
			set => SetProperty(ref _temperature, Math.Clamp(value, 0.0, 1.0));
		}

		public double TopP
		{
			get => _topP;
			set => SetProperty(ref _topP, Math.Clamp(value, 0.0, 1.0));
		}

		public NotificationSettingsViewModel(NotificationService notificationService)
		{
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
			Log.Information("NotificationSettingsViewModel initialized");
		}

		[RelayCommand]
		private async Task TestNotificationAsync()
		{
			try
			{
				await _notificationService.ShowNotificationAsync("Тестовое уведомление", NotificationSoundIndex, (float)NotificationVolume / 100);
				Log.Information("Test notification sent");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to send test notification");
				MessageBox.Show($"Не удалось отправить тестовое уведомление: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async Task NotifyAsync(string message)
		{
			if (NotificationsEnabled)
			{
				await _notificationService.ShowNotificationAsync(message, NotificationSoundIndex, (float)NotificationVolume / 100);
			}
		}

		public void Reset()
		{
			NotificationsEnabled = true;
			NotificationSoundIndex = 0;
			NotificationVolume = 50;
			ReminderFrequency = 0;
			ReminderTime = TimeSpan.FromHours(9);
			LangIndex = 0;
			MaxResponseLength = "200";
			CustomPrompt = "Ответь кратко и по делу.";
			Temperature = 0.7;
			TopP = 0.9;

			Log.Information("NotificationSettingsViewModel reset");
		}
	}
}