using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotClient.Core.Models
{
	public class MoodStatsResponse
	{
		[JsonProperty("mood_stats")]
		public Dictionary<string, int> MoodStats { get; set; }
	}
}
