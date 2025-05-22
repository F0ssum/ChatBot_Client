using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatBotClient.Models;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;

namespace ChatBotClient.Services
{
	public class ChatResponse
	{
		[JsonProperty("bot_response")]
		public string BotResponse { get; set; }
	}

	public class MoodStatsResponse
	{
		[JsonProperty("mood_stats")]
		public Dictionary<string, int> MoodStats { get; set; }
	}

	public class ApiService : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly AsyncRetryPolicy _retryPolicy;

		public ApiService()
		{
			string baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8080/";
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(baseUrl),
				Timeout = TimeSpan.FromMinutes(5)
			};
			_retryPolicy = Policy
				.Handle<HttpRequestException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
			Log.Information("ApiService initialized with BaseAddress: {BaseAddress}", baseUrl);
		}

		public async Task<string> SendMessageAsync(string userId, string message, ObservableCollection<Message> history)
		{
			if (string.IsNullOrEmpty(userId))
			{
				Log.Error("User ID is not set");
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			}

			var serializedHistory = history.Select(m => new { author = m.Author, text = m.Text }).ToList();
			var payload = new { user_id = userId, message, history = serializedHistory };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			Log.Information("Sending message to {Url} with payload: {Payload}", _httpClient.BaseAddress + "chat/", jsonPayload);

			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.PostAsync("chat/", content);

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Response: {ResponseBody}", responseBody);
					var jsonResponse = JsonConvert.DeserializeObject<ChatResponse>(responseBody);
					return jsonResponse?.BotResponse ?? throw new Exception("Invalid response format");
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in SendMessageAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in SendMessageAsync");
				throw;
			}
		}

		public async Task<List<Message>> GetHistoryAsync(string userId)
		{
			Log.Information("Fetching history from {Url}", _httpClient.BaseAddress + $"chat/get-history/{userId}");

			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.GetAsync($"chat/get-history/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Response: {ResponseBody}", responseBody);
					return JsonConvert.DeserializeObject<List<Message>>(responseBody) ?? [];
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in GetHistoryAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in GetHistoryAsync");
				throw;
			}
		}

		public async Task<Dictionary<string, int>> GetMoodStatsAsync(string userId)
		{
			Log.Information("Fetching mood stats from {Url}", _httpClient.BaseAddress + $"chat/mood-stats/{userId}");

			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.GetAsync($"chat/mood-stats/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Response: {ResponseBody}", responseBody);
					var jsonResponse = JsonConvert.DeserializeObject<MoodStatsResponse>(responseBody);
					return jsonResponse?.MoodStats ?? throw new Exception("Invalid response format");
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in GetMoodStatsAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in GetMoodStatsAsync");
				throw;
			}
		}

		public async Task<List<string>> GetChatHistoryListAsync(string userId)
		{
			Log.Information("Fetching chat history list from {Url}", _httpClient.BaseAddress + $"chat/history-list/{userId}");

			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.GetAsync($"chat/history-list/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Response: {ResponseBody}", responseBody);
					return JsonConvert.DeserializeObject<List<string>>(responseBody) ?? [];
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in GetChatHistoryListAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in GetChatHistoryListAsync");
				throw;
			}
		}

		public async Task ClearChatHistoryAsync(string userId)
		{
			Log.Information("Clearing chat history at {Url}", _httpClient.BaseAddress + $"chat/clear-history/{userId}");

			try
			{
				await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.DeleteAsync($"chat/clear-history/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					Log.Information("Chat history cleared successfully for user {UserId}", userId);
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in ClearChatHistoryAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in ClearChatHistoryAsync");
				throw;
			}
		}

		public async Task<List<DiaryEntry>> GetDiaryEntriesAsync(string userId)
		{
			Log.Information("Fetching diary entries from {Url}", _httpClient.BaseAddress + $"diary/entries/{userId}");

			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.GetAsync($"diary/entries/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Response: {ResponseBody}", responseBody);
					return JsonConvert.DeserializeObject<List<DiaryEntry>>(responseBody) ?? [];
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in GetDiaryEntriesAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in GetDiaryEntriesAsync");
				throw;
			}
		}

		public async Task CreateDiaryEntryAsync(string userId, DiaryEntry entry)
		{
			var payload = new { user_id = userId, entry };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			Log.Information("Creating diary entry at {Url} with payload: {Payload}", _httpClient.BaseAddress + "diary/entries", jsonPayload);

			try
			{
				await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.PostAsync("diary/entries", content);

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					Log.Information("Diary entry created successfully");
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in CreateDiaryEntryAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in CreateDiaryEntryAsync");
				throw;
			}
		}

		public async Task<List<string>> GetDiaryTagsAsync(string userId)
		{
			Log.Information("Fetching diary tags from {Url}", _httpClient.BaseAddress + $"diary/tags/{userId}");

			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.GetAsync($"diary/tags/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Response: {ResponseBody}", responseBody);
					return JsonConvert.DeserializeObject<List<string>>(responseBody) ?? [];
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in GetDiaryTagsAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in GetDiaryTagsAsync");
				throw;
			}
		}

		public async Task AddDiaryTagAsync(string userId, string tag)
		{
			var payload = new { user_id = userId, tag };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			Log.Information("Adding diary tag at {Url} with payload: {Payload}", _httpClient.BaseAddress + "diary/tags", jsonPayload);

			try
			{
				await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.PostAsync("diary/tags", content);

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					Log.Information("Diary tag added successfully: {Tag}", tag);
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in AddDiaryTagAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in AddDiaryTagAsync");
				throw;
			}
		}

		public async Task ArchiveDiaryEntriesAsync(string userId)
		{
			Log.Information("Archiving diary entries at {Url}", _httpClient.BaseAddress + $"diary/archive/{userId}");

			try
			{
				await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await _httpClient.DeleteAsync($"diary/archive/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					Log.Information("Diary entries archived successfully for user {UserId}", userId);
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in ArchiveDiaryEntriesAsync");
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in ArchiveDiaryEntriesAsync");
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				_httpClient?.Dispose();
				Log.Information("ApiService disposed");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to dispose ApiService");
			}
		}
	}
}