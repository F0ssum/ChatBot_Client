using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using System.Windows.Media;

namespace ChatBotClient.ViewModel.Settings
{
	public partial class ChatSettingsViewModel : ObservableObject
	{
		private int _themeIndex;
		private string _botName = "EmotionAid";
		private Brush _previewBackground;

		public int ThemeIndex
		{
			get => _themeIndex;
			set
			{
				SetProperty(ref _themeIndex, value);
				UpdatePreviewBackground();
			}
		}

		public string BotName
		{
			get => _botName;
			set => SetProperty(ref _botName, !string.IsNullOrWhiteSpace(value) ? value : _botName);
		}

		public Brush PreviewBackground
		{
			get => _previewBackground;
			set => SetProperty(ref _previewBackground, value);
		}

		public ChatSettingsViewModel()
		{
			UpdatePreviewBackground();
			Log.Information("ChatSettingsViewModel initialized");
		}

		public void Reset()
		{
			ThemeIndex = 0;
			BotName = "EmotionAid";
			UpdatePreviewBackground();
			Log.Information("ChatSettingsViewModel reset");
		}

		private void UpdatePreviewBackground()
		{
			PreviewBackground = ThemeIndex switch
			{
				0 => Brushes.White, // Светлая
				1 => Brushes.DarkGray, // Тёмная
				2 => Brushes.LightBlue, // Системная (заглушка)
				_ => Brushes.White
			};
		}
	}
}