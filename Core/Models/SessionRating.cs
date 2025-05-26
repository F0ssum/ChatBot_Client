// Core/Interfaces/SessionRating.cs
namespace ChatBotClient.Core.Models
{
	/// <summary>
	/// Represents a session rating entry.
	/// </summary>
	public class SessionRating
	{
		public DateTime Date { get; set; }
		public int Score { get; set; }
	}
}