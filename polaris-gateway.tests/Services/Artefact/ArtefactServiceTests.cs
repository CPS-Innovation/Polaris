using Common.Clients.PdfGenerator;
using Common.Services.OcrService;
using Common.Services.PiiService;
using Common.Services.RenderHtmlService;
using Ddei;
using Ddei.Factories;
using Moq;
using PolarisGateway.Services.Artefact.Factories;
using PolarisGateway.Services.Artefact;
using Xunit;
using System;
using AutoFixture;
using Ddei.Domain.CaseData.Args;
using Common.Dto.Response;
using System.IO;
using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;
using System.Threading.Tasks;

namespace PolarisGateway.Tests.Services.Artefact;

public class ArtefactServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IArtefactServiceResponseFactory> _artefactServiceResponseFactoryMock;
    private readonly Mock<IDdeiClient> _ddeiClientMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IPdfGeneratorClient> _pdfGeneratorClientMock;
    private readonly Mock<IConvertModelToHtmlService> _convertModelToHtmlServiceMock;
    private readonly Mock<IOcrService> _ocrServiceMock;
    private readonly Mock<IPiiService> _piiServiceMock;
    private readonly ArtefactService _artefactService;
    private readonly string _cmsAuthValues;
    private readonly Guid _correlationId;
    private readonly string _urn;
    private readonly int _caseId;
    private readonly long _versionId;
    private readonly bool _isOcrProcessed;
    private readonly Guid? _operationId;
    private readonly string _documentId;
    private readonly string _pcdId;
    private readonly string _dacId;
    private readonly Stream _documentStream;
    private readonly ArtefactResult<Stream> _failedStreamResult;
    private readonly DdeiDocumentIdAndVersionIdArgDto _ddeiDocumentArg;
    public ArtefactServiceTests()
    {
        _fixture = new Fixture();

        _cmsAuthValues = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();
        _urn = _fixture.Create<string>();
        _caseId = _fixture.Create<int>();

        _versionId = _fixture.Create<long>();
        _isOcrProcessed = _fixture.Create<bool>();
        _operationId = _fixture.Create<Guid>();

        _documentId = "CMS" + _fixture.Create<string>();
        _pcdId = "PCD" + _fixture.Create<string>();
        _dacId = "DAC" + _fixture.Create<string>();

        _documentStream = new MemoryStream();
        _failedStreamResult = new ArtefactResult<Stream>();

        _artefactServiceResponseFactoryMock = new Mock<IArtefactServiceResponseFactory>();
        _artefactServiceResponseFactoryMock
            .Setup(x => x.CreateFailedResult<Stream>(PdfConversionStatus.DocumentTypeUnsupported))
            .Returns(_failedStreamResult);

        _ddeiClientMock = new Mock<IDdeiClient>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _pdfGeneratorClientMock = new Mock<IPdfGeneratorClient>();
        _convertModelToHtmlServiceMock = new Mock<IConvertModelToHtmlService>();
        _ocrServiceMock = new Mock<IOcrService>();
        _piiServiceMock = new Mock<IPiiService>();

        _ddeiDocumentArg = _fixture.Create<DdeiDocumentIdAndVersionIdArgDto>();
        _ddeiArgFactoryMock
            .Setup(x => x.CreateDocumentVersionArgDto(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId))
            .Returns(_ddeiDocumentArg);

        _artefactService = new ArtefactService(
            _artefactServiceResponseFactoryMock.Object,
            _ddeiClientMock.Object,
            _ddeiArgFactoryMock.Object,
            _pdfGeneratorClientMock.Object,
            _convertModelToHtmlServiceMock.Object,
            _ocrServiceMock.Object,
            _piiServiceMock.Object);
    }

    [Fact]
    public async Task GetPdfAsync_WhenCalledWithAnUnsupportedFileType_ReturnsFailedResult()
    {
        var fileResult = new FileResult
        {
            Stream = Stream.Null,
            FileName = "file._unsupported_file_extension_"
        };

        _ddeiClientMock
            .Setup(x => x.GetDocumentAsync(_ddeiDocumentArg))
            .ReturnsAsync(fileResult);

        var result = await _artefactService.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, _isOcrProcessed);

        Assert.Equal(_failedStreamResult, result);
    }

    [Fact]
    public async Task GetPdfAsync_WhenReceivingAFailedConversionFromPdfGenerator_ReturnsFailedResult()
    {
        var fileResult = new FileResult
        {
            Stream = _documentStream,
            FileName = "file.docx"
        };

        _ddeiClientMock
            .Setup(x => x.GetDocumentAsync(_ddeiDocumentArg))
            .ReturnsAsync(fileResult);

        var result = await _artefactService.GetPdfAsync(_cmsAuthValues, _correlationId, _urn, _caseId, _documentId, _versionId, _isOcrProcessed);

        Assert.Equal(_failedStreamResult, result);
    }
}