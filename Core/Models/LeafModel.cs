using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBotClient.Core.Models
{
    public class LeafModel
    {
        public int Id { get; set; } // Id записи дневника
        public string Title { get; set; }
        public string Emoji { get; set; }
        public double X { get; set; } // Позиция на дереве
        public double Y { get; set; }
    }
}