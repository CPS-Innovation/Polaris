using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Services.OcrResultsService;
using Microsoft.Extensions.Configuration;

namespace coordinator.Services.PiiService
{
    public class PiiService : IPiiService
    {
        private const int DocumentSize = 5;
        private readonly string[] _piiCategories;
        private readonly IConfiguration _configuration;
        private readonly IPiiEntityMapper _piiEntityMapper;

        public PiiService(IConfiguration configuration, IPiiEntityMapper piiEntityMapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _piiEntityMapper = piiEntityMapper ?? throw new ArgumentNullException(nameof(piiEntityMapper));
            // To come for config value...
            var piiCategoriesConfigValue = ""; //"Person;PersonType;PhoneNumber;Organization;Address;Email;";
            _piiCategories = piiCategoriesConfigValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<PiiRequestDto> CreatePiiRequests(List<PiiChunk> piiChunks)
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

        public List<ReconciledPiiEntity> ReconcilePiiResults(IList<PiiChunk> piiChunks, PiiEntitiesWrapper piiResults)
        {
            var results = new List<ReconciledPiiEntity>();

            var piiToProcess = piiResults.PiiResultCollection.SelectMany(result => result.Items).ToList();

            foreach (var (item, itemIndex) in piiToProcess.Select((item, itemIndex) => (item, itemIndex)))
            {
                foreach (var piiEntity in item.Entities)
                {
                    var words = piiEntity.GetWordsWithOffset();
                    var chunk = piiChunks[itemIndex];

                    foreach (var (text, offset) in words)
                    {
                        var chunkLine = chunk.Lines.Where(x => x.ContainsOffset(offset)).SingleOrDefault();
                        var ocrWord = chunkLine.GetWord(text, offset);

                        if (ocrWord != null)
                            results.Add(new ReconciledPiiEntity(chunkLine, ocrWord, piiEntity.Category));
                    }
                }
            }

            return results;
        }

        public PiiEntitiesWrapper MapPiiResults(RecognizePiiEntitiesResultCollection[] piiResults)
        {
            return new PiiEntitiesWrapper
            {
                PiiResultCollection = piiResults.Select(result => _piiEntityMapper.MapCollection(result))
            };
        }
    }
}