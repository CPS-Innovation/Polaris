
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using PolarisGateway.common.Mappers;
using PolarisGateway.Mappers;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;

namespace PolarisGateway.Tests.Mappers
{
    public class RedactPdfRequestMapperTests
    {
        private readonly Fixture _fixture;

        public RedactPdfRequestMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void GivenADocumentRedactionSaveRequest_ThenAValidPdfRequestObjectIsReturned()
        {
            var testRequest = _fixture.Create<DocumentRedactionSaveRequestDto>();
            testRequest.Redactions = _fixture.CreateMany<RedactionDefinitionDto>(5).ToList();

            IRedactPdfRequestMapper mapper = new RedactPdfRequestMapper();
            var result = mapper.Map(testRequest);

            using (new AssertionScope())
            {
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
