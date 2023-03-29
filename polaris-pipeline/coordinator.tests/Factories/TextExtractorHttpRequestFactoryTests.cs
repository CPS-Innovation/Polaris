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
    public class TextExtractorHttpRequestFactoryTests
	{
        private readonly Guid _polarisDocumentId;
        private readonly long _caseId;
		private readonly string _documentId;
		private readonly long _versionId;
		private readonly string _blobName;
		private readonly string _content;
        private readonly string _textExtractorUrl;
        private readonly Guid _correlationId;

        private readonly ITextExtractorHttpRequestFactory _textExtractorHttpRequestFactory;

		public TextExtractorHttpRequestFactoryTests()
		{
			var fixture = new Fixture();
			_polarisDocumentId = fixture.Create<Guid>();
			_caseId = fixture.Create<int>();
			_documentId = fixture.Create<string>();
			_versionId = fixture.Create<long>();
			_blobName = fixture.Create<string>();
			_content = fixture.Create<string>();
			_textExtractorUrl = "https://www.test.co.uk/";
			_correlationId = fixture.Create<Guid>();

            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
			
            mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<ExtractTextRequestDto>(r => r.CmsCaseId == _caseId && r.CmsDocumentId == _documentId && r.BlobName == _blobName)))
				.Returns(_content);

			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.TextExtractorUrl]).Returns(_textExtractorUrl);

			var mockLogger = new Mock<ILogger<TextExtractorHttpRequestFactory>>();

			_textExtractorHttpRequestFactory = new TextExtractorHttpRequestFactory(mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
		}

		[Fact]
		public void Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = _textExtractorHttpRequestFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public void Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = _textExtractorHttpRequestFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Uri.AbsoluteUri.Should().Be(_textExtractorUrl);
		}

		[Fact]
		public void Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = _textExtractorHttpRequestFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Correlation-Id", _correlationId.ToString());
		}

		[Fact]
		public void Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = _textExtractorHttpRequestFactory.Create(_polarisDocumentId, _caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Content.Should().Be(_content);
		}
	}
}

