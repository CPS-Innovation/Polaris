using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;

namespace PolarisGateway.Tests.Mappers
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
            var testRequest = _fixture.Create<DocumentRedactionSaveRequestDto>();
            testRequest.Redactions = _fixture.CreateMany<RedactionDefinitionDto>(5).ToList();
            var testCaseId = _fixture.Create<int>();
            var testDocumentId = _fixture.Create<Guid>();

            IRedactPdfRequestMapper mapper = new RedactPdfRequestMapper(_loggerMock.Object);
            var result = mapper.Map(testRequest, testCaseId, testDocumentId, _correlationId);

            using (new AssertionScope())
            {
                result.CaseId.Should().Be(testCaseId);
                result.DocumentId.Should().Be(testDocumentId.ToString());
                result.FileName.Should().BeNullOrEmpty();
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
