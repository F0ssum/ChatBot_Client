// Core/Interfaces/INotificationService.cs
namespace ChatBotClient.Core.Interfaces
{
	/// <summary>
	/// Provides methods for displaying notifications with optional sound.
	/// </summary>
	public interface INotificationService : IDisposable
	{
		/// <summary>
		/// Asynchronously displays a notification with an optional sound.
		/// </summary>
		/// <param name="message">The notification message.</param>
		/// <param name="soundIndex">The index of the sound file to play (default is 0).</param>
		/// <param name="volume">The volume level for the sound (default is 1.0).</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task ShowNotificationAsync(string message, int soundIndex = 0, float volume = 1.0f);
	}
}