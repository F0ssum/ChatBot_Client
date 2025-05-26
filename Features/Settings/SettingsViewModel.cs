using ChatBotClient.Infrastructure.Services;
using ChatBotClient.ViewModel.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;

namespace ChatBotClient.Features.Settings { 
	public partial class SettingsViewModel : ObservableObject
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ChatSettingsViewModel _chatSettings;
		private readonly DiarySettingsViewModel _diarySettings;
		private readonly ProfileSettingsViewModel _profileSettings;
		private readonly ModelSettingsViewModel _modelSettings;
		private readonly NotificationSettingsViewModel _notificationSettings;
		private readonly LocalStorageService _localStorageService;
		private bool _isChatTabSelected = true;
		private bool _isDiaryTabSelected;
		private bool _isProfileTabSelected;
		private bool _isModelTabSelected;
		private bool _isNotificationsTabSelected;
		private bool _isSaveSettingsModalVisible;
		private bool _isResetSettingsModalVisible;
		private bool _isAboutModalVisible;
		private bool _isBackupModalVisible;

		public ChatSettingsViewModel ChatSettings => _chatSettings;
		public DiarySettingsViewModel DiarySettings => _diarySettings;
		public ProfileSettingsViewModel ProfileSettings => _profileSettings;
		public ModelSettingsViewModel ModelSettings => _modelSettings;
		public NotificationSettingsViewModel NotificationSettings => _notificationSettings;

		public bool IsChatTabSelected
		{
			get => _isChatTabSelected;
			set
			{
				SetProperty(ref _isChatTabSelected, value);
				if (value)
				{
					IsDiaryTabSelected = false;
					IsProfileTabSelected = false;
					IsModelTabSelected = false;
					IsNotificationsTabSelected = false;
				}
			}
		}

		public bool IsDiaryTabSelected
		{
			get => _isDiaryTabSelected;
			set
			{
				SetProperty(ref _isDiaryTabSelected, value);
				if (value)
				{
					IsChatTabSelected = false;
					IsProfileTabSelected = false;
					IsModelTabSelected = false;
					IsNotificationsTabSelected = false;
				}
			}
		}

		public bool IsProfileTabSelected
		{
			get => _isProfileTabSelected;
			set
			{
				SetProperty(ref _isProfileTabSelected, value);
				if (value)
				{
					IsChatTabSelected = false;
					IsDiaryTabSelected = false;
					IsModelTabSelected = false;
					IsNotificationsTabSelected = false;
				}
			}
		}

		public bool IsModelTabSelected
		{
			get => _isModelTabSelected;
			set
			{
				SetProperty(ref _isModelTabSelected, value);
				if (value)
				{
					IsChatTabSelected = false;
					IsDiaryTabSelected = false;
					IsProfileTabSelected = false;
					IsNotificationsTabSelected = false;
				}
			}
		}

		public bool IsNotificationsTabSelected
		{
			get => _isNotificationsTabSelected;
			set
			{
				SetProperty(ref _isNotificationsTabSelected, value);
				if (value)
				{
					IsChatTabSelected = false;
					IsDiaryTabSelected = false;
					IsProfileTabSelected = false;
					IsModelTabSelected = false;
				}
			}
		}

		public bool IsSaveSettingsModalVisible
		{
			get => _isSaveSettingsModalVisible;
			set => SetProperty(ref _isSaveSettingsModalVisible, value);
		}

		public bool IsResetSettingsModalVisible
		{
			get => _isResetSettingsModalVisible;
			set => SetProperty(ref _isResetSettingsModalVisible, value);
		}

		public bool IsAboutModalVisible
		{
			get => _isAboutModalVisible;
			set => SetProperty(ref _isAboutModalVisible, value);
		}

		public bool IsBackupModalVisible
		{
			get => _isBackupModalVisible;
			set => SetProperty(ref _isBackupModalVisible, value);
		}

		public SettingsViewModel(ChatSettingsViewModel chatSettings, DiarySettingsViewModel diarySettings,
			ProfileSettingsViewModel profileSettings, ModelSettingsViewModel modelSettings,
			NotificationSettingsViewModel notificationSettings, LocalStorageService localStorageService,
			IServiceProvider serviceProvider)
		{
			_chatSettings = chatSettings ?? throw new ArgumentNullException(nameof(chatSettings));
			_diarySettings = diarySettings ?? throw new ArgumentNullException(nameof(diarySettings));
			_profileSettings = profileSettings ?? throw new ArgumentNullException(nameof(profileSettings));
			_modelSettings = modelSettings ?? throw new ArgumentNullException(nameof(modelSettings));
			_notificationSettings = notificationSettings ?? throw new ArgumentNullException(nameof(notificationSettings));
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Log.Information("SettingsViewModel initialized");
		}

		[RelayCommand]
		private void SaveSettings()
		{
			IsSaveSettingsModalVisible = true;
			Log.Information("Opened save settings modal");
		}

