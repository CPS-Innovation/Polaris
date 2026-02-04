using Common.Clients.PdfGenerator;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Response;
using Common.Services.BlobStorage;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace pdf_generator.tests.Durable.Activity;

public class GeneratePdfFromDocumentTests
{
    private readonly Mock<IPdfGeneratorClient> _pdfGeneratorClientMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<Func<string, IPolarisBlobStorageService>> _blobStorageServiceFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly Mock<IPolarisBlobStorageService> _polarisBlobStorageServiceMock;
    private readonly GeneratePdfFromDocument _generatePdfFromDocument;

    public GeneratePdfFromDocumentTests()
    {
        _pdfGeneratorClientMock = new Mock<IPdfGeneratorClient>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _blobStorageServiceFactoryMock = new Mock<Func<string, IPolarisBlobStorageService>>();
        _configurationMock = new Mock<IConfiguration>();
        _mdsClientMock = new Mock<IMdsClient>();

        _polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        _configurationMock.Setup(s => s[StorageKeys.BlobServiceContainerNameDocuments]).Returns(string.Empty);
        _blobStorageServiceFactoryMock.Setup(s => s.Invoke(It.IsAny<string>())).Returns(_polarisBlobStorageServiceMock.Object);
        
        _generatePdfFromDocument = new GeneratePdfFromDocument(_pdfGeneratorClientMock.Object, _mdsArgFactoryMock.Object,_blobStorageServiceFactoryMock.Object,_configurationMock.Object,_mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_BlobExists_ShouldReturnPdfConversionsResponse()
    {
        //arrange
        var payload = new DocumentPayload();
        _polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(true);

        //act
        var result = await _generatePdfFromDocument.Run(payload);

        //assert
        Assert.True(result.BlobAlreadyExists);
        Assert.Equal(PdfConversionStatus.DocumentConverted, result.PdfConversionStatus);
    }
    
    [Fact]
    public async Task Run_FileTypeIsNull_ShouldReturnPdfConversionsResponse()
    {
        //arrange
        var payload = new DocumentPayload();
        _polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(false);

        //act
        var result = await _generatePdfFromDocument.Run(payload);

        //assert
        Assert.False(result.BlobAlreadyExists);
        Assert.Equal(PdfConversionStatus.DocumentTypeUnsupported, result.PdfConversionStatus);
    }
    
    [Theory]
    [InlineData(PdfConversionStatus.PdfEncrypted)]
    [InlineData(PdfConversionStatus.DocumentTypeUnsupported)]
    [InlineData(PdfConversionStatus.AsposePdfPasswordProtected)]
    [InlineData(PdfConversionStatus.AsposePdfInvalidFileFormat)]
    [InlineData(PdfConversionStatus.AsposePdfException)]
    [InlineData(PdfConversionStatus.AsposeWordsUnsupportedFileFormat)]
    [InlineData(PdfConversionStatus.AsposeWordsPasswordProtected)]
    [InlineData(PdfConversionStatus.AsposeCellsGeneralError)]
    [InlineData(PdfConversionStatus.AsposeImagingCannotLoad)]
    [InlineData(PdfConversionStatus.UnexpectedError)]
    [InlineData(PdfConversionStatus.AsposeSlidesPasswordProtected)]
    public async Task Run_PdfGeneratorResponseIsNotDocumentConverted_ShouldReturnPdfConversionsResponse(PdfConversionStatus pdfConversionStatus)
    {
        //arrange
        var payload = new DocumentPayload()
        {
            DocumentNatureType = DocumentNature.Types.Document,
            Path = "path.txt"
        };
        var arg = new MdsDocumentIdAndVersionIdArgDto();
        var fileResult = new FileResult();
        var convertToPdfResponse = new ConvertToPdfResponse()
        {
            Status = pdfConversionStatus
        };
        _polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(false);
        _mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(payload.CmsAuthValues, payload.CorrelationId, payload.Urn, payload.CaseId, payload.DocumentId, payload.VersionId)).Returns(arg);
        _mdsClientMock.Setup(s => s.GetDocumentAsync(arg)).ReturnsAsync(fileResult);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(payload.CorrelationId, payload.Urn, payload.CaseId, payload.DocumentId, payload.VersionId, fileResult.Stream, payload.FileType.Value)).ReturnsAsync(convertToPdfResponse);

        //act
        var result = await _generatePdfFromDocument.Run(payload);

        //assert
        Assert.False(result.BlobAlreadyExists);
        Assert.Equal(pdfConversionStatus, result.PdfConversionStatus);
    }

    [Fact]
    public async Task Run_PdfGeneratorResponseIsDocumentConverted_ShouldReturnPdfConversionsResponse()
    {
        //arrange
        var payload = new DocumentPayload()
        {
            DocumentNatureType = DocumentNature.Types.Document,
            Path = "path.txt"
        };
        var arg = new MdsDocumentIdAndVersionIdArgDto();
        var fileResult = new FileResult();
        var convertToPdfResponse = new ConvertToPdfResponse()
        {
            Status = PdfConversionStatus.DocumentConverted,
            PdfStream = new MemoryStream()
        };
        _polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(false);
        _mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(payload.CmsAuthValues, payload.CorrelationId, payload.Urn, payload.CaseId, payload.DocumentId, payload.VersionId)).Returns(arg);
        _mdsClientMock.Setup(s => s.GetDocumentAsync(arg)).ReturnsAsync(fileResult);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(payload.CorrelationId, payload.Urn, payload.CaseId, payload.DocumentId, payload.VersionId, fileResult.Stream, payload.FileType.Value)).ReturnsAsync(convertToPdfResponse);

        //act
        var result = await _generatePdfFromDocument.Run(payload);

        //assert
        Assert.False(result.BlobAlreadyExists);
        Assert.Equal(PdfConversionStatus.DocumentConverted, result.PdfConversionStatus);
        _polarisBlobStorageServiceMock.Verify(s => s.UploadBlobAsync(convertToPdfResponse.PdfStream, It.IsAny<BlobIdType>(),payload.IsOcredProcessedPreference), Times.Once);
    }
}