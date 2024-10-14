using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using Common.Helpers;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.IdentityModel.Tokens;

namespace coordinator.Services.OcrResultsService
{
    public class OcrResultsService : IOcrResultsService
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public OcrResultsService(IPolarisBlobStorageService blobStorageService, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        public async Task<AnalyzeResults> GetOcrResultsFromBlob(int caseId, string documentId, Guid correlationId)
        {
            Stream jsonStream;

            try
            {
                var ocrBlobName = BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Ocr);
                jsonStream = await _blobStorageService.GetDocumentAsync(ocrBlobName, correlationId);
            }
            catch (Exception)
            {
                return null; // return null for now;
            }

            var streamReader = new StreamReader(jsonStream);

            return _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(await streamReader.ReadToEndAsync());
        }

        public List<PiiChunk> GetDocumentTextPiiChunks(AnalyzeResults analyzeResults, int caseId, string documentId, int characterLimit, Guid correlationId) // char limit should be a config value
        {
            var chunks = new List<PiiChunk>();
            var chunkId = 1;
            var currentCharacterLimit = characterLimit;
            var linesToProcessCount = analyzeResults.ReadResults.Sum(x => x.Lines.Count);
            var processedCount = 0;

            while (processedCount < linesToProcessCount)
            {
                var piiChunk = new PiiChunk(chunkId, caseId, documentId, currentCharacterLimit);
                piiChunk.BuildChunk(analyzeResults, ref processedCount);
                chunks.Add(piiChunk);
                chunkId++;
            }

            return chunks;
        }
    }

    public class PiiChunk
    {
        private readonly int _characterLimit;
        private string _text;

        public PiiChunk(int id, int caseId, string documentId, int characterLimit)
        {
            ChunkId = id;
            CaseId = caseId;
            DocumentId = documentId;
            _characterLimit = characterLimit;
        }

        public int ChunkId { get; protected set; }
        public int CaseId { get; protected set; }
        public string DocumentId { get; protected set; }
        public IList<OcrLineResult> Lines { get; set; } = new List<OcrLineResult>();
        public int LineCount => Lines.Count;
        public int WordCount => Lines.Sum(x => x.WordCount);
        public string Text
        {
            get { return _text.IsNullOrEmpty() ? null : _text.Trim(); }
            set { _text = value; }
        }
        public int TextLength => Text.IsNullOrEmpty() ? 0 : Text.Length;

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
        public List<OcrWord> Words { get; set; } = new List<OcrWord>();

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

    public class OcrWord : Word
    {
        public OcrWord(Word word, int accumulativeOffset) : base(word.BoundingBox, word.Text, word.Confidence)
        {
            SetOffsetRange(accumulativeOffset);
        }

        public int TextLength => Text.Length;
        public (int Min, int Max) RelativeOffset { get; protected set; }

        private void SetOffsetRange(int accumulativeOffset)
        {
            RelativeOffset = (accumulativeOffset, accumulativeOffset + TextLength - 1);
        }
    }
}