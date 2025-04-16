
using Common.Clients.PdfGenerator;
using Common.Services.RenderHtmlService;
using Ddei.Factories;
using Moq;
using PolarisGateway.Services.Artefact;
using Xunit;
using System;
using AutoFixture;
using Ddei.Domain.CaseData.Args;
using Common.Dto.Response;
using System.IO;
using Common.Constants;
using System.Threading.Tasks;
using Common.Domain.Document;
using Common.Clients.PdfGeneratorDomain.Domain;
using FluentAssertions;
using Ddei.Domain.CaseData.Args.Core;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;
using DdeiClient.Clients.Interfaces;

namespace PolarisGateway.Tests.Services.Artefact;

public class PdfRetrievalServiceTests
{
    private readonly Fixture _fixture;

    private readonly Mock<IDdeiClient> _ddeiClientMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IConvertModelToHtmlService> _convertModelToHtmlServiceMock;
    private readonly Mock<IPdfGeneratorClient> _pdfGeneratorClientMock;
    private readonly PdfRetrievalService _pdfRetrievalService;
    private readonly string _cmsAuthValues;
    private readonly Guid _correlationId;
    private readonly string _urn;
    private readonly int _caseId;
    private readonly long _versionId;
    private readonly string _documentDocumentId;
    private readonly string _pcdDocumentId;
    private readonly string _dacDocumentId;
    private readonly Stream _documentStream;
    private readonly Stream _pcdStream;
    private readonly Stream _pdfStream;
    private readonly Stream _dacStream;
    private readonly DdeiDocumentIdAndVersionIdArgDto _ddeiDocumentArg;
    private readonly DdeiPcdArgDto _ddeiPcdArg;
    private readonly DdeiCaseIdentifiersArgDto _ddeiCaseIdentifiersArg;
    private readonly PcdRequestDto _pcdRequestDto;
    private readonly DefendantsAndChargesListDto _defendantsAndChargesListDto;

    public PdfRetrievalServiceTests()
    {
        _fixture = new Fixture();

        _cmsAuthValues = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();
        _urn = _fixture.Create<string>();
        _caseId = _fixture.Create<int>();

        _versionId = _fixture.Create<long>();

        _documentDocumentId = "CMS" + _fixture.Create<string>();
        _pcdDocumentId = "PCD" + _fixture.Create<string>();
        _dacDocumentId = "DAC" + _fixture.Create<string>();

        _documentStream = new MemoryStream(_fixture.Create<byte[]>());
        _pdfStream = new MemoryStream(_fixture.Create<byte[]>());
        _pcdStream = new MemoryStream(_fixture.Create<byte[]>());
        _dacStream = new MemoryStream(_fixture.Create<byte[]>());

        _ddeiClientMock = new Mock<IDdeiClient>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _pdfGeneratorClientMock = new Mock<IPdfGeneratorClient>();
        _convertModelToHtmlServiceMock = new Mock<IConvertModelToHtmlService>();

        _ddeiDocumentArg = _fixture.Create<DdeiDocumentIdAndVersionIdArgDto>();
        _ddeiPcdArg = _fixture.Create<DdeiPcdArgDto>();
        _pcdRequestDto = _fixture.Create<PcdRequestDto>();
        _ddeiCaseIdentifiersArg = _fixture.Create<DdeiCaseIdentifiersArgDto>();
        _defendantsAndChargesListDto = _fixture.Create<DefendantsAndChargesListDto>();

        _ddeiArgFactoryMock
            .Setup(x => x.CreateDocumentVersionArgDto(_cmsAuthValues, _correlationId, _urn, _caseId, _documentDocumentId, _versionId))
            .Returns(_ddeiDocumentArg);

        _ddeiArgFactoryMock
            .Setup(x => x.CreatePcdArg(_cmsAuthValues, _correlationId, _urn, _caseId, _pcdDocumentId))
            .Returns(_ddeiPcdArg);

        _ddeiArgFactoryMock
            .Setup(x => x.CreateCaseIdentifiersArg(_cmsAuthValues, _correlationId, _urn, _caseId))
            .Returns(_ddeiCaseIdentifiersArg);

        _ddeiClientMock
            .Setup(x => x.GetPcdRequestAsync(_ddeiPcdArg))
            .ReturnsAsync(_pcdRequestDto);

        _ddeiClientMock
            .Setup(x => x.GetPcdRequestAsync(_ddeiPcdArg))
            .ReturnsAsync(_pcdRequestDto);

        _ddeiClientMock
            .Setup(x => x.GetDefendantAndChargesAsync(_ddeiCaseIdentifiersArg))
            .ReturnsAsync(_defendantsAndChargesListDto);

        _convertModelToHtmlServiceMock
            .Setup(x => x.ConvertAsync(_pcdRequestDto))
            .ReturnsAsync(_pcdStream);

        _convertModelToHtmlServiceMock
            .Setup(x => x.ConvertAsync(_defendantsAndChargesListDto))
            .ReturnsAsync(_dacStream);

        _pdfRetrievalService = new PdfRetrievalService(
            _ddeiClientMock.Object,
            _ddeiArgFactoryMock.Object,
            _convertModelToHtmlServiceMock.Object,
            _pdfGeneratorClientMock.Object);
    }

