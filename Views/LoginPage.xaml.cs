using ChatBotClient.Services;
using ChatBotClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ChatBotClient.Views
{
	public partial class LoginPage : Window
	{
		public LoginPage()
		{
			InitializeComponent();
			var apiService = new ApiService();
			var storageService = new LocalStorageService();
			DataContext = new LoginViewModel(apiService, storageService);
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			((LoginViewModel)DataContext).Password = PasswordBox.Password;
		}

	}
}
