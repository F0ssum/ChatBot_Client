using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotClient.Infrastructure.Services
{
	public class ApiService : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly AsyncRetryPolicy _retryPolicy;
		private readonly LocalStorageService _localStorageService;
		private readonly AppConfiguration _config;

		public ApiService(LocalStorageService localStorageService, AppConfiguration config)
		{
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			_config = config ?? throw new ArgumentNullException(nameof(config));
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
			Log.Information("ApiService initialized with BaseAddress: {BaseAddress}", _config.ApiBaseUrl);
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

			var cacheKey = $"message_{userId}_{message.GetHashCode()}";
			var cachedResponse = await _localStorageService.GetCachedDataAsync<string>(cacheKey);
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
			await _localStorageService.CacheDataAsync(cacheKey, response.BotResponse, TimeSpan.FromMinutes(10));
			return response.BotResponse;
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