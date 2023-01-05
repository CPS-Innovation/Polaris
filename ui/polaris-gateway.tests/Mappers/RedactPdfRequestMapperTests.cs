using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using RumpoleGateway.Domain.DocumentRedaction;
using RumpoleGateway.Mappers;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RumpoleGateway.Tests.Mappers
{
    public class RedactPdfRequestMapperTests
    {
        private readonly Fixture _fixture;
        private readonly Guid _correlationId;
        private readonly Mock<ILogger<RedactPdfRequestMapper>> _loggerMock;

        public RedactPdfRequestMapperTests()
        {
            _fixture = new Fixture();
            _correlationId = _fixture.Create<Guid>();
            _loggerMock = new Mock<ILogger<RedactPdfRequestMapper>>();
        }

        [Fact]
        public void GivenADocumentRedactionSaveRequest_ThenAValidPdfRequestObjectIsReturned()
        {
            var testRequest = _fixture.Create<DocumentRedactionSaveRequest>();
            testRequest.Redactions = _fixture.CreateMany<RedactionDefinition>(5).ToList();
            var testCaseId = _fixture.Create<int>();
            var testDocumentId = _fixture.Create<int>();
            var testFileName = _fixture.Create<string>();

            IRedactPdfRequestMapper mapper = new RedactPdfRequestMapper(_loggerMock.Object);
            var result = mapper.Map(testRequest, testCaseId, testDocumentId, testFileName, _correlationId);

            using (new AssertionScope())
            {
                result.CaseId.Should().Be(testCaseId);
                result.DocumentId.Should().Be(testDocumentId);
                result.FileName.Should().Be(testFileName);
                result.RedactionDefinitions.Should().NotBeNull();
                result.RedactionDefinitions.Count.Should().Be(5);
                result.RedactionDefinitions[0].PageIndex.Should().Be(testRequest.Redactions[0].PageIndex);
                result.RedactionDefinitions[0].Width.Should().Be(testRequest.Redactions[0].Width);
                result.RedactionDefinitions[0].RedactionCoordinates[0].X1.Should().Be(testRequest.Redactions[0].RedactionCoordinates[0].X1);
                result.RedactionDefinitions[0].RedactionCoordinates[0].Y2.Should().Be(testRequest.Redactions[0].RedactionCoordinates[0].Y2);
            }
        }
    }
}
