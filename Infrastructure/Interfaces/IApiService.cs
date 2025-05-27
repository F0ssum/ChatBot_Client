// Core/Interfaces/IApiService.cs

// Core/Interfaces/IApiService.cs
using ChatBotClient.Core.Models;

namespace ChatBotClient.Infrastructure.Interfaces
{
	/// <summary>
	/// Provides methods for interacting with the server API.
	/// </summary>
	public interface IApiService : IDisposable
	{
		/// <summary>
		/// Sets the model processing mode (local or server).
		/// </summary>
		/// <param name="useLocalModel">True to use local model, false for server.</param>
		void SetModelMode(bool useLocalModel);

		/// <summary>
		/// Asynchronously sends a message to the server or local model.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="message">The message content.</param>
		/// <param name="history">The conversation history.</param>
		/// <param name="language">The language code (e.g., "en").</param>
		/// <param name="customPrompt">The custom prompt (optional).</param>
		/// <param name="temperature">The temperature for response generation.</param>
		/// <param name="topP">The top-p sampling parameter.</param>
		/// <param name="maxResponseLength">The maximum response length.</param>
		/// <returns>The server or local model response.</returns>
		Task<string> SendMessageAsync(string userId, string message, List<string> history, string language, string customPrompt, double temperature, double topP, int maxResponseLength);

		/// <summary>
		/// Asynchronously retrieves chat history for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="page">The page number (default: 1).</param>
		/// <param name="pageSize">The number of entries per page (default: 50).</param>
		/// <returns>The list of messages.</returns>
		Task<List<Message>> GetHistoryAsync(string userId, int page = 1, int pageSize = 50);

		/// <summary>
		/// Asynchronously retrieves mood statistics for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>A dictionary of mood statistics.</returns>
		Task<Dictionary<string, int>> GetMoodStatsAsync(string userId);

		/// <summary>
		/// Asynchronously retrieves a list of chat history identifiers for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>A list of conversation IDs.</returns>
		Task<List<string>> GetChatHistoryListAsync(string userId);

		/// <summary>
		/// Asynchronously clears chat history for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task ClearChatHistoryAsync(string userId);

		/// <summary>
		/// Asynchronously retrieves diary entries for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="page">The page number (default: 1).</param>
		/// <param name="pageSize">The number of entries per page (default: 50).</param>
		/// <returns>A list of diary entries.</returns>
		Task<List<DiaryEntry>> GetDiaryEntriesAsync(string userId, int page = 1, int pageSize = 50);

		/// <summary>
		/// Asynchronously creates a new diary entry for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="entry">The diary entry to create.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task CreateDiaryEntryAsync(string userId, DiaryEntry entry);

		/// <summary>
		/// Asynchronously retrieves diary tags for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>A list of diary tags.</returns>
		Task<List<string>> GetDiaryTagsAsync(string userId);

		/// <summary>
		/// Asynchronously adds a tag to a user's diary.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="tag">The tag to add.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task AddDiaryTagAsync(string userId, string tag);

		/// <summary>
		/// Asynchronously archives diary entries for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task ArchiveDiaryEntriesAsync(string userId);

		Task<string> SendAudioAsync(string userId, string filePath);
	}
}