using System.Threading.Tasks;

namespace ChatBotClient.Infrastructure.Interfaces
{
	/// <summary>
	/// Provides methods for saving and loading encrypted data to local storage.
	/// </summary>
	public interface ILocalStorageService
	{
		/// <summary>
		/// Asynchronously saves data to local storage with the specified key.
		/// </summary>
		/// <typeparam name="T">The type of data to save.</typeparam>
		/// <param name="key">The storage key.</param>
		/// <param name="data">The data to save.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SaveDataAsync<T>(string key, T data);

		/// <summary>
		/// Asynchronously loads data from local storage by key.
		/// </summary>
		/// <typeparam name="T">The type of data to load.</typeparam>
		/// <param name="key">The storage key.</param>
		/// <returns>The loaded data, or default if not found.</returns>
		Task<T> LoadDataAsync<T>(string key);

		/// <summary>
		/// Checks if data exists under the specified key.
		/// </summary>
		/// <param name="key">The storage key.</param>
		/// <returns>True if data exists; otherwise, false.</returns>
		Task<bool> ContainsDataAsync(string key);

		/// <summary>
		/// Removes data associated with the specified key.
		/// </summary>
		/// <param name="key">The storage key.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task RemoveDataAsync(string key);

		/// <summary>
		/// Clears all stored data.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task ClearAllDataAsync();
	}
}