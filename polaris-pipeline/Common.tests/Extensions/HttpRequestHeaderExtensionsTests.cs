using Common.Constants;
using Common.Domain.Exceptions;
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
            _headers.Add(HttpHeaderKeys.BlobName, "BlobName");

            _headers.Invoking(x => x.GetCorrelation())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid correlationId. A valid GUID is required. (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingCorrelation_AndTheHeaderValueIsInvalid_AnExceptionIsThrown()
        {
            var headerValue = "HeaderValue";
            _headers.Add(HttpHeaderKeys.CorrelationId, headerValue);

            _headers.Invoking(x => x.GetCorrelation())
                .Should().Throw<BadRequestException>()
                .WithMessage($"Invalid correlationId. A valid GUID is required. (Parameter '{headerValue}')");
        }

        [Fact]
        public void WhenGettingCorrelation_AndTheHeaderValueIsValid_AGuidIsReturned()
        {
            var headerValue = Guid.NewGuid();
            _headers.Add(HttpHeaderKeys.CorrelationId, headerValue.ToString());

            var result = _headers.GetCorrelation();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenCheckingForCmsAuthValues_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            _headers.Add(HttpHeaderKeys.BlobName, "BlobName");

            _headers.Invoking(x => x.CheckForCmsAuthValues())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid Cms Auth token. A valid Cms Auth token must be received for this request. (Parameter 'headers')");
        }

        [Fact]
        public void WhenCheckingForCmsAuthValues_AndTheHeaderValueIsEmpty_AnExceptionIsThrown()
        {
            var headerValue = "";
            _headers.Add(HttpHeaderKeys.CmsAuthValues, headerValue);

            _headers.Invoking(x => x.CheckForCmsAuthValues())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.");
        }

        [Fact]
        public void WhenGettingFileType_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            _headers.Add(HttpHeaderKeys.BlobName, "BlobName");

            _headers.Invoking(x => x.GetFileType())
                .Should().Throw<BadRequestException>()
                .WithMessage("Missing Filetype Value (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingFileType_AndTheHeaderValueIsEmpty_AnExceptionIsThrown()
        {
            var headerValue = "";
            _headers.Add(HttpHeaderKeys.Filetype, headerValue);

            _headers.Invoking(x => x.GetFileType())
                .Should().Throw<BadRequestException>()
                .WithMessage("Null Filetype Value");
        }

        [Fact]
        public void WhenGettingFileType_AndTheHeaderValueIsInvalid_AnExceptionIsThrown()
        {
            var headerValue = 99;
            _headers.Add(HttpHeaderKeys.Filetype, headerValue.ToString());

            _headers.Invoking(x => x.GetFileType())
                .Should().Throw<BadRequestException>()
                .WithMessage($"Invalid Filetype Enum Value (Parameter '{headerValue}')");
        }

        [Fact]
        public void WhenGettingFileType_AndTheHeaderValueIsValid_AnEnumValueIsReturned()
        {
            var headerValue = Domain.Document.FileType.PNG;
            _headers.Add(HttpHeaderKeys.Filetype, headerValue.ToString());

            var result = _headers.GetFileType();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingCaseId_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            _headers.Add(HttpHeaderKeys.BlobName, "BlobName");

            _headers.Invoking(x => x.GetCaseId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Missing CaseIds (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingCaseId_AndTheHeaderValueIsEmpty_AnExceptionIsThrown()
        {
            var headerValue = "";
            _headers.Add(HttpHeaderKeys.CaseId, headerValue);

            _headers.Invoking(x => x.GetCaseId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid CaseId");
        }

        [Fact]
        public void WhenGettingCaseId_AndTheHeaderValueIsValid_AGuidIsReturned()
        {
            var headerValue = "ABC123";
            _headers.Add(HttpHeaderKeys.CaseId, headerValue.ToString());

            var result = _headers.GetCaseId();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingDocumentId_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            _headers.Add(HttpHeaderKeys.BlobName, "BlobName");

            _headers.Invoking(x => x.GetDocumentId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Missing DocumentIds (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingDocumentId_AndTheHeaderValueIsEmpty_AnExceptionIsThrown()
        {
            var headerValue = "";
            _headers.Add(HttpHeaderKeys.DocumentId, headerValue);

            _headers.Invoking(x => x.GetDocumentId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid DocumentId");
        }

        [Fact]
        public void WhenGettingDocumentId_AndTheHeaderValueIsValid_AnIntIsReturned()
        {
            var headerValue = "ABC123";
            _headers.Add(HttpHeaderKeys.DocumentId, headerValue.ToString());

            var result = _headers.GetDocumentId();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingVersionId_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            _headers.Add(HttpHeaderKeys.BlobName, "BlobName");

            _headers.Invoking(x => x.GetVersionId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Missing VersionIds (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingVersionId_AndTheHeaderValueIsEmpty_AnExceptionIsThrown()
        {
            var headerValue = "";
            _headers.Add(HttpHeaderKeys.VersionId, headerValue);

            _headers.Invoking(x => x.GetVersionId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid VersionId");
        }

        [Fact]
        public void WhenGettingVersionId_AndTheHeaderValueIsValid_AnIntIsReturned()
        {
            var headerValue = "ABC123";
            _headers.Add(HttpHeaderKeys.VersionId, headerValue.ToString());

            var result = _headers.GetVersionId();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingDocumentTypeId_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            var request = new HttpRequestMessage();

            request.Invoking(x => x.Headers.GetDocumentTypeId())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid documentTypeId. (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingDocumentTypeId_AndTheHeaderValueIsEmpty_AnEmptyStringIsReturned()
        {
            var headerValue = "";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.DocumentTypeId, headerValue);

            var result = request.Headers.GetDocumentTypeId();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void WhenGettingDocumentTypeId_AndTheHeaderValueIsValid_AnIntIsReturned()
        {
            var headerValue = "123";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.DocumentTypeId, headerValue.ToString());

            var result = request.Headers.GetDocumentTypeId();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingDocumentType_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            var request = new HttpRequestMessage();

            request.Invoking(x => x.Headers.GetDocumentType())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid documentType. (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingDocumentType_AndTheHeaderValueIsEmpty_AnEmptyStringIsReturned()
        {
            var headerValue = "";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.DocumentType, headerValue);

            var result = request.Headers.GetDocumentType();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void WhenGettingDocumentType_AndTheHeaderValueIsValid_AnIntIsReturned()
        {
            var headerValue = "123";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.DocumentType, headerValue.ToString());

            var result = request.Headers.GetDocumentType();

            result.Should().Be(headerValue);
        }

        [Fact]
        public void WhenGettingDocumentCategory_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
        {
            var request = new HttpRequestMessage();

            request.Invoking(x => x.Headers.GetDocumentCategory())
                .Should().Throw<BadRequestException>()
                .WithMessage("Invalid documentCategory. (Parameter 'headers')");
        }

        [Fact]
        public void WhenGettingDocumentCategory_AndTheHeaderValueIsEmpty_AnEmptyStringIsReturned()
        {
            var headerValue = "";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.DocumentCategory, headerValue);

            var result = request.Headers.GetDocumentCategory();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void WhenGettingDocumentCategory_AndTheHeaderValueIsValid_AnIntIsReturned()
        {
            var headerValue = "123";
            var request = new HttpRequestMessage();
            request.Headers.Add(HttpHeaderKeys.DocumentCategory, headerValue.ToString());

            var result = request.Headers.GetDocumentCategory();

            result.Should().Be(headerValue);
        }

    }
}