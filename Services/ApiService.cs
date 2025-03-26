using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
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

		// Установка токена после входа
		public void SetToken(string token)
		{
			_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}

		// Регистрация
		public async Task<bool> RegisterAsync(string username, string password, string email)
		{
			var payload = new { username, password, email };
			var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

			HttpResponseMessage response = await _httpClient.PostAsync("auth/register", content);
			response.EnsureSuccessStatusCode();
			return true;
		}

		// Вход
		public async Task<string> LoginAsync(string username, string password)
		{
			var payload = new { username, password };
			var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

			HttpResponseMessage response = await _httpClient.PostAsync("auth/login", content);
			response.EnsureSuccessStatusCode();

			string responseBody = await response.Content.ReadAsStringAsync();
			var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
			return jsonResponse.access_token?.ToString() ?? throw new Exception("Invalid login response");
		}

		// Получение профиля
		public async Task<UserProfile> GetProfileAsync()
		{
			HttpResponseMessage response = await _httpClient.GetAsync("auth/profile");
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<UserProfile>(responseBody);
		}

		// Отправка сообщения (обновим user_id)
		public async Task<string> SendMessageAsync(string userId, string message, ObservableCollection<Message> history)
		{
			var serializedHistory = history.Select(m => new
			{
				author = m.Author.ToLower(),
				text = m.Text
			}).ToList();

			var payload = new { user_id = userId, message, history = serializedHistory };
			var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

			HttpResponseMessage response = await _httpClient.PostAsync("chat/", content);
			response.EnsureSuccessStatusCode();

			string responseBody = await response.Content.ReadAsStringAsync();
			var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
			return jsonResponse.bot_response?.ToString() ?? throw new Exception("Invalid response format");
		}

		// Получение истории
		public async Task<List<Message>> GetHistoryAsync(string userId)
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"chat/get-history/{userId}");
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<Message>>(responseBody) ?? [];
		}

		// Получение статистики настроений
		public async Task<Dictionary<string, int>> GetMoodStatsAsync(string userId)
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"chat/mood-stats/{userId}");
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
			return JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonResponse.mood_stats.ToString());
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}

}