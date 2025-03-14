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

		public async Task<string> SendMessageAsync(string message, ObservableCollection<Message> history)
		{
			// Формат, совместимый с сервером
			var serializedHistory = history.Select(m => new
			{
				author = m.Author.ToLower(),
				text = m.Text
			}).ToList();

			var payload = new { user_id = "123", message, history = serializedHistory };
			var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

			HttpResponseMessage response = await _httpClient.PostAsync("chat", content);
			response.EnsureSuccessStatusCode();

			string responseBody = await response.Content.ReadAsStringAsync();
			var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
			return jsonResponse.bot_response?.ToString() ?? throw new Exception("Invalid response format");
		}

		public async Task<List<Message>> GetHistoryAsync()
		{
			HttpResponseMessage response = await _httpClient.GetAsync("get-history/123");
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<Message>>(responseBody) ?? new List<Message>();
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}