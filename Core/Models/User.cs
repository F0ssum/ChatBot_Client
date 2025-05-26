namespace ChatBotClient.Core.Models
{
	public class User
	{
		private string _id;
		private string _name;

		public string Id
		{
			get => _id;
			set => _id = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Id cannot be empty");
		}

		public string Name
		{
			get => _name;
			set => _name = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Name cannot be empty");
		}
	}
}