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

            //var result = OcrResultsService.GetPageText(readResult);

            //result.Should().Be(expectedResult);
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNull_TheOffsetStartsAtZero()
        {
            var text = "This is a test string of text";
            var ocrLine = new coordinator.Services.OcrResultsService.OcrLine(text, null);

            Assert.Equal(0, ocrLine.OffsetRange.Min);
            Assert.Equal(text.Length + 1, ocrLine.OffsetRange.Max);
            Assert.Equal(text.Length + 1, ocrLine.Length);
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNotNull_TheOffsetDoesNotStartAtZero()
        {
            var textLine1 = "This is a test string of text";
            var previousLine = new coordinator.Services.OcrResultsService.OcrLine(textLine1, null);

            var textLine2 = "along with some more text";
            var ocrLine = new coordinator.Services.OcrResultsService.OcrLine(textLine2, previousLine);

            Assert.Equal(textLine1.Length + 2, ocrLine.OffsetRange.Min);
            Assert.Equal(textLine1.Length + textLine2.Length + 2, ocrLine.OffsetRange.Max);
            Assert.Equal(textLine2.Length + 1, ocrLine.Length);
        }

        [Fact]
        public void WhenAddingALineToAPageResult_TheLineListIsUpdated_AndPropertiesPopulated()
        {
            var pageNumber = 1;
            var readResult = new ReadResult
            {
                Page = pageNumber,
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

            var result = new PageResult(readResult);

            result.Text.Should().Be(expectedResult);
            result.Lines.Count.Should().Be(result.LineCount);
            result.WordCount.Should().Be(8);
            result.PageNumber.Should().Be(pageNumber);
        }
    }
}