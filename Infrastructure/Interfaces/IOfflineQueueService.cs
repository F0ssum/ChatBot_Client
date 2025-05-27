namespace ChatBotClient.Infrastructure.Interfaces
{
	/// <summary>
	/// Provides methods for queuing actions to be synchronized when online.
	/// </summary>
	public interface IOfflineQueueService
	{
		/// <summary>
		/// Queues an action with associated data for later synchronization.
		/// </summary>
		/// <param name="action">The action identifier.</param>
		/// <param name="data">The data associated with the action.</param>
		void QueueAction(string action, object data);

		/// <summary>
		/// Asynchronously synchronizes the queued actions with the server.
		/// </summary>
		/// <param name="apiService">The API service to use for synchronization.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SyncQueueAsync(IApiService apiService);
	}
}