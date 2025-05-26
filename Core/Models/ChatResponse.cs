using Newtonsoft.Json;

namespace ChatBotClient.Core.Models
{
	public class ChatResponse
	{
		[JsonProperty("bot_response")]
		public string BotResponse { get; set; }
	}
}