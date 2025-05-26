using ChatBotClient.Core.Models;
using ChatBotClient.Core;
using Serilog;

namespace ChatBotClient.Infrastructure.Services
{
	public class LocalModelService : ILocalModelService
	{
		public async Task<string> ProcessMessageAsync(string userId, string message, List<Message> history, string customPrompt, double temperature, double topP, int maxResponseLength)
		{
			try
			{
				Log.Information("Processing message locally for user {UserId}: {Message}", userId, message);
				string response = $"{customPrompt}: {message} (температура: {temperature}, topP: {topP}, макс. длина: {maxResponseLength})";
				return await Task.FromResult(response);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error processing message locally for user {UserId}", userId);
				throw;
			}
		}
	}
}