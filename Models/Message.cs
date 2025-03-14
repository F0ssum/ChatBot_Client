using System;
using Newtonsoft.Json;

namespace ChatBotClient.Models
{
	public class Message
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("author")]
		public string Author { get; set; } // "User" или "Bot"

		[JsonProperty("timestamp1")]
		public DateTime Timestamp { get; set; }

		public string Status { get; set; } // "Sending...", "Sent", "Failed"
	}
}