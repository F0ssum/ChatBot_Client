using ChatBotClient.Core;
using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;
using System.IO;
using System.Net.Http;
using System.Text;

namespace ChatBotClient.Infrastructure.Services
{


	public class ApiService : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly AsyncRetryPolicy _retryPolicy;
		private readonly ILocalModelService _localModelService;
		private readonly CacheService _cacheService;
		private bool _useLocalModel;
		private readonly AppConfiguration _config;

		public ApiService(ILocalModelService localModelService, CacheService cacheService, AppConfiguration config)
		{
			_localModelService = localModelService ?? throw new ArgumentNullException(nameof(localModelService));
			_cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_useLocalModel = _config.UseLocalModel;
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(_config.ApiBaseUrl),
				Timeout = TimeSpan.FromSeconds(_config.ApiTimeoutSeconds)
			};
			if (!string.IsNullOrEmpty(_config.ApiKey))
			{
				_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
			}
			_retryPolicy = Policy
				.Handle<HttpRequestException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
			Log.Information("ApiService initialized with BaseAddress: {BaseAddress}, UseLocalModel: {UseLocalModel}", _config.ApiBaseUrl, _useLocalModel);
		}

		public void SetModelMode(bool useLocalModel)
		{
			_useLocalModel = useLocalModel;
			Log.Information("ApiService model mode changed to {Mode}", useLocalModel ? "Local" : "Server");
		}

		private async Task<T> ExecuteHttpRequestAsync<T>(Func<Task<HttpResponseMessage>> requestAction, string logAction)
		{
			Log.Information("Executing {Action} at {Url}", logAction, _httpClient.BaseAddress);
			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await requestAction();
					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Log.Error("Request failed: {StatusCode}, {ErrorContent}", response.StatusCode, errorContent);
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
					}

					string responseBody = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(responseBody))
						throw new Exception("Empty response from server");

					Log.Information("Response: {ResponseBody}", responseBody);
					return JsonConvert.DeserializeObject<T>(responseBody) ?? throw new Exception("Invalid response format");
				});
			}
			catch (HttpRequestException ex)
			{
				Log.Error(ex, "HTTP error in {Action}", logAction);
				throw;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Unexpected error in {Action}", logAction);
				throw;
			}
		}

		public async Task<string> SendMessageAsync(string userId, string message, List<Message> history, string language, string customPrompt, double temperature, double topP, int maxResponseLength)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			if (_useLocalModel)
			{
				Log.Information("Processing message locally for user {UserId}", userId);
				return await _localModelService.ProcessMessageAsync(userId, message, history, customPrompt, temperature, topP, maxResponseLength);
			}

			var cacheKey = $"message_{userId}_{message.GetHashCode()}";
			var cachedResponse = _cacheService.GetCachedData<string>(cacheKey);
			if (cachedResponse != null)
			{
				Log.Information("Returning cached response for message: {Message}", message);
				return cachedResponse;
			}

			var serializedHistory = history.Select(m => new { author = m.Author, text = m.Text }).ToList();
			var payload = new { user_id = userId, message, history = serializedHistory, language, customPrompt, temperature, topP, maxResponseLength };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			var response = await ExecuteHttpRequestAsync<ChatResponse>(
				() => _httpClient.PostAsync("chat/", content),
				"SendMessageAsync"
			);
			_cacheService.CacheData(cacheKey, response.BotResponse, TimeSpan.FromMinutes(10));
			return response.BotResponse;
		}

		public async Task<List<Message>> GetHistoryAsync(string userId, int page = 1, int pageSize = 50)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_{userId}_page{page}";
			var cachedHistory = _cacheService.GetCachedData<List<Message>>(cacheKey);
			if (cachedHistory != null)
			{
				Log.Information("Returning cached history for user {UserId}, page {Page}", userId, page);
				return cachedHistory;
			}

			var history = await ExecuteHttpRequestAsync<List<Message>>(
				() => _httpClient.GetAsync($"chat/get-history/{userId}?page={page}&pageSize={pageSize}"),
				"GetHistoryAsync"
			);
			_cacheService.CacheData(cacheKey, history, TimeSpan.FromMinutes(10));
			return history;
		}

		public async Task<Dictionary<string, int>> GetMoodStatsAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"mood_stats_{userId}";
			var cachedStats = _cacheService.GetCachedData<Dictionary<string, int>>(cacheKey);
			if (cachedStats != null)
			{
				Log.Information("Returning cached mood stats for user {UserId}", userId);
				return cachedStats;
			}

			var response = await ExecuteHttpRequestAsync<MoodStatsResponse>(
				() => _httpClient.GetAsync($"chat/mood-stats/{userId}"),
				"GetMoodStatsAsync"
			);
			_cacheService.CacheData(cacheKey, response.MoodStats, TimeSpan.FromMinutes(10));
			return response.MoodStats;
		}

		public async Task<List<string>> GetChatHistoryListAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"history_list_{userId}";
			var cachedList = _cacheService.GetCachedData<List<string>>(cacheKey);
			if (cachedList != null)
			{
				Log.Information("Returning cached history list for user {UserId}", userId);
				return cachedList;
			}

			var list = await ExecuteHttpRequestAsync<List<string>>(
				() => _httpClient.GetAsync($"chat/history-list/{userId}"),
				"GetChatHistoryListAsync"
			);
			_cacheService.CacheData(cacheKey, list, TimeSpan.FromMinutes(10));
			return list;
		}

		public async Task ClearChatHistoryAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			await ExecuteHttpRequestAsync<object>(
				() => _httpClient.DeleteAsync($"chat/clear-history/{userId}"),
				"ClearChatHistoryAsync"
			);
			_cacheService.ClearCache($"history_{userId}");
			Log.Information("Chat history cleared for user {UserId}", userId);
		}

		public async Task<List<DiaryEntry>> GetDiaryEntriesAsync(string userId, int page = 1, int pageSize = 50)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"diary_entries_{userId}_page{page}";
			var cachedEntries = _cacheService.GetCachedData<List<DiaryEntry>>(cacheKey);
			if (cachedEntries != null)
			{
				Log.Information("Returning cached diary entries for user {UserId}, page {Page}", userId, page);
				return cachedEntries;
			}

			var entries = await ExecuteHttpRequestAsync<List<DiaryEntry>>(
				() => _httpClient.GetAsync($"diary/entries/{userId}?page={page}&pageSize={pageSize}"),
				"GetDiaryEntriesAsync"
			);
			_cacheService.CacheData(cacheKey, entries, TimeSpan.FromMinutes(10));
			return entries;
		}

		public async Task CreateDiaryEntryAsync(string userId, DiaryEntry entry)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			if (entry == null)
				throw new ArgumentNullException(nameof(entry), "Diary entry cannot be null");

			var payload = new { user_id = userId, entry };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			await ExecuteHttpRequestAsync<object>(
				() => _httpClient.PostAsync("diary/entries", content),
				"CreateDiaryEntryAsync"
			);
			_cacheService.ClearCache($"diary_entries_{userId}");
			Log.Information("Diary entry created for user {UserId}", userId);
		}

		public async Task<List<string>> GetDiaryTagsAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"diary_tags_{userId}";
			var cachedTags = _cacheService.GetCachedData<List<string>>(cacheKey);
			if (cachedTags != null)
			{
				Log.Information("Returning cached diary tags for user {UserId}", userId);
				return cachedTags;
			}

			var tags = await ExecuteHttpRequestAsync<List<string>>(
				() => _httpClient.GetAsync($"diary/tags/{userId}"),
				"GetDiaryTagsAsync"
			);
			_cacheService.CacheData(cacheKey, tags, TimeSpan.FromMinutes(10));
			return tags;
		}

		public async Task AddDiaryTagAsync(string userId, string tag)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			if (string.IsNullOrEmpty(tag))
				throw new ArgumentNullException(nameof(tag), "Tag cannot be empty");

			var payload = new { user_id = userId, tag };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			await ExecuteHttpRequestAsync<object>(
				() => _httpClient.PostAsync("diary/tags", content),
				"AddDiaryTagAsync"
			);
			_cacheService.ClearCache($"diary_tags_{userId}");
			Log.Information("Diary tag added for user {UserId}: {Tag}", userId, tag);
		}

		public async Task ArchiveDiaryEntriesAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			await ExecuteHttpRequestAsync<object>(
				() => _httpClient.DeleteAsync($"diary/archive/{userId}"),
				"ArchiveDiaryEntriesAsync"
			);
			_cacheService.ClearCache($"diary_entries_{userId}");
			Log.Information("Diary entries archived for user {UserId}", userId);
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
		public async Task<List<string>> GetTriggersAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"triggers_{userId}";
			var cachedTriggers = _cacheService.GetCachedData<List<string>>(cacheKey);
			if (cachedTriggers != null)
			{
				Log.Information("Returning cached triggers for user {UserId}", userId);
				return cachedTriggers;
			}

			var triggers = await ExecuteHttpRequestAsync<List<string>>(
				() => _httpClient.GetAsync($"diary/triggers/{userId}"),
				"GetTriggersAsync"
			);

			_cacheService.CacheData(cacheKey, triggers, TimeSpan.FromMinutes(10));
			return triggers;
		}
		
		public async Task<string> SendAudioAsync(string userId, string filePath)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
				throw new ArgumentException("Invalid audio file path", nameof(filePath));

			using var content = new MultipartFormDataContent();
			content.Add(new StringContent(userId), "user_id");
			content.Add(new StreamContent(File.OpenRead(filePath)), "audio", Path.GetFileName(filePath));

			var response = await ExecuteHttpRequestAsync<ChatResponse>(
				() => _httpClient.PostAsync("chat/audio", content),
				"SendAudioAsync"
			);
			return response.BotResponse;
		}
	}
}