using ChatBotClient.Core.Configuration;
using NAudio.Wave;
using Serilog;
using System.Windows.Forms;

namespace ChatBotClient.Infrastructure.Service
{
	public class NotificationService : IDisposable
	{
		private readonly NotifyIcon _notifyIcon;
		private readonly string[] _soundFiles;

		public NotificationService(AppConfiguration config)
		{
			_notifyIcon = new NotifyIcon { Icon = new System.Drawing.Icon("Resources/icon.ico"), Visible = true };
			_soundFiles =
			[
				"Resources/Sounds/standard.wav",
				"Resources/Sounds/alert.wav",
				"Resources/Sounds/custom.wav"
			];
			Log.Information("NotificationService initialized");
		}

		public async Task ShowNotificationAsync(string message, int soundIndex = 0, float volume = 1.0f)
		{
			try
			{
				_notifyIcon.ShowBalloonTip(3000, "EmotionAid", message, ToolTipIcon.Info);
				if (soundIndex >= 0 && soundIndex < _soundFiles.Length)
				{
					using var audioFile = new AudioFileReader(_soundFiles[soundIndex]);
					using var volumeStream = new WaveChannel32(audioFile) { Volume = volume };
					using var outputDevice = new WaveOutEvent();
					outputDevice.Init(volumeStream);
					outputDevice.Play();
					await Task.Delay(audioFile.TotalTime);
				}
				Log.Information("Notification sent: {Message}", message);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to send notification: {Message}", message);
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				_notifyIcon?.Dispose();
				Log.Information("NotificationService disposed");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to dispose NotificationService");
			}
		}
	}
}