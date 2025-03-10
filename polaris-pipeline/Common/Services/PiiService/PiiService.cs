using System;
using System.Collections.Generic;
using System.Linq;
using Azure.AI.TextAnalytics;
using Common.Domain.Pii;
using Common.Services.PiiService.Domain;
using Common.Dto.Request;
using Microsoft.Extensions.Configuration;
using Common.Services.PiiService.Mappers;
using Common.Services.PiiService.TextSanitization;
using Common.Services.PiiService.AllowedWords;
using Common.Services.PiiService.Domain.Chunking;
using Common.Services.PiiService.Chunking;
using Common.Services.PiiService.TextAnalytics;
using Common.Domain.Ocr;
using System.Threading.Tasks;

namespace Common.Services.PiiService
{
    public class PiiService : IPiiService
    {
        private readonly int PiiChunkCharacterLimit;
        private const int DocumentSize = 5;
        private readonly string[] _piiCategories;
        private readonly IPiiEntityMapper _piiEntityMapper;
        private readonly IPiiAllowedListService _piiAllowedList;
        private readonly ITextSanitizationService _textSanitizationService;
        private readonly IPiiChunkingService _piiChunkingService;
        private readonly ITextAnalysisClient _textAnalysisClient;

        public PiiService(
            IPiiEntityMapper piiEntityMapper,
            IConfiguration configuration,
            IPiiAllowedListService allowedList,
            ITextSanitizationService textSanitizationService,
            IPiiChunkingService piiChunkingService,
            ITextAnalysisClient textAnalysisClient)
        {
            _piiEntityMapper = piiEntityMapper ?? throw new ArgumentNullException(nameof(piiEntityMapper));
            _piiAllowedList = allowedList ?? throw new ArgumentNullException(nameof(allowedList));
            _piiCategories = configuration["PiiCategories"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            _textSanitizationService = textSanitizationService ?? throw new ArgumentNullException(nameof(textSanitizationService));
            _piiChunkingService = piiChunkingService ?? throw new ArgumentNullException(nameof(piiChunkingService));
            _textAnalysisClient = textAnalysisClient ?? throw new ArgumentNullException(nameof(textAnalysisClient));
            PiiChunkCharacterLimit = int.Parse(configuration[nameof(PiiChunkCharacterLimit)]);
        }

        public async Task<IEnumerable<PiiLine>> GetPiiResultsAsync(AnalyzeResults ocrResults, Guid correlationId)
        {
            var piiChunks = _piiChunkingService.GetDocumentTextPiiChunks(ocrResults, PiiChunkCharacterLimit);
            var piiRequests = CreatePiiRequests(piiChunks);

            var calls = piiRequests.Select(async piiRequest => await _textAnalysisClient.CheckForPii(piiRequest));
            var piiRequestResults = await Task.WhenAll(calls);

            var piiResultsWrapper = MapPiiResults(piiRequestResults);
            return ReconcilePiiResults(piiChunks, piiResultsWrapper);
        }

        private IEnumerable<PiiRequestDto> CreatePiiRequests(List<PiiChunk> piiChunks)
        {
            var piiRequests = new List<PiiRequestDto>();
            var processedCount = 0;
            var chunksToProcess = piiChunks.Count;

            while (processedCount < chunksToProcess)
            {
                var documents = piiChunks
                    .Select(x => x)
                    .Skip(processedCount)
                    .Take(DocumentSize).ToList();

                piiRequests.Add(new PiiRequestDto(_piiCategories, documents));
                processedCount += documents.Count;
            }

            return piiRequests;
        }

        private IEnumerable<PiiLine> ReconcilePiiResults(IList<PiiChunk> piiChunks, PiiEntitiesWrapper piiResults)
        {
            if (piiChunks == null || piiResults == null) return [];

            var results = new List<ReconciledPiiEntity>();

            var piiToProcess = piiResults.PiiResultCollection.SelectMany(result => result.Items).ToList();

            foreach (var (item, itemIndex) in piiToProcess.Select((item, itemIndex) => (item, itemIndex)))
            {
                foreach (var piiEntity in item.Entities)
                {
                    var entityGroupId = Guid.NewGuid();
                    var words = piiEntity.GetWordsWithOffset();
                    var chunk = piiChunks[itemIndex];

                    foreach (var (text, offset) in words)
                    {
                        if (_piiAllowedList.Contains(text, piiEntity.Category)) continue;

                        var chunkLine = chunk.Lines.Single(x => x.ContainsOffset(offset));
                        var ocrWord = chunkLine.GetWord(text, offset);
                        var redactionType = GetRedactionTypeCategoryMapping(piiEntity.Category);

                        if (ocrWord != null)
                            results.Add(new ReconciledPiiEntity(chunkLine, ocrWord, piiEntity.Category, redactionType, entityGroupId));
                    }
                }
            }

            return MapReconciledPiiToResponse(results);
        }

        private PiiEntitiesWrapper MapPiiResults(RecognizePiiEntitiesResultCollection[] piiResults)
        {
            return new PiiEntitiesWrapper
            {
                PiiResultCollection = piiResults.Select(result => _piiEntityMapper.MapCollection(result))
            };
        }

        private IEnumerable<PiiLine> MapReconciledPiiToResponse(List<ReconciledPiiEntity> piiEntities)
        {
            var results = new List<PiiLine>();

            foreach (var entity in piiEntities)
            {
                var piiLine = results.SingleOrDefault(x => x.AccumulativeLineIndex == entity.AccumulativeLineIndex);

                if (piiLine == null)
                {
                    piiLine = new PiiLine
                    {
                        PageIndex = entity.PageIndex,
                        LineIndex = entity.LineIndex,
                        AccumulativeLineIndex = entity.AccumulativeLineIndex,
                        PageHeight = entity.PageHeight,
                        PageWidth = entity.PageWidth,
                        Text = entity.LineText,
                        Id = Guid.NewGuid().ToString(),
                        Words = []
                    };

                    var lineWords = entity.LineText.Split(' ');
                    foreach (var lineWord in lineWords)
                    {
                        piiLine.Words.Add(new PiiWord { Text = lineWord, SanitizedText = _textSanitizationService.SanitizeText(lineWord) });
                    }

                    results.Add(piiLine);
                }

                var word = piiLine.Words
                    .Where(x => x.Text == entity.Word.Text)
                    .Where(x => x.BoundingBox == null)
                    .FirstOrDefault();

                var wordIndex = piiLine.Words.IndexOf(word);
                word = new PiiWord
                {
                    Text = entity.Word.Text,
                    BoundingBox = entity.Word.BoundingBox,
                    PiiCategory = entity.PiiCategory,
                    PiiGroupId = entity.EntityGroupId,
                    RedactionType = entity.RedactionType,
                    SanitizedText = _textSanitizationService.SanitizeText(entity.Word.Text)
                };
                if (wordIndex != -1)
                    piiLine.Words[wordIndex] = word;
            }

            return results;
        }

        private static string GetRedactionTypeCategoryMapping(string piiCategory)
        {
            PiiToRedactionLogCategoryMappings.TryGetValue(piiCategory, out var category);

            return category ?? "Other";
        }

        private static Dictionary<string, string> PiiToRedactionLogCategoryMappings =>
            new()
            {
                { PiiCategory.Address,                    "Address" },
                { PiiCategory.Email,                      "Email Address"},
                { PiiCategory.IPAddress,                  "Location" },
                { PiiCategory.Person,                     "Named Individual" },
                { PiiCategory.UKNationalHealthNumber,     "NHS number" },
                { PiiCategory.UKNationalInsuranceNumber,  "NI number" },
                { PiiCategory.PersonType,                 "Occupation" },
                { PiiCategory.PhoneNumber,                "Phone number" },
                { PiiCategory.CreditCardNumber,           "Other" },
                { PiiCategory.EUDriversLicenseNumber,     "Other" },
                { PiiCategory.UKDriversLicenseNumber,     "Other" },
                { PiiCategory.EUPassportNumber,           "Other" },
                { PiiCategory.USUKPassportNumber,         "Other" }
            };
    }
}