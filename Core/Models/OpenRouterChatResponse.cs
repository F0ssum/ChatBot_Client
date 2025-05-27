namespace ChatBotClient.Core.Models
{
	public class OpenRouterChatResponse
	{
		public List<Choice> choices { get; set; }
	}

	public class Choice
	{
		public ChatMessage message { get; set; }
	}

	public class ChatMessage
	{
		public string role { get; set; }
		public string content { get; set; }
	}
}