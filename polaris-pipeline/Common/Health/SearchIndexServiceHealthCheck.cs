using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Services.CaseSearchService.Contracts;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    public class SearchIndexServiceHealthCheck : IHealthCheck
    {
        private readonly ISearchIndexService _searchIndexService;

        #region OcrResults
        readonly string _ocrResult =
            @"
            {
                ""Version"":""3.2.0"",
                ""ModelVersion"":""2022-04-30"",
                ""ReadResults"":
                [
                    {
                        ""Page"":1,
                        ""Language"":null,
                        ""Angle"":0,
                        ""Width"":8.2639,
                        ""Height"":11.6806,
                        ""Unit"":1,
                        ""Lines"":
                        [
                            {
                                ""Language"":null,
                                ""BoundingBox"":[1.0011,1.0271,2.8298,1.0271,2.8298,1.16,1.0011,1.16],
                                ""Appearance"":
                                {
                                    ""Style"":
                                    {
                                        ""Name"":""other"",
                                        ""Confidence"":1
                                    }
                                },
                                ""Text"":""TestDOCX top"",
                                ""Words"":
                                [
                                    {
                                        ""BoundingBox"":[1.0011,1.035,1.6136,1.035,1.6136,1.1342,1.0011,1.1342],
                                        ""Text"":""TestDOCX"",
                                        ""Confidence"":1
                                    },
                                    {
                                        ""BoundingBox"":[1.6543,1.0437,1.8568,1.0437,1.8568,1.16,1.6543,1.16],
                                        ""Text"":""top"",
                                        ""Confidence"":1
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }";
        #endregion


        public SearchIndexServiceHealthCheck(ISearchIndexService searchIndexService)
        {
            _searchIndexService = searchIndexService;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<HealthCheckResult> CheckHealthAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                AnalyzeResults analyzeResult = JsonSerializer.Deserialize<AnalyzeResults>(_ocrResult);
                await _searchIndexService.SendStoreResultsAsync
                (
                    analyzeResult,
                    default(PolarisDocumentId),
                    default(long),
                    nameof(SearchIndexServiceHealthCheck),
                    default(long),
                    nameof(SearchIndexServiceHealthCheck)
                );

                return HealthCheckResult.Healthy(string.Empty);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy($"{e.Message}", e);
            }
        }
    }
}