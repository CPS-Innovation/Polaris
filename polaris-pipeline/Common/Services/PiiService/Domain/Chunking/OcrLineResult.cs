using System.Collections.Generic;
using System.Linq;
using Common.Domain.Ocr;

namespace Common.Services.PiiService.Domain.Chunking
{
    public class OcrLineResult
    {
        private readonly OcrLineResult _previousLine;

        public OcrLineResult(Line line, ReadResult readResult, int accumulativeLineIndex, OcrLineResult previousLine)
        {
            _previousLine = previousLine;
            Text = $"{line.Text} ";
            PageIndex = readResult.Page;
            PageHeight = readResult.Height;
            PageWidth = readResult.Width;
            WordCount = line.Words.Count;
            AccumulativeLineIndex = accumulativeLineIndex;
            SetOffsetRange();
            SetLineIndex();
            AddWords(line.Words);
        }

        public string Text { get; protected set; }
        public int PageIndex { get; protected set; }
        public int LineIndex { get; protected set; }
        public double PageHeight { get; protected set; }
        public double PageWidth { get; protected set; }
        public int AccumulativeLineIndex { get; protected set; }
        public int WordCount { get; protected set; }
        public int TextLength => Text.Length;
        public (int Min, int Max) OffsetRange { get; protected set; }
        public List<OcrWord> Words { get; set; } = [];

        public bool ContainsOffset(int offset)
        {
            return OffsetRange.Min <= offset && OffsetRange.Max >= offset;
        }

        public OcrWord GetWord(string text, int offset)
        {
            return Words.Where(x => x.RelativeOffset.Min == offset)
                        .Where(x => x.Text.Contains(text))
                        .SingleOrDefault();
        }

        private void SetOffsetRange()
        {
            var zeroBasedTextLengthIndex = TextLength - 1;

            if (_previousLine == null)
                OffsetRange = (0, zeroBasedTextLengthIndex);
            else
            {
                var accumulativeOffset = _previousLine.OffsetRange.Max + 1;
                OffsetRange = (accumulativeOffset, accumulativeOffset + zeroBasedTextLengthIndex);
            }
        }

        private void SetLineIndex()
        {
            if (_previousLine == null || PageIndex != _previousLine.PageIndex)
                LineIndex = 1;
            else
                LineIndex = _previousLine.LineIndex + 1;
        }

        private void AddWords(IList<Word> words)
        {
            foreach (var word in words)
            {
                var offsetMin = !Words.Any() ? OffsetRange.Min : Words[^1].RelativeOffset.Max + 2;
                Words.Add(new OcrWord(word, offsetMin));
            }
        }
    }
}