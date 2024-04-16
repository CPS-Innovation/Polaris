using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using coordinator.Domain;
using coordinator.Services.OcrResultsService;
using Microsoft.Extensions.Configuration;

namespace coordinator.Services.PiiService
{
    public class PiiService : IPiiService
    {
        private const int DocumentSize = 5;
        private readonly string[] _piiCategories;

        public PiiService(IConfiguration configuration)
        {
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

        public Task ReconcilePiiResults(IList<PiiChunk> piiChunks, RecognizePiiEntitiesResultCollection piiResults)
        {
            foreach (var piiResult in piiResults)
            {
                foreach (var piiEntity in piiResult.Entities)
                {
                    var chunk = piiChunks.Single(x => x.ChunkId.ToString() == piiResult.Id);
                    var chunkLine = chunk.Lines.Select(x => x.ContainsOffset(piiEntity.Offset)).SingleOrDefault();



                }
            }

            return Task.CompletedTask;
        }
    }
}