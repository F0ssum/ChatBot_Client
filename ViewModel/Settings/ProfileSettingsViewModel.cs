using ChatBotClient.Core.Models;
using ChatBotClient.Infrastructure.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Serilog;
using System.Windows;

namespace ChatBotClient.ViewModel.Settings
{
	public partial class ProfileSettingsViewModel : ObservableObject
	{
		private readonly LocalStorageService _localStorageService;
		private string _username;
		private string _avatarPath;
		private readonly IServiceProvider _serviceProvider;
		private readonly NotificationSettingsViewModel _notificationSettings;
		public string Username
		{
			get => _username;
			set => SetProperty(ref _username, !string.IsNullOrWhiteSpace(value) ? value : _username);
		}

		public string AvatarPath
		{
			get => _avatarPath;
			set => SetProperty(ref _avatarPath, value);
		}

		public ProfileSettingsViewModel(LocalStorageService localStorageService)
		{
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			var (userIds, _) = localStorageService.LoadUserData();
			if (userIds?.Count > 0)
			{
				var user = localStorageService.LoadData<User>($"user_{userIds[0]}");
				Username = user?.Name ?? string.Empty;
			}
			Log.Information("ProfileSettingsViewModel initialized");
		}

		[RelayCommand]
		private async Task UploadAvatarAsync()
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

					var (userIds, _) = _localStorageService.LoadUserData();
					if (userIds?.Count > 0)
					{
						await _localStorageService.SaveAvatarAsync(userIds[0], AvatarPath);
						await _notificationSettings.NotifyAsync("Аватар сохранён локально");
						Log.Information("Аватар успешно сохранён локально");
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Ошибка при сохранении аватара");
				MessageBox.Show($"Не удалось загрузить аватар: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Reset()
		{
			Username = string.Empty;
			AvatarPath = null;
			Log.Information("ProfileSettingsViewModel reset");
		}
	}
}