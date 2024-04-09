using System.Collections.Generic;
using System.Linq;
using coordinator.Services.OcrResultsService;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit;

namespace coordinator.tests.Services
{
    public class OcrResultsServiceTests
    {
        private readonly OcrResultsService _ocrResultsService;
        private readonly Line _ocrLine1;
        private readonly Line _ocrLine2;
        private readonly Line _ocrLine3;

        public OcrResultsServiceTests()
        {
            _ocrResultsService = new OcrResultsService();
            _ocrLine1 = new(null, "This is line 1",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "1", 1)
                    });
            _ocrLine2 = new(null, "This is line 2",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "line", 1),
                        new(null, "2", 1)
                    });
            _ocrLine3 = new(null, "This is the third line of OCR analysis",
                    new List<Word> {
                        new(null, "This", 1),
                        new(null, "is", 1),
                        new(null, "the", 1),
                        new(null, "third", 1),
                        new(null, "line", 1),
                        new(null, "of", 1),
                        new(null, "OCR", 1),
                        new(null, "analysis", 1)
                    });
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNull_TheOffsetStartsAtZero()
        {
            var ocrLine = new OcrLineResult(_ocrLine1, 1, null);

            Assert.Equal(0, ocrLine.OffsetRange.Min);
            Assert.Equal(_ocrLine1.Text.Length + 1, ocrLine.OffsetRange.Max);
            Assert.Equal(_ocrLine1.Text.Length + 1, ocrLine.TextLength);
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNotNull_TheOffsetDoesNotStartAtZero()
        {
            var previousLine = new OcrLineResult(_ocrLine1, 1, null);

            var ocrLine = new OcrLineResult(_ocrLine2, 1, previousLine);

            Assert.Equal(_ocrLine1.Text.Length + 2, ocrLine.OffsetRange.Min);
            Assert.Equal(_ocrLine1.Text.Length + _ocrLine2.Text.Length + 2, ocrLine.OffsetRange.Max);
            Assert.Equal(_ocrLine2.Text.Length + 1, ocrLine.TextLength);
        }

        [Fact]
        public void WhenAddingALineToAPageResult_TheLineListIsUpdated_AndPropertiesPopulated() // rename
        {
            var pageNumber = 1;
            var readResult = new ReadResult
            {
                Page = pageNumber,
                Lines = new List<Line> {
                    _ocrLine1,
                    _ocrLine2
                }
            };
            var analyzeResults = new AnalyzeResults
            {
                ReadResults = new List<ReadResult> { readResult }
            };

            var expectedResult = $"{readResult.Lines[0].Text} {readResult.Lines[1].Text}";

            var result = new PiiChunk(1, 100, 0);
            result.BuildChunk(analyzeResults);
            result.SetChunkText();

            result.Text.Should().Be(expectedResult);
            result.Lines.Count.Should().Be(result.LineCount);
            result.WordCount.Should().Be(8);
        }

        [Fact]
        public void WhenGettingTheTextFromAResult_AContinuousStringIsReturned()
        {
            var characterLimit = 100;
            var expectedChunk1Text = $"{_ocrLine1.Text} {_ocrLine2.Text} {_ocrLine3.Text}";

            var readResults = new List<ReadResult>
            {
                new() {
                    Page = 1,
                    Lines = new List<Line> {
                        _ocrLine1,
                        _ocrLine2
                    }
                },
                new() {
                    Page = 2,
                    Lines = new List<Line> {
                        _ocrLine3
                    }
                }
            };
            var analyzeResults = new AnalyzeResults
            {
                ReadResults = readResults
            };

            var results = _ocrResultsService.GetDocumentText(analyzeResults, characterLimit);

            results[0].Text.Should().Be(expectedChunk1Text);
        }

        [Fact]
        public void WhenChunkingAnalyzeResults_MultipleChunksAreCreated()
        {
            var characterLimit = 50;
            var expectedChunk1Text = $"{_ocrLine1.Text} {_ocrLine2.Text}";
            var expectedChunk2Text = $"{_ocrLine3.Text}";

            var readResult = new ReadResult
            {
                Page = 1,
                Lines = new List<Line> {
                    _ocrLine1,
                    _ocrLine2,
                    _ocrLine3
                }
            };
            var analyzeResults = new AnalyzeResults
            {
                ReadResults = new List<ReadResult> { readResult }
            };

            var results = _ocrResultsService.GetDocumentText(analyzeResults, characterLimit);

            results.Count.Should().Be(2);
            results.All(x => x.TextLength < characterLimit).Should().BeTrue();
            results[0].Text.Should().Be(expectedChunk1Text);
            results[0].WordCount.Should().Be(8);
            results[1].Text.Should().Be(expectedChunk2Text);
            results[1].WordCount.Should().Be(8);
        }

        [Fact]
        public void WhenChunkingAnalyzeResults_WithMultiplePages_ThePageIndexesAreSetCorrectly()
        {
            var characterLimit = 100;
            var expectedChunk1Text = $"{_ocrLine1.Text} {_ocrLine2.Text} {_ocrLine3.Text}";

            var readResults = new List<ReadResult>
            {
                new() {
                    Page = 1,
                    Lines = new List<Line> {
                        _ocrLine1,
                        _ocrLine2
                    }
                },
                new() {
                    Page = 2,
                    Lines = new List<Line> {
                        _ocrLine3
                    }
                }
            };
            var analyzeResults = new AnalyzeResults
            {
                ReadResults = readResults
            };

            var results = _ocrResultsService.GetDocumentText(analyzeResults, characterLimit);

            results.Count.Should().Be(1);
            results.All(x => x.TextLength < characterLimit).Should().BeTrue();
            results[0].Text.Should().Be(expectedChunk1Text);
            results[0].WordCount.Should().Be(16);
            results[0].Lines[0].PageIndex.Should().Be(1);
            results[0].Lines[1].PageIndex.Should().Be(1);
            results[0].Lines[2].PageIndex.Should().Be(2);
        }
    }
}