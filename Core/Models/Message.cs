using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ChatBotClient.Core.Models
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
		private string _author;
		private string _text;

		public string Author
		{
			get => _author;
			set => _author = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Author cannot be empty");
		}

		public string Text
		{
			get => _text;
			set => _text = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Text cannot be empty");
		}

		public DateTime Timestamp { get; set; }

		public MessageStatus StatusCode { get; set; }

		[JsonIgnore]
		public bool IsUserMessage => Author == "User";

		public Message()
		{
			Timestamp = DateTime.Now;
			StatusCode = MessageStatus.None;
		}
	}
}