﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Exceptions.Contracts;
using Common.Handlers;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluation.Contracts;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Domain;
using pdf_generator.Functions;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Functions
{
	public class GeneratePdfTests
	{
		private readonly Fixture _fixture = new();
        private readonly string _serializedGeneratePdfRequest;
		private readonly HttpRequestMessage _httpRequestMessage;
		private readonly GeneratePdfRequest _generatePdfRequest;
		private readonly string _blobName;
		private readonly Stream _documentStream;
		private readonly Stream _pdfStream;
		private readonly string _serializedGeneratePdfResponse;
		private HttpResponseMessage _errorHttpResponseMessage;

		private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
		private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
		private readonly Mock<IDocumentEvaluationService> _mockDocumentEvaluationService;
		private readonly Mock<IDdeiDocumentExtractionService> _mockDocumentExtractionService;
		private readonly Mock<IBlobStorageService> _mockBlobStorageService;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<GeneratePdf>> _mockLogger;
        private readonly Mock<IValidatorWrapper<GeneratePdfRequest>> _mockValidatorWrapper;
        private readonly Guid _correlationId;

		private readonly GeneratePdf _generatePdf;

		public GeneratePdfTests()
		{
			_serializedGeneratePdfRequest = _fixture.Create<string>();
            var upstreamToken = _fixture.Create<string>();
			_httpRequestMessage = new HttpRequestMessage()
			{
				Content = new StringContent(_serializedGeneratePdfRequest)
			};
			_httpRequestMessage.Headers.Add("upstream-token", upstreamToken);
			_generatePdfRequest = _fixture.Build<GeneratePdfRequest>()
									.With(r => r.FileName, "Test.doc")
									.With(r => r.CaseId, 123456)
									.With(r => r.VersionId, 123456)
									.Create();
			_blobName = $"{_generatePdfRequest.CaseId}/pdfs/{Path.GetFileNameWithoutExtension(_generatePdfRequest.FileName)}.pdf";
			_fixture.Create<string>();
			_documentStream = new MemoryStream();
			_pdfStream = new MemoryStream();
			_serializedGeneratePdfResponse = _fixture.Create<string>();
			
			_mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockValidatorWrapper = new Mock<IValidatorWrapper<GeneratePdfRequest>>();
			_mockDocumentEvaluationService = new Mock<IDocumentEvaluationService>();
			_mockDocumentExtractionService = new Mock<IDdeiDocumentExtractionService>();
			_mockBlobStorageService = new Mock<IBlobStorageService>();
			var mockPdfOrchestratorService = new Mock<IPdfOrchestratorService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();
			_mockLogger = new Mock<ILogger<GeneratePdf>>();
			_correlationId = _fixture.Create<Guid>();

			_mockAuthorizationValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfRequest>(_serializedGeneratePdfRequest))
				.Returns(_generatePdfRequest);
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfResponse>(r => r.BlobName == _blobName)))
				.Returns(_serializedGeneratePdfResponse);
			_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_generatePdfRequest)).Returns(new List<ValidationResult>());
			_mockDocumentEvaluationService.Setup(service => service.EvaluateDocumentAsync(It.IsAny<EvaluateDocumentRequest>(), It.IsAny<Guid>()))
				.ReturnsAsync(new EvaluateDocumentResponse(_generatePdfRequest.CaseId, _generatePdfRequest.DocumentId, _generatePdfRequest.VersionId) { EvaluationResult = DocumentEvaluationResult.AcquireDocument } );
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.CaseUrn, _generatePdfRequest.CaseId.ToString(), _generatePdfRequest.DocumentCategory, _generatePdfRequest.DocumentId, It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(_documentStream);
			mockPdfOrchestratorService.Setup(service => service.ReadToPdfStream(_documentStream, FileType.DOC, _generatePdfRequest.DocumentId, _correlationId))
				.Returns(_pdfStream);

			_generatePdf = new GeneratePdf(
								_mockAuthorizationValidator.Object,
								_mockJsonConvertWrapper.Object,
								_mockValidatorWrapper.Object,
								_mockDocumentEvaluationService.Object,
								_mockDocumentExtractionService.Object,
								_mockBlobStorageService.Object,
								mockPdfOrchestratorService.Object,
								_mockExceptionHandler.Object, 
								_mockLogger.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
		{
			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<GeneratePdf>>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			
			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
		{
            _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsNull()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = null;
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Headers.Add("Correlation-Id", string.Empty);

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Headers.Add("Correlation-Id", Guid.Empty.ToString());

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
		{
			var validationResults = _fixture.CreateMany<ValidationResult>(2).ToList();
			
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_generatePdfRequest)).Returns(validationResults);
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_DoesNotProcessTheDocument_WhenDocumentEvaluation_Returns_AlreadyProcessed()
		{
			_generatePdfRequest.FileName = "Test.pdf";
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.CaseUrn, _generatePdfRequest.CaseId.ToString(), 
					_generatePdfRequest.DocumentCategory, _generatePdfRequest.DocumentId, It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(_documentStream);
			_mockDocumentEvaluationService.Setup(service => service.EvaluateDocumentAsync(It.IsAny<EvaluateDocumentRequest>(), It.IsAny<Guid>()))
				.ReturnsAsync(new EvaluateDocumentResponse(_generatePdfRequest.CaseId, _generatePdfRequest.DocumentId, _generatePdfRequest.VersionId) { EvaluationResult = DocumentEvaluationResult.DocumentUnchanged } );
			
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			await _generatePdf.Run(_httpRequestMessage);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_documentStream, _blobName, _generatePdfRequest.CaseId.ToString(), _generatePdfRequest.DocumentId, 
				_generatePdfRequest.VersionId.ToString(), _correlationId), Times.Never);
		}
		
		[Fact]
		public async Task Run_UploadsDocumentStreamWhenFileTypeIsPdf()
		{
			_generatePdfRequest.FileName = "Test.pdf";
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.CaseUrn, _generatePdfRequest.CaseId.ToString(), 
					_generatePdfRequest.DocumentCategory, _generatePdfRequest.DocumentId, It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(_documentStream);

			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			await _generatePdf.Run(_httpRequestMessage);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_documentStream, _blobName, _generatePdfRequest.CaseId.ToString(), _generatePdfRequest.DocumentId, 
				_generatePdfRequest.VersionId.ToString(), _correlationId));
		}

		[Fact]
		public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
		{
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			await _generatePdf.Run(_httpRequestMessage);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_pdfStream, _blobName, _generatePdfRequest.CaseId.ToString(), _generatePdfRequest.DocumentId, 
				_generatePdfRequest.VersionId.ToString(), _correlationId));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			var response = await _generatePdf.Run(_httpRequestMessage);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsExpectedContent()
		{
			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			var response = await _generatePdf.Run(_httpRequestMessage);

			var content = await response.Content.ReadAsStringAsync();
			content.Should().Be(_serializedGeneratePdfResponse);
		}

		[Fact]
		public async Task Run_ReturnsResponseWhenExceptionOccurs()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			var exception = new Exception();
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfRequest>(_serializedGeneratePdfRequest))
				.Throws(exception);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);

			_httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}