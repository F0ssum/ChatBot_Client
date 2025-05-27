using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using ChatBotClient.ViewModel.Settings;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
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
		private readonly ModelSettingsViewModel _modelSettings;
		private string _baseUrl;

		public ApiService(LocalStorageService localStorageService, AppConfiguration config, ModelSettingsViewModel modelSettings)
		{
			_localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_modelSettings = modelSettings ?? throw new ArgumentNullException(nameof(modelSettings));
			_httpClient = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(_config.ApiTimeoutSeconds > 0 ? _config.ApiTimeoutSeconds : 60)
			};
			_retryPolicy = Policy
				.Handle<HttpRequestException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 2));
			UpdateBaseUrl(modelSettings.ServerAddress);
			UpdateApiToken(modelSettings.ApiToken);

			modelSettings.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(modelSettings.ServerAddress))
					UpdateBaseUrl(modelSettings.ServerAddress);
				if (e.PropertyName == nameof(modelSettings.ApiToken))
					UpdateApiToken(modelSettings.ApiToken);
			};
			Log.Information("ApiService initialized with BaseAddress: {BaseAddress}", _baseUrl);
		}

		public void UpdateBaseUrl(string newUrl)
		{
			_baseUrl = newUrl?.TrimEnd('/');
			_httpClient.BaseAddress = new Uri(_baseUrl);
			Log.Information("Base URL updated to: {BaseUrl}", _baseUrl);
		}

		public void UpdateApiToken(string token)
		{
			_httpClient.DefaultRequestHeaders.Remove("Authorization");
			if (!string.IsNullOrWhiteSpace(token))
			{
				_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
				Log.Information("Authorization header set with token: {Token}", token.Substring(0, Math.Min(10, token.Length)) + "...");
			}
			else
			{
				Log.Warning("API token is empty or null");
			}
		}

		private async Task<T> ExecuteHttpRequestAsync<T>(Func<Task<HttpResponseMessage>> requestAction, string logAction)
		{
			Log.Information("Executing {Action} at {Url}", logAction, _httpClient.BaseAddress);
			try
			{
				return await _retryPolicy.ExecuteAsync(async () =>
				{
					HttpResponseMessage response = await requestAction();
					string responseBody = await response.Content.ReadAsStringAsync();
					Log.Information("Received response: StatusCode={StatusCode}, Content={ResponseBody}", response.StatusCode, responseBody);

					if (!response.IsSuccessStatusCode)
					{
						Log.Error("Request failed: StatusCode={StatusCode}, Reason={Reason}, Content={ErrorContent}",
							response.StatusCode, response.ReasonPhrase, responseBody);
						if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
							throw new HttpRequestException("Rate limit exceeded. Please try again later.");
						throw new HttpRequestException($"Request failed: {response.StatusCode}, {responseBody}");
					}

					if (string.IsNullOrEmpty(responseBody))
						throw new Exception("Empty response from server");

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

		private string ComputeHash(string input)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(input);
			var hash = sha256.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}

		public async Task<string> SendMessageAsync(
			string userId, string message, List<Message> history, string language,
			string customPrompt, double temperature, double topP, int maxResponseLength,
			string userEmotion = null)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set");

			var cacheKey = $"message_{userId}_{ComputeHash(message)}";
			var cachedResponse = await _localStorageService.GetCachedDataAsync<string>(cacheKey);
			if (cachedResponse != null)
			{
				Log.Information("Returning cached response for message: {Message}", message);
				return cachedResponse;
			}

			var messagesList = new List<object>();

			if (!string.IsNullOrWhiteSpace(customPrompt))
			{
				messagesList.Add(new { role = "system", content = customPrompt });
			}

			if (history != null)
			{
				foreach (var m in history)
				{
					messagesList.Add(new
					{
						role = m.Author == userId ? "user" : "assistant",
						content = m.Text
					});
				}
			}

			messagesList.Add(new { role = "user", content = message });

			var payload = new
			{
				model = _modelSettings.SelectedModel,
				messages = messagesList,
				temperature,
				top_p = topP,
				max_tokens = maxResponseLength
			};

			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			Log.Information("Sending request to OpenRouter: {JsonPayload}", jsonPayload);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			var requestUri = "api/v1/chat/completions";
			Log.Information("Full request URL: {FullUrl}", new Uri(_httpClient.BaseAddress, requestUri).ToString());

			var response = await ExecuteHttpRequestAsync<OpenRouterChatResponse>(
				() => _httpClient.PostAsync(requestUri, content),
				"SendMessageAsync"
			);

			var botResponse = response?.choices?.FirstOrDefault()?.message?.content ?? "";
			await _localStorageService.CacheDataAsync(cacheKey, botResponse, TimeSpan.FromMinutes(10));
			return botResponse;
		}

		public async Task<bool> PingAsync()
		{
			try
			{
				var response = await _httpClient.GetAsync("api/v1/ping");
				return response.IsSuccessStatusCode;
			}
			catch
			{
				return false;
			}
		}

		public bool Ping()
		{
			try
			{
				return PingAsync().GetAwaiter().GetResult();
			}
			catch
			{
				return false;
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