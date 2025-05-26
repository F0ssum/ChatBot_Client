namespace ChatBotClient.Infrastructure.Services
{
		public class CacheEntry<T>
		{
			public T Data { get; set; }
			public DateTime? Expiry { get; set; }
		}
}