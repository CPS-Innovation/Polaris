using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Common.tests.Extensions
{
    public class HttpRequestHeaderExtensionsTests
    {
        private readonly IHeaderDictionary _headers;

        public HttpRequestHeaderExtensionsTests()
        {
            var httpContext = new DefaultHttpContext();
            _headers = httpContext.Request.Headers;
        }

        [Fact]
        public void WhenGettingCorrelationId_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            var request = new HttpRequestMessage();

            request.Invoking(x => x.Headers.GetCorrelationId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid correlationId. A valid GUID is required. (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingCorrelationId_AndTheHeaderValueIsInvalid_AnExceptionIsThrown()
        {
            var headerValue = "HeaderValue";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.CorrelationId, headerValue);

            request.Invoking(x => x.Headers.GetCorrelationId())
                .Should().Throw<BadRequestException>()
                .WithMessage($"Invalid correlationId. A valid GUID is required. (Parameter '{headerValue}')");
        }

        [Fact]
        public void WhenGettingCorrelationId_AndTheHeaderValueIsValid_AGuidIsReturned()
        {
            var headerValue = Guid.NewGuid();
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.CorrelationId, headerValue.ToString());

            var result = request.Headers.GetCorrelationId();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingCorrelation_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            _headers.Append("BlobName", "BlobName");

            _headers.Invoking(x => x.GetCorrelationId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid correlationId. A valid GUID is required. (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingCorrelation_AndTheHeaderValueIsInvalid_AnExceptionIsThrown()
        {
            var headerValue = "HeaderValue";
            _headers.Append(HttpHeaderKeys.CorrelationId, headerValue);

            _headers.Invoking(x => x.GetCorrelationId())
                .Should().Throw<BadRequestException>()
                .WithMessage($"Invalid correlationId. A valid GUID is required. (Parameter '{headerValue}')");
        }

        [Fact]
        public void WhenGettingCorrelation_AndTheHeaderValueIsValid_AGuidIsReturned()
        {
            var headerValue = Guid.NewGuid();
            _headers.Append(HttpHeaderKeys.CorrelationId, headerValue.ToString());

            var result = _headers.GetCorrelationId();

            result.Should().Be(headerValue);
        }
    }
}