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
using Ddei;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Factories
{
    public class RenameDocumentTests
    {
        private readonly Fixture _fixture = new();
        private readonly Guid _correlationId;
        private readonly string _cmsAuthValues;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly int _documentId;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly Mock<ILogger<RenameDocument>> _mockLogger;
        private readonly Mock<IDdeiClient> _mockDdeiClient;
        private readonly Mock<IDdeiArgFactory> _mockDdeiArgFactory;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IValidator<RenameDocumentDto>> _mockRequestValidator;
        private readonly RenameDocument _renameDocument;

        public RenameDocumentTests()
        {
            var cmsAuthValues = _fixture.Create<string>();
            _correlationId = _fixture.Create<Guid>();
            _cmsAuthValues = _fixture.Create<string>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _documentId = _fixture.Create<int>();
            _mockLogger = new Mock<ILogger<RenameDocument>>();
            _mockDdeiClient = new Mock<IDdeiClient>();
            _mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockRequestValidator = new Mock<IValidator<RenameDocumentDto>>();
            var renameDocument = new RenameDocumentDto { DocumentId = "123", DocumentName = "Document renamed" };

            _httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonSerializer.Serialize(renameDocument))
            };
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestMessage.Headers.Add("Cms-Auth-Values", _cmsAuthValues);

            _mockJsonConvertWrapper.Setup(x => x.DeserializeObject<RenameDocumentDto>(It.IsAny<string>())).Returns(renameDocument);

            _renameDocument = new RenameDocument(_mockLogger.Object, _mockDdeiClient.Object, _mockDdeiArgFactory.Object, _mockJsonConvertWrapper.Object, _mockRequestValidator.Object);
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RenameDocument(null, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndDdeiClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RenameDocument(_mockLogger.Object, null, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndDdeiArgFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RenameDocument(_mockLogger.Object, _mockDdeiClient.Object, null, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndJsonConvertWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RenameDocument(_mockLogger.Object, _mockDdeiClient.Object, _mockDdeiArgFactory.Object, null, null));
        }

        [Fact]
        public void Run_ShouldReturnAnExceptionWhenInitializingAndValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RenameDocument(_mockLogger.Object, _mockDdeiClient.Object, _mockDdeiArgFactory.Object, _mockJsonConvertWrapper.Object, null));
        }

        [Fact]
        public async Task RenameDocument_ReturnsBadRequestWhenCorrelationIdIsMissing()
        {
            _httpRequestMessage.Headers.Clear();

            var result = await _renameDocument.Run(_httpRequestMessage, _caseUrn, _caseId, _documentId);

            (result as StatusCodeResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RenameDocument_ReturnsBadRequestWhenThereAreAnyValidationErrors()
        {
            var validationResults = new ValidationResult
            {
                Errors = _fixture.CreateMany<ValidationFailure>(2).ToList()
            };

            _mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RenameDocumentDto>(), CancellationToken.None))
                .ReturnsAsync(validationResults);

            var response = await _renameDocument.Run(_httpRequestMessage, _caseUrn, _caseId, _documentId);

            response.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task RenameDocument_ReturnsOkWhenNoteIsSuccessfullyAdded()
        {
            var validationResults = new ValidationResult();

            _mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<RenameDocumentDto>(), CancellationToken.None))
                .ReturnsAsync(validationResults);

            var response = await _renameDocument.Run(_httpRequestMessage, _caseUrn, _caseId, _documentId);

            response.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

    }
}