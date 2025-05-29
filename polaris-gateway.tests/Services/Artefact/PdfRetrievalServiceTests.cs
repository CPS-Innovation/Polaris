using Common.Clients.PdfGenerator;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Services.RenderHtmlService;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Moq;
using PolarisGateway.Services.Artefact;
using System;
using System.IO;
using System.Threading.Tasks;
using Common.Dto.Response;
using Xunit;

namespace PolarisGateway.Tests.Services.Artefact;

public class PdfRetrievalServiceTests
{
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IConvertModelToHtmlService> _convertModelToHtmlServiceMock;
    private readonly Mock<IPdfGeneratorClient> _pdfGeneratorClientMock;
    private readonly Mock<IDdeiClientFactory> _ddeiClientFactoryMock;
    private readonly PdfRetrievalService _pdfRetrievalService;

    public PdfRetrievalServiceTests()
    {
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _convertModelToHtmlServiceMock = new Mock<IConvertModelToHtmlService>();
        _pdfGeneratorClientMock = new Mock<IPdfGeneratorClient>();
        _ddeiClientFactoryMock = new Mock<IDdeiClientFactory>();
        _pdfRetrievalService = new PdfRetrievalService(_ddeiArgFactoryMock.Object, _convertModelToHtmlServiceMock.Object, _pdfGeneratorClientMock.Object, _ddeiClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetPdfStreamAsync_DocumentTypeIsPreChargeDecisionRequestAndStatusIsDocumentConverted_ShouldReturnDocumentRetrievalResultWithStream()
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var documentId = "PCD-123456";
        long versionId = 1;
        var ddeiPcdArgDto = new DdeiPcdArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var pcdRequest = new PcdRequestDto();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = PdfConversionStatus.DocumentConverted
        };
        _ddeiArgFactoryMock.Setup(s => s.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId)).Returns(ddeiPcdArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(cmsAuthValues, DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetPcdRequestAsync(ddeiPcdArgDto)).ReturnsAsync(pcdRequest);
        _convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(pcdRequest)).ReturnsAsync(stream);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(pdfResult.PdfStream, result.PdfStream);
        Assert.Equal(pdfResult.Status, result.Status);
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
    public async Task GetPdfStreamAsync_DocumentTypeIsPreChargeDecisionRequestAndStatusIsNotDocumentConverted_ShouldReturnDocumentRetrievalResultWithoutStream(PdfConversionStatus status)
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var documentId = "PCD-123456";
        long versionId = 1;
        var ddeiPcdArgDto = new DdeiPcdArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var pcdRequest = new PcdRequestDto();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = status
        };
        _ddeiArgFactoryMock.Setup(s => s.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId)).Returns(ddeiPcdArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(cmsAuthValues, DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetPcdRequestAsync(ddeiPcdArgDto)).ReturnsAsync(pcdRequest);
        _convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(pcdRequest)).ReturnsAsync(stream);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        //assert
        Assert.Null(result.PdfStream);
        Assert.Equal(pdfResult.Status, result.Status);
    }

    [Fact]
    public async Task GetPdfStreamAsync_DocumentTypeIsDefendantsAndChargesIsDocumentConverted_ShouldReturnDocumentRetrievalResultWithStream()
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var documentId = "DAC-123456";
        long versionId = 1;
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var defendantsAndCharges = new DefendantsAndChargesListDto();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = PdfConversionStatus.DocumentConverted
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId)).Returns(ddeiCaseIdentifiersArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(cmsAuthValues, DdeiClients.Ddei)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetDefendantAndChargesAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(defendantsAndCharges);
        _convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(defendantsAndCharges)).ReturnsAsync(stream);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(pdfResult.PdfStream, result.PdfStream);
        Assert.Equal(pdfResult.Status, result.Status);
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
    public async Task GetPdfStreamAsync_DocumentTypeIsDefendantsAndChargesIsNotDocumentConverted_ShouldReturnDocumentRetrievalResultWithoutStream(PdfConversionStatus status)
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var documentId = "DAC-123456";
        long versionId = 1;
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var defendantsAndCharges = new DefendantsAndChargesListDto();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = status
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId)).Returns(ddeiCaseIdentifiersArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(cmsAuthValues, DdeiClients.Ddei)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetDefendantAndChargesAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(defendantsAndCharges);
        _convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(defendantsAndCharges)).ReturnsAsync(stream);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        //assert
        Assert.Null(result.PdfStream);
        Assert.Equal(pdfResult.Status, result.Status);
    }

    [Theory]
    [InlineData(FileType.PDF)]
    [InlineData(FileType.DOC)]
    [InlineData(FileType.DOCX)]
    [InlineData(FileType.DOCM)]
    [InlineData(FileType.TXT)]
    [InlineData(FileType.XLS)]
    [InlineData(FileType.XLSX)]
    [InlineData(FileType.PPT)]
    [InlineData(FileType.PPTX)]
    [InlineData(FileType.BMP)]
    [InlineData(FileType.GIF)]
    [InlineData(FileType.JPG)]
    [InlineData(FileType.JPEG)]
    [InlineData(FileType.TIF)]
    [InlineData(FileType.TIFF)]
    [InlineData(FileType.PNG)]
    [InlineData(FileType.VSD)]
    [InlineData(FileType.HTM)]
    [InlineData(FileType.HTML)]
    [InlineData(FileType.MSG)]
    [InlineData(FileType.HTE)]
    [InlineData(FileType.XLSM)]
    [InlineData(FileType.DOTM)]
    [InlineData(FileType.XPS)]
    [InlineData(FileType.CSV)]
    [InlineData(FileType.DOTX)]
    [InlineData(FileType.EMZ)]
    [InlineData(FileType.EML)]
    [InlineData(FileType.XLT)]
    [InlineData(FileType.MHT)]
    [InlineData(FileType.MHTML)]
    public async Task GetPdfStreamAsync_DocumentTypeIsNotDefendantsOrPreChargeDecisionRequestAndIsSupportedFileType_ShouldReturnDocumentRetrievalResultWithStream(FileType fileType)
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-123456";
        long versionId = 1;
        var ddeiDocumentIdAndVersionIdArgDto = new DdeiDocumentIdAndVersionIdArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var fileResult = new FileResult()
        {
            FileName = $"name.{fileType}",
            Stream = new MemoryStream()
        };
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = PdfConversionStatus.DocumentConverted
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId,documentId, versionId)).Returns(ddeiDocumentIdAndVersionIdArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(cmsAuthValues, DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetDocumentAsync(ddeiDocumentIdAndVersionIdArgDto)).ReturnsAsync(fileResult);
        _pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, fileResult.Stream, fileType)).ReturnsAsync(pdfResult);

        //act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(pdfResult.PdfStream, result.PdfStream);
        Assert.Equal(pdfResult.Status, result.Status);
    }
    [Fact]
    public async Task GetPdfStreamAsync_DocumentTypeIsNotDefendantsOrPreChargeDecisionRequestAndIsNotSupportedFileType_ShouldReturnDocumentRetrievalResultWithUnsupported()
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-123456";
        long versionId = 1;
        var ddeiDocumentIdAndVersionIdArgDto = new DdeiDocumentIdAndVersionIdArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var fileResult = new FileResult()
        {
            FileName = "name.nonFileType",
            Stream = new MemoryStream()
        };
        
        _ddeiArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId,documentId, versionId)).Returns(ddeiDocumentIdAndVersionIdArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(cmsAuthValues, DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetDocumentAsync(ddeiDocumentIdAndVersionIdArgDto)).ReturnsAsync(fileResult);
        

        //act
        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(PdfConversionStatus.DocumentTypeUnsupported, result.Status);
    }
}
