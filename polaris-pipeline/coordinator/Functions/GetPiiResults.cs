using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.SearchIndex;
using Common.Extensions;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Helpers;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace coordinator.Functions
{
    public class GetPiiResults
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrResultsService _ocrResultsService;
        private readonly IPiiService _piiService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public GetPiiResults(IPolarisBlobStorageService blobStorageService, IOcrResultsService ocrResultsService, IPiiService piiService, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobStorageService = blobStorageService ?? throw new System.ArgumentNullException(nameof(blobStorageService));
            _ocrResultsService = ocrResultsService ?? throw new System.ArgumentNullException(nameof(ocrResultsService));
            _piiService = piiService ?? throw new System.ArgumentNullException(nameof(piiService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new System.ArgumentNullException(nameof(jsonConvertWrapper));
        }

        [FunctionName(nameof(GetPiiResults))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.PiiResults)] HttpRequest req,
            string caseUrn,
            int caseId,
            string polarisDocumentId)
        {
            Guid currentCorrelationId = default;
            var searchResults = new List<StreamlinedSearchLine>();

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var ocrBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Ocr);
                using var ocrStream = await _blobStorageService.GetDocumentAsync(ocrBlobName, currentCorrelationId);

                // Need to handle if OCR results are null;
                var ocrStreamReader = new StreamReader(ocrStream);
                var ocrResults = _jsonConvertWrapper.DeserializeObject<AnalyzeResults>(ocrStreamReader.ReadToEnd());

                var piiChunks = _ocrResultsService.GetDocumentText(ocrResults, caseId, polarisDocumentId, 1000);

                var piiBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Pii);
                using var piiStream = await _blobStorageService.GetDocumentAsync(piiBlobName, currentCorrelationId);

                var piiStreamReader = new StreamReader(piiStream);
                var piiResults = _jsonConvertWrapper.DeserializeObject<PiiEntitiesWrapper>(piiStreamReader.ReadToEnd());

                var results = _piiService.ReconcilePiiResults(piiChunks, piiResults);

                foreach (var result in results)
                {
                    var searchLine = searchResults.SingleOrDefault(x => x.LineIndex == result.LineIndex);

                    if (searchLine == null)
                    {
                        searchLine = new StreamlinedSearchLine
                        {
                            PageIndex = result.PageIndex,
                            LineIndex = result.LineIndex,
                            Text = result.LineText,
                            Id = Guid.NewGuid().ToString(),
                            Words = new List<StreamlinedWord>()
                        };

                        var lineWords = result.LineText.Split(' ');
                        foreach (var lineWord in lineWords)
                        {
                            searchLine.Words.Add(new StreamlinedWord { Text = lineWord });
                        }

                        searchResults.Add(searchLine);
                    }

                    var word = searchLine.Words.FirstOrDefault(x => x.Text == result.Word.Text);
                    var wordIndex = searchLine.Words.IndexOf(word);
                    word = new StreamlinedWord
                    {
                        Text = result.Word.Text,
                        BoundingBox = result.Word.BoundingBox
                    };
                    if (wordIndex != -1)
                        searchLine.Words[wordIndex] = word;
                }

                return new OkObjectResult(searchResults);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}