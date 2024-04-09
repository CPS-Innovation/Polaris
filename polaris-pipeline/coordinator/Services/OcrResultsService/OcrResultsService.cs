using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.IdentityModel.Tokens;

namespace coordinator.Services.OcrResultsService
{
    public class OcrResultsService : IOcrResultsService
    {
        private const int CharacterLimit = 1000;

        public List<PiiChunk> GetDocumentText(AnalyzeResults analyzeResults, int characterLimit) // char limit should be a config value
        {
            var chunks = new List<PiiChunk>();
            var chunkId = 1;
            var currentCharacterLimit = characterLimit;
            var linesToProcessCount = analyzeResults.ReadResults.Sum(x => x.Lines.Count);
            var processedCount = 0;

            while (processedCount < linesToProcessCount)
            {
                var piiChunk = new PiiChunk(chunkId, currentCharacterLimit, processedCount);
                processedCount = piiChunk.BuildChunk(analyzeResults);
                piiChunk.SetChunkText();
                chunks.Add(piiChunk);
                chunkId++;
            }

            return chunks;
        }
    }

    public class PiiChunk
    {
        private int _remainingCharacterLimit;
        private int _processedCount;

        public PiiChunk(int id, int characterLimit, int processedCount)
        {
            ChunkId = id;
            _remainingCharacterLimit = characterLimit;
            _processedCount = processedCount;
        }

        public int ChunkId { get; set; }
        public IList<OcrLineResult> Lines { get; set; } = new List<OcrLineResult>();
        public int LineCount => Lines.Count;
        public int WordCount => Lines.Sum(x => x.WordCount);
        public string Text { get; set; }
        public int TextLength => Text.IsNullOrEmpty() ? 0 : Text.Length;

        public int BuildChunk(AnalyzeResults analyzeResults)
        {
            var resultsToProcess = analyzeResults.ReadResults
                .SelectMany(result => result.Lines.Select(line => new { result, line })
                .Select(x =>
                    new { x.result.Page, x.line }
                ));

            foreach (var result in resultsToProcess.Skip(_processedCount))
            {
                if (TextLength + result.line.Text.Length <= _remainingCharacterLimit)
                {
                    AddLine(result.line, result.Page);
                    _remainingCharacterLimit -= result.line.Text.Length;
                    _processedCount++;
                }
                else
                {
                    return _processedCount;
                }
            }

            return _processedCount;
        }

        public void AddLine(Line line, int pageIndex)
        {
            var previousLine = Lines.LastOrDefault();
            var ocrLine = new OcrLineResult(line, pageIndex, previousLine);
            Lines.Add(ocrLine);
        }

        public void SetChunkText()
        {
            var sb = new StringBuilder();
            foreach (var line in Lines)
            {
                sb.AppendFormat($"{line.Text}");
            }

            Text = sb.ToString().TrimEnd();
        }
    }

    public class OcrLineResult
    {
        private readonly OcrLineResult _previousLine;

        public OcrLineResult(Line line, int pageIndex, OcrLineResult previousLine)
        {
            _previousLine = previousLine;
            Text = $"{line.Text} ";
            PageIndex = pageIndex;
            WordCount = line.Words.Count;
            SetOffsetRange();
            SetLineIndex();
        }

        public string Text { get; protected set; }
        public int PageIndex { get; protected set; }
        public int LineIndex { get; protected set; }
        public int WordCount { get; protected set; }
        public int TextLength => Text.Length;
        public (int Min, int Max) OffsetRange { get; protected set; }

        private void SetOffsetRange()
        {
            if (_previousLine == null)
            {
                OffsetRange = (0, Text.Length);
            }
            else
            {
                var accumulativeOffset = _previousLine.OffsetRange.Max;
                OffsetRange = (accumulativeOffset + 1, accumulativeOffset + TextLength);
            }
        }

        private void SetLineIndex()
        {
            if (_previousLine == null || PageIndex != _previousLine.PageIndex)
                LineIndex = 1;
            else
                LineIndex = _previousLine.LineIndex + 1;
        }
    }
}