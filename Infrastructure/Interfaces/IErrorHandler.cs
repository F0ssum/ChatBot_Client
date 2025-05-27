// Core/Interfaces/IErrorHandler.cs
namespace ChatBotClient.Infrastructure.Interfaces
{
	/// <summary>
	/// Provides methods for displaying error messages.
	/// </summary>
	public interface IErrorHandler
	{
		/// <summary>
		/// Displays an error message synchronously.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="title">The error title (default is "Error").</param>
		void ShowError(string message, string title = "Error");

		/// <summary>
		/// Asynchronously displays an error message.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="title">The error title (default is "Error").</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task ShowErrorAsync(string message, string title = "Error");
	}
}