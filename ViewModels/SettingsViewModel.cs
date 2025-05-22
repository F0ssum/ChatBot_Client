using ChatBotClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Linq;

namespace ChatBotClient.ViewModels
{
	public partial class SettingsViewModel : ObservableObject
	{
		private readonly LocalStorageService _localStorageService;

		[ObservableProperty]
		private string username;

		public SettingsViewModel(LocalStorageService localStorageService)
		{
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			Log.Information("SettingsViewModel initialized");
			LoadSettings();
		}

		private void LoadSettings()
		{
			try
			{
				var (userIds, _) = _localStorageService.LoadUserData();
				Username = userIds?.FirstOrDefault() ?? string.Empty;
				Log.Information("Loaded username: {Username}", Username);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load settings: {Message}", ex.Message);
			}
		}

		[RelayCommand]
		private void SaveSettings()
		{
			try
			{
				if (!string.IsNullOrEmpty(Username))
				{
					_localStorageService.SaveUserIds(new[] { Username }.ToList());
					Log.Information("Settings saved, Username: {Username}", Username);
				}
				else
				{
					Log.Warning("Username is empty, skipping save");
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save settings: {Message}", ex.Message);
			}
		}
	}
}