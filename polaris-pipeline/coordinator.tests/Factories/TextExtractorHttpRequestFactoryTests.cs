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
	public class TextExtractorHttpRequestFactoryTests
	{
        private readonly long _caseId;
		private readonly string _documentId;
		private readonly long _versionId;
		private readonly string _blobName;
		private readonly AccessToken _clientAccessToken;
		private readonly string _content;
        private readonly string _textExtractorUrl;
        private readonly Guid _correlationId;

        private readonly Mock<IIdentityClientAdapter> _mockIdentityClientAdapter;
        private readonly ITextExtractorHttpRequestFactory _textExtractorHttpRequestFactory;

		public TextExtractorHttpRequestFactoryTests()
		{
			var fixture = new Fixture();
			_caseId = fixture.Create<int>();
			_documentId = fixture.Create<string>();
			_versionId = fixture.Create<long>();
			_blobName = fixture.Create<string>();
			_clientAccessToken = fixture.Create<AccessToken>();
			_content = fixture.Create<string>();
			var textExtractorScope = fixture.Create<string>();
			_textExtractorUrl = "https://www.test.co.uk/";
			_correlationId = fixture.Create<Guid>();

            _mockIdentityClientAdapter = new Mock<IIdentityClientAdapter>();
            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
			
            _mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), _correlationId))
	            .ReturnsAsync(_clientAccessToken.Token);

            mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<ExtractTextRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.BlobName == _blobName)))
				.Returns(_content);

			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.TextExtractorScope]).Returns(textExtractorScope);
			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.TextExtractorUrl]).Returns(_textExtractorUrl);

			var mockLogger = new Mock<ILogger<TextExtractorHttpRequestFactory>>();

			_textExtractorHttpRequestFactory = new TextExtractorHttpRequestFactory(_mockIdentityClientAdapter.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Uri.AbsoluteUri.Should().Be(_textExtractorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_clientAccessToken.Token}");
			durableRequest.Headers.Should().Contain("Correlation-Id", _correlationId.ToString());
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _versionId, _blobName, _correlationId);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ClientCredentialsFlow_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
				.Throws(new Exception());

            await Assert.ThrowsAsync<TextExtractorHttpRequestFactoryException>(() => _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _versionId, _blobName, _correlationId));
		}
	}
}

