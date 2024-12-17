using System.Collections.Generic;
using System.Linq;
using Common.Domain.Ocr;

namespace Common.Services.PiiService.Domain.Chunking
{
    public class PiiChunk
    {
        private readonly int _characterLimit;
        private string _text;

        public PiiChunk(int id, int characterLimit)
        {
            ChunkId = id;
            _characterLimit = characterLimit;
        }

        public int ChunkId { get; protected set; }
        public IList<OcrLineResult> Lines { get; set; } = new List<OcrLineResult>();
        public int LineCount => Lines.Count;
        public int WordCount => Lines.Sum(x => x.WordCount);
        public string Text
        {
            get { return string.IsNullOrEmpty(_text) ? null : _text.Trim(); }
            set { _text = value; }
        }
        public int TextLength => string.IsNullOrEmpty(Text) ? 0 : Text.Length;

        public void BuildChunk(AnalyzeResults analyzeResults, ref int processedCount)
        {
            var itemsToProcess = analyzeResults.ReadResults
                .SelectMany(result => result.Lines.Select(line => new { result, line }));

            foreach (var item in itemsToProcess.Skip(processedCount))
            {
                if (TextLength + item.line.Text.Length > _characterLimit)
                    break;

                AddLine(item.line, item.result, processedCount + 1);
                processedCount++;
            }
        }

        public void AddLine(Line line, ReadResult readResult, int accumulativeLineIndex)
        {
            var ocrLine = new OcrLineResult(line, readResult, accumulativeLineIndex, Lines.LastOrDefault());
            _text += ocrLine.Text;
            Lines.Add(ocrLine);
        }
    }
}