    [Fact]
    public async Task GetPdfAsync_WhenCalledWithAnUnsupportedFileType_ReturnsFailedResult()
    {
        // Arrange
        _ddeiClientMock
            .Setup(x => x.GetDocumentAsync(_ddeiDocumentArg))
            .ReturnsAsync(new FileResult
            {
                Stream = _documentStream,
                FileName = "file._unsupported_file_extension_"
            });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentDocumentId, _versionId);

        // Assert
        result.Status.Should().Be(PdfConversionStatus.DocumentTypeUnsupported);
        result.PdfStream.Should().BeNull();
    }

    [Fact]
    public async Task GetPdfAsync_WhenReceivingAFailedConversionFromPdfGeneratorWithADocument_ReturnsFailedResult()
    {
        // Arrange
        _ddeiClientMock
            .Setup(x => x.GetDocumentAsync(_ddeiDocumentArg))
            .ReturnsAsync(new FileResult
            {
                Stream = _documentStream,
                FileName = "file.docx"
            });

        _pdfGeneratorClientMock.Setup(x => x.ConvertToPdfAsync(_correlationId, _urn, _caseId, _documentDocumentId, _versionId, _documentStream, FileType.DOCX))
            .ReturnsAsync(new ConvertToPdfResponse
            {
                Status = PdfConversionStatus.PdfEncrypted
            });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentDocumentId, _versionId);

        // Assert   
        result.Status.Should().Be(PdfConversionStatus.PdfEncrypted);
        result.PdfStream.Should().BeNull();
    }

    [Fact]
    public async Task GetPdfAsync_WhenConversionIsSuccessful_ReturnsOkResult()
    {
        // Arrange
        _ddeiClientMock
            .Setup(x => x.GetDocumentAsync(_ddeiDocumentArg))
            .ReturnsAsync(new FileResult
            {
                Stream = _documentStream,
                FileName = "file.docx"
            });

        _pdfGeneratorClientMock.Setup(x => x.ConvertToPdfAsync(_correlationId, _urn, _caseId, _documentDocumentId, _versionId, _documentStream, FileType.DOCX))
            .ReturnsAsync(new ConvertToPdfResponse
            {
                PdfStream = _pdfStream,
                Status = PdfConversionStatus.DocumentConverted
            });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentDocumentId, _versionId);

        // Assert   
        result.Status.Should().Be(PdfConversionStatus.DocumentConverted);
        result.PdfStream.Should().BeSameAs(_pdfStream);
    }

    [Fact]
    public async Task GetPdfAsync_WhenReceivingAFailedConversionFromPdfGeneratorWithAPcdRequest_ReturnsFailedResult()
    {
        // Arrange  
        _pdfGeneratorClientMock.Setup(x => x.ConvertToPdfAsync(_correlationId, _urn, _caseId, _pcdDocumentId, _versionId, _pcdStream, FileType.HTML))
            .ReturnsAsync(new ConvertToPdfResponse
            {
                Status = PdfConversionStatus.UnexpectedError
            });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _pcdDocumentId, _versionId);

        // Assert
        result.Status.Should().Be(PdfConversionStatus.UnexpectedError);
        result.PdfStream.Should().BeNull();
    }

    [Fact]
    public async Task GetPdfAsync_WhenConversionIsSuccessfulWithAPcdRequest_ReturnsOkResult()
    {
        // Arrange
        _pdfGeneratorClientMock.Setup(x => x.ConvertToPdfAsync(_correlationId, _urn, _caseId, _pcdDocumentId, _versionId, _pcdStream, FileType.HTML))
             .ReturnsAsync(new ConvertToPdfResponse
             {
                 PdfStream = _pdfStream,
                 Status = PdfConversionStatus.DocumentConverted
             });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _pcdDocumentId, _versionId);

        // Assert   
        result.Status.Should().Be(PdfConversionStatus.DocumentConverted);
        result.PdfStream.Should().BeSameAs(_pdfStream);
    }

    [Fact]
    public async Task GetPdfAsync_WhenReceivingAFailedConversionFromPdfGeneratorWithADac_ReturnsFailedResult()
    {
        // Arrange
        _pdfGeneratorClientMock.Setup(x => x.ConvertToPdfAsync(_correlationId, _urn, _caseId, _dacDocumentId, _versionId, _dacStream, FileType.HTML))
            .ReturnsAsync(new ConvertToPdfResponse
            {
                Status = PdfConversionStatus.UnexpectedError
            });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _dacDocumentId, _versionId);

        // Assert
        result.Status.Should().Be(PdfConversionStatus.UnexpectedError);
        result.PdfStream.Should().BeNull();
    }

    [Fact]
    public async Task GetPdfAsync_WhenConversionIsSuccessfulWithADac_ReturnsOkResult()
    {
        // Arrange
        _pdfGeneratorClientMock.Setup(x => x.ConvertToPdfAsync(_correlationId, _urn, _caseId, _dacDocumentId, _versionId, _dacStream, FileType.HTML))
             .ReturnsAsync(new ConvertToPdfResponse
             {
                 PdfStream = _pdfStream,
                 Status = PdfConversionStatus.DocumentConverted
             });

        // Act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _dacDocumentId, _versionId);

        // Assert
        result.Status.Should().Be(PdfConversionStatus.DocumentConverted);
        result.PdfStream.Should().BeSameAs(_pdfStream);
    }
}
