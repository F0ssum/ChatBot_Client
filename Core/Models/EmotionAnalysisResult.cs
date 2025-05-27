using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBotClient.Core.Models
{
    public class EmotionAnalysisResult
    {
        public string Emotion { get; set; } // "радость", "грусть" и т.д.
        public double Confidence { get; set; } // 0..1
        public bool IsSarcasm { get; set; }
        public string Source { get; set; } // "lexicon", "neural", "hybrid"
    }
}