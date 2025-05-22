using Newtonsoft.Json;
namespace ChatBotClient.Models
{
	public enum MessageStatus
	{
		None,
		Sending,
		Sent,
		Delivered,
		Read,
		Error
	}

	public class Message
	{
		public string Author { get; set; }
		public string Text { get; set; }
		public DateTime Timestamp { get; set; }
		public MessageStatus Status { get; set; }
		[JsonIgnore]
		public bool IsUserMessage => Author == "User";
	}
}