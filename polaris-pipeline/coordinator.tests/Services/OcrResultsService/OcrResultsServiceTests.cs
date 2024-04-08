using System.Collections.Generic;
using coordinator.Services.OcrResultsService;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit;

namespace coordinator.tests.Services
{
    public class OcrResultsServiceTests
    {
        private readonly OcrResultsService _ocrResultsService;
        public OcrResultsServiceTests()
        {
            _ocrResultsService = new OcrResultsService();
        }

        [Fact]
        public void WhenGettingTheTextFromAResult_AContinuousStringIsReturned()
        {
            var readResult = new ReadResult
            {
                Lines = new List<Line> {
                new(null, "This is line 1",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "1", 1)
                }),
                new(null, "This is line 2",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "1", 1)
                })
            }
            };

            var expectedResult = $"{readResult.Lines[0].Text} {readResult.Lines[1].Text}";

            var result = OcrResultsService.GetPageText(readResult);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNull_TheOffsetStartsAtZero()
        {
            var text = "This is a test string of text";
            var ocrLine = new coordinator.Services.OcrResultsService.OcrLine(text, null);

            //ocrLine.
        }
    }
}