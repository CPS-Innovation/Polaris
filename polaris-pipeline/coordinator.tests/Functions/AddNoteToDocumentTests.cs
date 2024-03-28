using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Request;
using Common.Wrappers;
using coordinator.Functions;
using Ddei.Factories;
using DdeiClient.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class AddNoteToDocumentTests
    {
        private readonly Fixture _fixture = new();
        private readonly Guid _correlationId;
        private readonly string _cmsAuthValues;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly string _documentCategory;
        private readonly int _documentId;
        private readonly HttpRequestMessage _httpRequestMessage;
        private HttpResponseMessage _errorHttpResponseMessage;
        private readonly Mock<ILogger<AddNoteToDocument>> _mockLogger;
        private readonly Mock<IDdeiClient> _mockDdeiClient;
        private readonly Mock<IDdeiArgFactory> _mockDdeiArgFactory;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IValidator<AddDocumentNoteDto>> _mockRequestValidator;
        private readonly AddNoteToDocument _addNoteToDocument;

        public AddNoteToDocumentTests()
        {
            var cmsAuthValues = _fixture.Create<string>();
            _correlationId = _fixture.Create<Guid>();
            _cmsAuthValues = _fixture.Create<string>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _documentCategory = _fixture.Create<string>();
            _documentId = _fixture.Create<int>();
            _mockLogger = new Mock<ILogger<AddNoteToDocument>>();
            _mockDdeiClient = new Mock<IDdeiClient>();
            _mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockRequestValidator = new Mock<IValidator<AddDocumentNoteDto>>();
            var testNote = new AddDocumentNoteDto { DocumentId = 123, Text = "Test " };

            _httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(testNote))
            };
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestMessage.Headers.Add("Cms-Auth-Values", _cmsAuthValues);

            _mockJsonConvertWrapper.Setup(x => x.DeserializeObject<AddDocumentNoteDto>(It.IsAny<string>())).Returns(testNote);

            _addNoteToDocument = new AddNoteToDocument(_mockLogger.Object, _mockDdeiClient.Object, _mockDdeiArgFactory.Object, _mockJsonConvertWrapper.Object, _mockRequestValidator.Object);
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddNoteToDocument(null, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndDdeiClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddNoteToDocument(_mockLogger.Object, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndDdeiArgFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddNoteToDocument(_mockLogger.Object, _mockDdeiClient.Object, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndJsonConvertWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddNoteToDocument(_mockLogger.Object, _mockDdeiClient.Object, _mockDdeiArgFactory.Object, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddNoteToDocument(_mockLogger.Object, _mockDdeiClient.Object, _mockDdeiArgFactory.Object, _mockJsonConvertWrapper.Object, null));
        }

        [Fact]
        public async Task AddNote_ReturnsBadRequestWhenCorrelationIdIsMissing()
        {
            _httpRequestMessage.Headers.Clear();

            var result = await _addNoteToDocument.AddNote(_httpRequestMessage, _caseUrn, _caseId, _documentCategory, _documentId);

            (result as StatusCodeResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddNote_ReturnsBadRequestWhenThereAreAnyValidationErrors()
        {
            var validationResults = new ValidationResult
            {
                Errors = _fixture.CreateMany<ValidationFailure>(2).ToList()
            };

            _mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<AddDocumentNoteDto>(), CancellationToken.None))
                .ReturnsAsync(validationResults);

            var response = await _addNoteToDocument.AddNote(_httpRequestMessage, _caseUrn, _caseId, _documentCategory, _documentId);

            response.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task AddNote_ReturnsOkWhenNoteIsSuccessfullyAdded()
        {
            var validationResults = new ValidationResult();

            _mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<AddDocumentNoteDto>(), CancellationToken.None))
                .ReturnsAsync(validationResults);

            var response = await _addNoteToDocument.AddNote(_httpRequestMessage, _caseUrn, _caseId, _documentCategory, _documentId);

            response.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}