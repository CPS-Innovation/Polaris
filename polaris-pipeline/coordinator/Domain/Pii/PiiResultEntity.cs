using System.Collections.Generic;

namespace coordinator.Domain
{
    public class PiiResultEntity
    {
        public string Text { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public double ConfidenceScore { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public List<(string Text, int Offset)> GetWordsWithOffset()
        {
            var wordsInText = new List<(string, int)>();
            var words = Text.Split(" ");
            var offset = Offset;

            foreach (var word in words)
            {
                wordsInText.Add((word, offset));
                offset += word.Length + 1;
            }

            return wordsInText;
        }
    }
}