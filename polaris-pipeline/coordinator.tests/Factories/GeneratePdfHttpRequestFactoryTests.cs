using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using Common.Adapters;
using Common.Constants;
using Common.Domain.Requests;
using Common.Wrappers;
using coordinator.Domain.Exceptions;
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
        private readonly AccessToken _clientAccessToken;
        private readonly string _cmsAuthValues;
        private readonly string _content;
        private readonly string _pdfGeneratorUrl;
        private readonly Guid _correlationId;

        private readonly Mock<IIdentityClientAdapter> _mockIdentityClientAdapter;

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
            _clientAccessToken = fixture.Create<AccessToken>();
            _cmsAuthValues = fixture.Create<string>();
            _content = fixture.Create<string>();
            var pdfGeneratorScope = fixture.Create<string>();
            _pdfGeneratorUrl = "https://www.test.co.uk/";
            _correlationId = fixture.Create<Guid>();

            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            var mockConfiguration = new Mock<IConfiguration>();
            _mockIdentityClientAdapter = new Mock<IIdentityClientAdapter>();

            _mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(_clientAccessToken.Token);

            mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.FileName == _fileName)))
                .Returns(_content);

            var mockLogger = new Mock<ILogger<GeneratePdfHttpRequestFactory>>();

            mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.PdfGeneratorScope]).Returns(pdfGeneratorScope);
            mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.PdfGeneratorUrl]).Returns(_pdfGeneratorUrl);
            mockConfiguration.Setup(config => config["OnBehalfOfTokenTenantId"]).Returns(fixture.Create<string>());

            _generatePdfHttpRequestFactory = new GeneratePdfHttpRequestFactory(_mockIdentityClientAdapter.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
        {
            var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public async Task Create_SetsExpectedUriOnDurableRequest()
        {
            var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Uri.AbsoluteUri.Should().Be(_pdfGeneratorUrl);
        }

        [Fact]
        public async Task Create_SetsExpectedHeadersOnDurableRequest()
        {
            var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Headers.Should().Contain("Content-Type", "application/json");
            durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_clientAccessToken.Token}");
            durableRequest.Headers.Should().Contain("upstream-token", _cmsAuthValues);
            durableRequest.Headers.Should().Contain("Correlation-Id", _correlationId.ToString());
        }

        [Fact]
        public async Task Create_SetsExpectedContentOnDurableRequest()
        {
            var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId);

            durableRequest.Content.Should().Be(_content);
        }

        [Fact]
        public async Task Create_ClientCredentialsFlow_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .Throws(new Exception());

            await Assert.ThrowsAsync<GeneratePdfHttpRequestFactoryException>(() => _generatePdfHttpRequestFactory.Create(_caseUrn, _caseId, _documentCategory, _documentId, _fileName, _versionId, _cmsAuthValues, _correlationId));
        }
    }
}

