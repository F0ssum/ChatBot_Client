using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;

namespace ChatBotClient.ViewModel.Settings
{
	public partial class ModelSettingsViewModel : ObservableObject
	{
		private readonly IServiceProvider _serviceProvider;
		private int _communicationStyleIndex;
		private int _empathyLevel;
		private int _toneIndex;
		private bool _isLocalModel;
		private bool _isServerModel = true;
		private double _temperature = 0.7;
		private double _topP = 0.9;
		private string _maxResponseLength = "200";
		private string _customPrompt = "You are a helpful assistant.";
		private bool _isAdvancedModelSettingsModalVisible;

		public int CommunicationStyleIndex
		{
			get => _communicationStyleIndex;
			set => SetProperty(ref _communicationStyleIndex, value);
		}

		public int EmpathyLevel
		{
			get => _empathyLevel;
			set => SetProperty(ref _empathyLevel, Math.Clamp(value, 0, 2));
		}

		public int ToneIndex
		{
			get => _toneIndex;
			set => SetProperty(ref _toneIndex, value);
		}

		public bool IsLocalModel
		{
			get => _isLocalModel;
			set
			{
				SetProperty(ref _isLocalModel, value);
				if (value) IsServerModel = false;
			}
		}

		public bool IsServerModel
		{
			get => _isServerModel;
			set
			{
				SetProperty(ref _isServerModel, value);
				if (value) IsLocalModel = false;
			}
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

		public string MaxResponseLength
		{
			get => _maxResponseLength;
			set
			{
				if (int.TryParse(value, out int length) && length > 0)
					SetProperty(ref _maxResponseLength, value);
				else
					Log.Warning("Invalid MaxResponseLength: {Value}", value);
			}
		}

		public string CustomPrompt
		{
			get => _customPrompt;
			set => SetProperty(ref _customPrompt, !string.IsNullOrWhiteSpace(value) ? value : "You are a helpful assistant.");
		}

		public bool IsAdvancedModelSettingsModalVisible
		{
			get => _isAdvancedModelSettingsModalVisible;
			set => SetProperty(ref _isAdvancedModelSettingsModalVisible, value);
		}

		public ModelSettingsViewModel()
		{
			Log.Information("ModelSettingsViewModel initialized");
		}

		[RelayCommand]
		private void ShowAdvancedModelSettings()
		{
			IsAdvancedModelSettingsModalVisible = true;
			Log.Information("Opened advanced model settings modal");
		}

		[RelayCommand]
		private async Task SaveAdvancedModelSettingsAsync()
		{
			try
			{
				IsAdvancedModelSettingsModalVisible = false;

				var notificationSettings = _serviceProvider.GetService<NotificationSettingsViewModel>();
				await notificationSettings.NotifyAsync("Расширенные настройки модели сохранены");

				Log.Information("Saved advanced model settings");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save advanced model settings");
				MessageBox.Show($"Не удалось сохранить настройки модели: {ex.Message}",
								"Ошибка",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
			}
		}

		[RelayCommand]
		private void CancelAdvancedModelSettings()
		{
			IsAdvancedModelSettingsModalVisible = false;
			Log.Information("Cancelled advanced model settings");
		}

		public void Reset()
		{
			CommunicationStyleIndex = 0;
			EmpathyLevel = 0;
			ToneIndex = 0;
			IsLocalModel = false;
			IsServerModel = true;
			Temperature = 0.7;
			TopP = 0.9;
			MaxResponseLength = "200";
			CustomPrompt = "You are a helpful assistant.";
			Log.Information("ModelSettingsViewModel reset");
		}
	}
}