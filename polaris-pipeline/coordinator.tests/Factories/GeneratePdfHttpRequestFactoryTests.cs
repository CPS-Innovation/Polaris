using System;
using System.Net.Http;
using AutoFixture;
using Common.Constants;
using Common.Dto.Request;
using Common.Wrappers.Contracts;
using coordinator.Factories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Factories
{
    public class GeneratePdfHttpRequestFactoryTests
    {
        private readonly string _caseUrn;
        private readonly long _caseId;
        private readonly string _documentCategory;
        private readonly string _documentId;
        private readonly string _fileName;
        private readonly long _versionId;
        private readonly string _cmsAuthValues;
        private readonly string _content;
        private readonly string _pdfGeneratorUrl;
        private readonly Guid _correlationId;

        private readonly GeneratePdfHttpRequestFactory _generatePdfHttpRequestFactory;

        public GeneratePdfHttpRequestFactoryTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _documentCategory = fixture.Create<string>();
            _documentId = fixture.Create<string>();
            _fileName = fixture.Create<string>();
            _versionId = fixture.Create<long>();
            _cmsAuthValues = fixture.Create<string>();
            _content = fixture.Create<string>();
            _pdfGeneratorUrl = "https://www.generate.pdf";
            _correlationId = fixture.Create<Guid>();

            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            var mockConfiguration = new Mock<IConfiguration>();
            
            mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfRequestDto>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.FileName == _fileName)))
                .Returns(_content);

            var mockLogger = new Mock<ILogger<GeneratePdfHttpRequestFactory>>();

            mockConfiguration.Setup(config => config[PipelineSettings.PipelineRedactPdfBaseUrl]).Returns(_pdfGeneratorUrl);
            
            _generatePdfHttpRequestFactory = new GeneratePdfHttpRequestFactory(mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
        }

        [Fact]
        public void Create_SetsExpectedHttpMethodOnDurableRequest()
        {
            var durableRequest = _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public void Create_SetsExpectedUriOnDurableRequest()
        {
            var durableRequest = _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Uri.AbsoluteUri.Should().Be($"{_pdfGeneratorUrl}/generate");
        }

        [Fact]
        public void Create_SetsExpectedHeadersOnDurableRequest()
        {
            var durableRequest = _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Headers.Should().Contain("Content-Type", "application/json");
            durableRequest.Headers.Should().Contain("cms-auth-values", _cmsAuthValues);
            durableRequest.Headers.Should().Contain("Correlation-Id", _correlationId.ToString());
        }

        [Fact]
        public void Create_SetsExpectedContentOnDurableRequest()
        {
            var durableRequest = _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Content.Should().Be(_content);
        }
    }
}

