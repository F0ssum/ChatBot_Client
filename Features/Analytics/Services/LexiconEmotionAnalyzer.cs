using System;
using System.Collections.Generic;
using System.Linq;
using ChatBotClient.Core.Models;

namespace ChatBotClient.Features.Analytics.Services
{
    public class LexiconEmotionAnalyzer
    {
        private readonly Dictionary<string, string> _emotionLexicon = new()
        {
            { "радость", "радость" }, { "счастлив", "радость" }, { "грусть", "грусть" },
            { "печаль", "грусть" }, { "злюсь", "гнев" }, { "боюсь", "страх" }, { "апатия", "апатия" }
            // ...добавьте больше слов
        };

        private readonly string[] _sarcasmPatterns = { "ага", "конечно", "ну да", "очень весело", "спасибо, смешно" };

        public EmotionAnalysisResult Analyze(string text)
        {
            var words = text.ToLower().Split(' ', '.', ',', '!', '?', ':', ';');
            var found = words.Select(w => _emotionLexicon.TryGetValue(w, out var emotion) ? emotion : null)
                             .Where(e => e != null)
                             .GroupBy(e => e)
                             .OrderByDescending(g => g.Count())
                             .FirstOrDefault();

            bool isSarcasm = _sarcasmPatterns.Any(p => text.ToLower().Contains(p));

            if (found != null)
            {
                return new EmotionAnalysisResult
                {
                    Emotion = found.Key,
                    Confidence = found.Count() / (double)words.Length,
                    IsSarcasm = isSarcasm,
                    Source = "lexicon"
                };
            }
            return new EmotionAnalysisResult { Emotion = "неопределено", Confidence = 0, IsSarcasm = isSarcasm, Source = "lexicon" };
        }
    }
}