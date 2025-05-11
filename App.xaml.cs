using ChatBotClient.Views;
using System.Windows;

namespace ChatBotClient
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			var chatPage = new ChatPage();
			chatPage.Show();
		}
	}
}