using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotClient.Models
{
	public class DiaryEntry
	{
		public string Title { get; set; } // "День X"
		public DateTime Date { get; set; }
		public string Content { get; set; } // Описание
		public List<string> Tags { get; set; } // ["#Позитив", "#Работа"]
	}
}