		[RelayCommand]
		private async Task ConfirmSaveSettingsAsync()
		{
			try
			{
				_localStorageService.SaveData(new
				{
					ChatSettings = new
					{
						_chatSettings.ThemeIndex,
						_chatSettings.BotName
					},
					DiarySettings = new
					{
						_diarySettings.DiaryNoteStyleIndex
					},
					ProfileSettings = new
					{
						_profileSettings.Username,
						_profileSettings.AvatarPath
					},
					ModelSettings = new
					{
						_modelSettings.CommunicationStyleIndex,
						_modelSettings.EmpathyLevel,
						_modelSettings.ToneIndex,
						_modelSettings.IsLocalModel,
						_modelSettings.IsServerModel,
						_modelSettings.Temperature,
						_modelSettings.TopP,
						_modelSettings.MaxResponseLength,
						_modelSettings.CustomPrompt
					},
					NotificationSettings = new
					{
						_notificationSettings.NotificationsEnabled,
						_notificationSettings.NotificationSoundIndex,
						_notificationSettings.NotificationVolume,
						_notificationSettings.ReminderFrequency,
						_notificationSettings.ReminderTime
					}
				});
				var apiService = _serviceProvider.GetService<ApiService>();
				apiService?.SetModelMode(_modelSettings.IsLocalModel);
				IsSaveSettingsModalVisible = false;
				await _notificationSettings.NotifyAsync("Настройки сохранены");
				Log.Information("Settings saved");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save settings");
				MessageBox.Show($"Не удалось сохранить настройки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelSaveSettings()
		{
			IsSaveSettingsModalVisible = false;
			Log.Information("Cancelled save settings");
		}

		[RelayCommand]
		private void ResetSettings()
		{
			IsResetSettingsModalVisible = true;
			Log.Information("Opened reset settings modal");
		}

		[RelayCommand]
		private async Task ConfirmResetSettingsAsync()
		{
			try
			{
				_chatSettings.Reset();
				_diarySettings.Reset();
				_profileSettings.Reset();
				_modelSettings.Reset();
				_notificationSettings.Reset();
				IsResetSettingsModalVisible = false;
				await _notificationSettings.NotifyAsync("Настройки сброшены");
				Log.Information("Settings reset");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to reset settings");
				MessageBox.Show($"Не удалось сбросить настройки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelResetSettings()
		{
			IsResetSettingsModalVisible = false;
			Log.Information("Cancelled reset settings");
		}

		[RelayCommand]
		private void ShowAbout()
		{
			IsAboutModalVisible = true;
			Log.Information("Opened about modal");
		}

		[RelayCommand]
		private void CloseAbout()
		{
			IsAboutModalVisible = false;
			Log.Information("Closed about modal");
		}

		[RelayCommand]
		private void BackupSettings()
		{
			IsBackupModalVisible = true;
			Log.Information("Opened backup settings modal");
		}

		[RelayCommand]
		private async Task SaveBackupAsync()
		{
			try
			{
				var saveFileDialog = new Microsoft.Win32.SaveFileDialog
				{
					Title = "Резервное копирование настроек",
					Filter = "JSON файлы (*.json)|*.json"
				};

				if (saveFileDialog.ShowDialog() == true)
				{
					var settings = new
					{
						ChatSettings = new
						{
							_chatSettings.ThemeIndex,
							_chatSettings.BotName
						},
						DiarySettings = new
						{
							_diarySettings.DiaryNoteStyleIndex
						},
						ProfileSettings = new
						{
							_profileSettings.Username,
							_profileSettings.AvatarPath
						},
						ModelSettings = new
						{
							_modelSettings.CommunicationStyleIndex,
							_modelSettings.EmpathyLevel,
							_modelSettings.ToneIndex,
							_modelSettings.IsLocalModel,
							_modelSettings.IsServerModel,
							_modelSettings.Temperature,
							_modelSettings.TopP,
							_modelSettings.MaxResponseLength,
							_modelSettings.CustomPrompt
						},
						NotificationSettings = new
						{
							_notificationSettings.NotificationsEnabled,
							_notificationSettings.NotificationSoundIndex,
							_notificationSettings.NotificationVolume,
							_notificationSettings.ReminderFrequency,
							_notificationSettings.ReminderTime
						}
					};
					var json = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
					System.IO.File.WriteAllText(saveFileDialog.FileName, json);
					IsBackupModalVisible = false;
					await _notificationSettings.NotifyAsync("Настройки сохранены в резервной копии");
					Log.Information("Settings backed up to {FilePath}", saveFileDialog.FileName);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to backup settings");
				MessageBox.Show($"Не удалось создать резервную копию: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelBackup()
		{
			IsBackupModalVisible = false;
			Log.Information("Cancelled backup settings");
		}
	}
}