using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ChatBotClient.Models;

namespace ChatBotClient.Services
{
	public class ApiService : IDisposable
	{
		private readonly HttpClient _httpClient;

		public ApiService()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("http://localhost:8080/"),
				Timeout = TimeSpan.FromMinutes(5)
			};
		}

		public async Task<string> SendMessageAsync(string userId, string message, ObservableCollection<Message> history)
		{
			if (string.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId), "User ID is not set.");

			// Попробуйте разные варианты сериализации history
			var serializedHistory = history.Select(m => new { author = m.Author, text = m.Text }).ToList();
			var payload = new { user_id = userId, message, history = serializedHistory };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			Console.WriteLine($"Sending message to {_httpClient.BaseAddress}chat/ with payload: {jsonPayload}");
			Console.WriteLine($"Content-Type: {content.Headers.ContentType}");

			try
			{
				HttpResponseMessage response = await _httpClient.PostAsync("chat/", content);

				if (!response.IsSuccessStatusCode)
				{
					string errorContent = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"Request failed: {response.StatusCode}, {errorContent}");
					throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
				}

				string responseBody = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Response: {responseBody}");
				var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
				return jsonResponse.bot_response?.ToString() ?? throw new Exception("Invalid response format");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				throw;
			}
		}
		public async Task<List<Message>> GetHistoryAsync(string userId)
		{
			Console.WriteLine($"Fetching history from {_httpClient.BaseAddress}chat/get-history/{userId}");
			HttpResponseMessage response = await _httpClient.GetAsync($"chat/get-history/{userId}");

			if (!response.IsSuccessStatusCode)
			{
				string errorContent = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Request failed: {response.StatusCode}, {errorContent}");
				throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
			}

			string responseBody = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Response: {responseBody}");
			return JsonConvert.DeserializeObject<List<Message>>(responseBody) ?? [];
		}

		public async Task<Dictionary<string, int>> GetMoodStatsAsync(string userId)
		{
			Console.WriteLine($"Fetching mood stats from {_httpClient.BaseAddress}chat/mood-stats/{userId}");
			HttpResponseMessage response = await _httpClient.GetAsync($"chat/mood-stats/{userId}");

			if (!response.IsSuccessStatusCode)
			{
				string errorContent = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Request failed: {response.StatusCode}, {errorContent}");
				throw new HttpRequestException($"Request failed: {response.StatusCode}, {errorContent}");
			}

			string responseBody = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Response: {responseBody}");
			var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
			return JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonResponse.mood_stats.ToString());
		}
		
		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}