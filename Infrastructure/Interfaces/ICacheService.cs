// Core/Interfaces/ICacheService.cs
namespace ChatBotClient.Infrastructure.Interfaces
{
	/// <summary>
	/// Provides methods for caching data with expiration.
	/// </summary>
	public interface ICacheService
	{
		/// <summary>
		/// Caches data with a specified key and expiration time.
		/// </summary>
		/// <typeparam name="T">The type of data to cache.</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="data">The data to cache.</param>
		/// <param name="expiry">The expiration time span.</param>
		void CacheData<T>(string key, T data, TimeSpan expiry);

		/// <summary>
		/// Retrieves cached data by key.
		/// </summary>
		/// <typeparam name="T">The type of data to retrieve.</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>The cached data, or default if not found or expired.</returns>
		T GetCachedData<T>(string key);

		/// <summary>
		/// Clears cached data for a specific key prefix.
		/// </summary>
		/// <param name="keyPrefix">The prefix of cache keys to clear.</param>
		void ClearCache(string keyPrefix);
	}
}