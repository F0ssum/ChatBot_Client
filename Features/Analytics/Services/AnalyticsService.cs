using ChatBotClient.Core.Configuration;
using ChatBotClient.Core.Models;
using Serilog;
using System;
using System.Data.SQLite;
using Microsoft.Extensions.Logging;

namespace ChatBotClient.Features.Services
{
	public class AnalyticsService
	{
		private readonly string _connectionString;

		public AnalyticsService(AppConfiguration config)
		{
			// Убедись, что AppConfiguration содержит AnalyticsDatabaseConnectionString
			_connectionString = config.AnalyticsDatabaseConnectionString
				?? throw new ArgumentNullException(nameof(config.AnalyticsDatabaseConnectionString));

			InitializeDatabase();
		}

		private void InitializeDatabase()
		{
			try
			{
				using var connection = new SQLiteConnection(_connectionString);
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS SessionRatings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId TEXT,
                        Date TEXT,
                        Score INTEGER
                    );
                    CREATE TABLE IF NOT EXISTS Points (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId TEXT,
                        Date TEXT,
                        Amount INTEGER,
                        Source TEXT
                    );
                ";
				command.ExecuteNonQuery();
				Log.Information("SQLite database initialized");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to initialize SQLite database");
				throw;
			}
		}

		public async Task<List<SessionRating>> GetWeeklyRatingsAsync(string userId)
		{
			var ratings = new List<SessionRating>();
			try
			{
				using var connection = new SQLiteConnection(_connectionString);
				await connection.OpenAsync();
				var command = connection.CreateCommand();
				command.CommandText = "SELECT Date, Score FROM SessionRatings WHERE UserId = @userId AND Date >= @startDate";
				command.Parameters.AddWithValue("@userId", userId);
				command.Parameters.AddWithValue("@startDate", DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd"));
				using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					ratings.Add(new SessionRating
					{
						Date = DateTime.Parse(reader.GetString(0)),
						Score = reader.GetInt32(1)
					});
				}
				Log.Information("Fetched weekly ratings for user {UserId}", userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to fetch weekly ratings");
				throw;
			}
			return ratings;
		}

		public async Task SaveSessionRatingAsync(string userId, int score)
		{
			try
			{
				using var connection = new SQLiteConnection(_connectionString);
				await connection.OpenAsync();
				var command = connection.CreateCommand();
				command.CommandText = "INSERT INTO SessionRatings (UserId, Date, Score) VALUES (@userId, @date, @score)";
				command.Parameters.AddWithValue("@userId", userId);
				command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
				command.Parameters.AddWithValue("@score", score);
				await command.ExecuteNonQueryAsync();
				Log.Information("Saved session rating {Score} for user {UserId}", score, userId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save session rating");
				throw;
			}
		}

		public async Task AddPointsAsync(string userId, int amount, string source)
		{
			try
			{
				using var connection = new SQLiteConnection(_connectionString);
				await connection.OpenAsync();
				var command = connection.CreateCommand();
				command.CommandText = "INSERT INTO Points (UserId, Date, Amount, Source) VALUES (@userId, @date, @amount, @source)";
				command.Parameters.AddWithValue("@userId", userId);
				command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
				command.Parameters.AddWithValue("@amount", amount);
				command.Parameters.AddWithValue("@source", source);
				await command.ExecuteNonQueryAsync();
				Log.Information("Added {Amount} points for user {UserId} from {Source}", amount, userId, source);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to add points");
				throw;
			}
		}

		public async Task<int> GetTotalPointsAsync(string userId)
		{
			try
			{
				using var connection = new SQLiteConnection(_connectionString);
				await connection.OpenAsync();
				var command = connection.CreateCommand();
				command.CommandText = "SELECT SUM(Amount) FROM Points WHERE UserId = @userId";
				command.Parameters.AddWithValue("@userId", userId);
				var result = await command.ExecuteScalarAsync();
				int points = result != DBNull.Value ? Convert.ToInt32(result) : 0;
				Log.Information("Retrieved {Points} total points for user {UserId}", points, userId);
				return points;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to retrieve total points");
				throw;
			}
		}
	}
}