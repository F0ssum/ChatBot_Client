using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatBotClient.Services;
using System.Windows;
using ChatBotClient.Views;

namespace ChatBotClient.ViewModels
{
	public partial class LoginViewModel(ApiService apiService, LocalStorageService storageService) : ObservableObject
	{
		private readonly ApiService _apiService = apiService;
		private readonly LocalStorageService _storageService = storageService;

		[ObservableProperty]
		private string username;

		[ObservableProperty]
		private string password;

		[ObservableProperty]
		private string email;

		[ObservableProperty]
		private string status;

		[RelayCommand]
		async Task Login()
		{
			Status = "Logging in...";
			try
			{
				string token = await _apiService.LoginAsync(Username, Password);
				_apiService.SetToken(token);
				_storageService.SaveData((Token: token, Username)); // Сохраняем токен и имя
				Status = "Success!";
				OpenChatWindow();
			}
			catch (Exception ex)
			{
				Status = "Failed";
				MessageBox.Show($"Login failed: {ex.Message}");
			}
		}

		[RelayCommand]
		async Task Register()
		{
			Status = "Registering...";
			try
			{
				await _apiService.RegisterAsync(Username, Password, Email);
				Status = "Registration successful! Please log in.";
				Email = ""; // Очищаем поле после регистрации
			}
			catch (Exception ex)
			{
				Status = "Failed";
				MessageBox.Show($"Registration failed: {ex.Message}");
			}
		}

		private static void OpenChatWindow()
		{
			var chatWindow = new ChatPage();
			chatWindow.Show();
			Application.Current.Windows[0]?.Close(); // Закрываем окно входа
		}
	}
}