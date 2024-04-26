using System.Collections.Generic;
using System.Linq;
using coordinator.Services.OcrResultsService;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit;

namespace coordinator.tests.Services.OcrResultsServiceTests
{
    public class OcrResultsServiceTests
    {
        private readonly ReadResult _readResult;
        private readonly OcrResultsService _ocrResultsService;
        private readonly Line _ocrLine1;
        private readonly Line _ocrLine2;
        private readonly Line _ocrLine3;
        private const int CaseId = 123456;
        private const string DocumentId = "CMS-1000";

        public OcrResultsServiceTests()
        {
            _readResult = new ReadResult();
            _ocrResultsService = new OcrResultsService();
            _ocrLine1 = Mother.OcrLine1();
            _ocrLine2 = Mother.OcrLine2();
            _ocrLine3 = Mother.OcrLine3();
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNull_TheOffsetStartsAtZero()
        {
            var ocrLine = new OcrLineResult(_ocrLine1, _readResult, 1, null);

            ocrLine.OffsetRange.Min.Should().Be(0);
            ocrLine.OffsetRange.Max.Should().Be(_ocrLine1.Text.Length);
            ocrLine.TextLength.Should().Be(_ocrLine1.Text.Length + 1);
        }

        [Fact]
        public void WhenInstantiatingAnOcrLine_AndThePreviousLineIsNotNull_TheOffsetDoesNotStartAtZero()
        {
            var previousLine = new OcrLineResult(_ocrLine1, _readResult, 1, null);

            var ocrLine = new OcrLineResult(_ocrLine2, _readResult, 1, previousLine);

            ocrLine.OffsetRange.Min.Should().Be(_ocrLine1.Text.Length + 1);
            ocrLine.OffsetRange.Max.Should().Be(_ocrLine1.Text.Length + _ocrLine2.Text.Length + 1);
            ocrLine.TextLength.Should().Be(_ocrLine2.Text.Length + 1);
        }

        [Fact]
        public void WhenAddingAOcrLineResult_TheLineIndexIsSetCorrectly()
        {
            var previousLine = new OcrLineResult(_ocrLine1, _readResult, 1, null);

            var ocrLine = new OcrLineResult(_ocrLine2, _readResult, 1, previousLine);

            ocrLine.LineIndex.Should().Be(2);
        }

        [Fact]
        public void WhenCreatingAPiiChunk_TheLineListIsUpdated_AndPropertiesPopulated()
        {
            var pageNumber = 1;
            var processedCount = 0;

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

            var result = new PiiChunk(1, CaseId, DocumentId, 100);
            result.BuildChunk(analyzeResults, ref processedCount);

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

            var results = _ocrResultsService.GetDocumentTextPiiChunks(analyzeResults, CaseId, DocumentId, characterLimit);

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

            var results = _ocrResultsService.GetDocumentTextPiiChunks(analyzeResults, CaseId, DocumentId, characterLimit);

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

            var results = _ocrResultsService.GetDocumentTextPiiChunks(analyzeResults, CaseId, DocumentId, characterLimit);

            results.Count.Should().Be(1);
            results.All(x => x.TextLength < characterLimit).Should().BeTrue();
            results[0].Text.Should().Be(expectedChunk1Text);
            results[0].WordCount.Should().Be(16);
            results[0].Lines[0].PageIndex.Should().Be(1);
            results[0].Lines[1].PageIndex.Should().Be(1);
            results[0].Lines[2].PageIndex.Should().Be(2);
        }

        [Fact]
        public void WhenChunkingAnalyzeResults_WordsAreAddedToTheirRespectiveLines_AndOffsetsAreSetCorrectly()
        {
            var pageNumber = 1;
            var processedCount = 0;

            var expectedOffsetForLine1Word1 = (0, _ocrLine1.Words[0].Text.Length - 1);
            var accumulativeOffset = expectedOffsetForLine1Word1.Item2 + 2;
            var expectedOffsetForLine1Word2 = (accumulativeOffset, accumulativeOffset + _ocrLine1.Words[1].Text.Length - 1);
            accumulativeOffset = expectedOffsetForLine1Word2.Item2 + 2;
            var expectedOffsetForLine1Word3 = (accumulativeOffset, accumulativeOffset + _ocrLine1.Words[2].Text.Length - 1);
            accumulativeOffset = expectedOffsetForLine1Word3.Item2 + 2;
            var expectedOffsetForLine1Word4 = (accumulativeOffset, accumulativeOffset + _ocrLine1.Words[3].Text.Length - 1);
            accumulativeOffset = expectedOffsetForLine1Word4.Item2 + 2;
            var expectedOffsetForLine2Word1 = (accumulativeOffset, accumulativeOffset + _ocrLine2.Words[0].Text.Length - 1);

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

            var result = new PiiChunk(1, CaseId, DocumentId, 100);
            result.BuildChunk(analyzeResults, ref processedCount);

            result.Lines.Count.Should().Be(result.LineCount);
            result.Lines[0].Words.Count.Should().Be(_ocrLine1.Words.Count);
            result.Lines[0].Words[0].RelativeOffset.Should().Be(expectedOffsetForLine1Word1);
            result.Lines[0].Words[1].RelativeOffset.Should().Be(expectedOffsetForLine1Word2);
            result.Lines[0].Words[2].RelativeOffset.Should().Be(expectedOffsetForLine1Word3);
            result.Lines[0].Words[3].RelativeOffset.Should().Be(expectedOffsetForLine1Word4);
            result.Lines[1].Words[0].RelativeOffset.Should().Be(expectedOffsetForLine2Word1);
        }
    }
}