// Core/Interfaces/IAnalyticsService.cs

// Core/Interfaces/IAnalyticsService.cs
using ChatBotClient.Core.Models;

namespace ChatBotClient.Infrastructure.Interfaces
{
	/// <summary>
	/// Provides methods for managing session ratings and points in a SQLite database.
	/// </summary>
	public interface IAnalyticsService
	{
		/// <summary>
		/// Asynchronously saves a session rating for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="score">The session rating score.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SaveSessionRatingAsync(string userId, int score);

		/// <summary>
		/// Asynchronously retrieves weekly session ratings for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>A list of tuples containing the date and score of each rating.</returns>
		Task<List<SessionRating>> GetWeeklyRatingsAsync(string userId);

		/// <summary>
		/// Asynchronously adds points for a user from a specified source.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="amount">The number of points to add.</param>
		/// <param name="source">The source of the points.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task AddPointsAsync(string userId, int amount, string source);

		/// <summary>
		/// Asynchronously retrieves the total points for a user.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns>The total number of points.</returns>
		Task<int> GetTotalPointsAsync(string userId);
	}
}