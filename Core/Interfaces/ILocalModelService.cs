
using ChatBotClient.Core.Models;

namespace ChatBotClient.Core
{
	/// <summary>
	/// Provides methods for processing messages using a local model.
	/// </summary>
	public interface ILocalModelService
	{
		/// <summary>
		/// Asynchronously processes a message using the local model.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="message">The message to process.</param>
		/// <param name="history">The conversation history.</param>
		/// <param name="customPrompt">The custom prompt (optional).</param>
		/// <param name="temperature">The temperature for response generation.</param>
		/// <param name="topP">The top-p sampling parameter.</param>
		/// <param name="maxResponseLength">The maximum response length.</param>
		/// <returns>The processed response.</returns>
		Task<string> ProcessMessageAsync(string userId, string message, List<Message> history, string customPrompt, double temperature, double topP, int maxResponseLength);
	}
}