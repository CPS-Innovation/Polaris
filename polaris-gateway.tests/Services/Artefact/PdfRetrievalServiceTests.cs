// <copyright file="PdfRetrievalServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Services.Artefact;

using Common.Clients.PdfGenerator;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.HouseKeeping;
using Common.Services.RenderHtmlService;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using PolarisGateway.Services.Artefact;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class PdfRetrievalServiceTests
{
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<IConvertModelToHtmlService> convertModelToHtmlServiceMock;
    private readonly Mock<IPdfGeneratorClient> pdfGeneratorClientMock;
    private readonly Mock<IMdsClient> mdsClientMock;
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly Mock<ICaseDetailsMapper> caseDetailsMapperMock;
    private readonly PdfRetrievalService pdfRetrievalService;

    public PdfRetrievalServiceTests()
    {
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.convertModelToHtmlServiceMock = new Mock<IConvertModelToHtmlService>();
        this.pdfGeneratorClientMock = new Mock<IPdfGeneratorClient>();
        this.mdsClientMock = new Mock<IMdsClient>();
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.caseDetailsMapperMock = new Mock<ICaseDetailsMapper>();
        this.pdfRetrievalService = new PdfRetrievalService(this.mdsArgFactoryMock.Object, this.convertModelToHtmlServiceMock.Object, this.pdfGeneratorClientMock.Object, this.mdsClientMock.Object, this.masterDataServiceClientMock.Object, this.caseDetailsMapperMock.Object);
    }

    [Fact]
    public async Task GetPdfStreamAsync_DocumentTypeIsPreChargeDecisionRequestAndStatusIsDocumentConverted_ShouldReturnDocumentRetrievalResultWithStream()
    {
        //arrange
        var cmsAuthValues = "cmsAuthValues";
        var correlationId = Guid.NewGuid();
        var urn = "urn";
        var caseId = 1;
        var materialId = "PCD-123456";
        long documentId = 1;
        var mdsPcdArgDto = new MdsPcdArgDto();
        var pcdRequest = new Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = PdfConversionStatus.DocumentConverted,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, materialId)).Returns(mdsPcdArgDto);
        this.masterDataServiceClientMock.Setup(x => x.GetPcdRequestByPcdIdAsync(
            It.IsAny<GetPcdRequestByPcdIdCoreRequest>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(pcdRequest);
        this.convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(It.IsAny<Common.Dto.Response.Case.PreCharge.PcdRequestDto>())).ReturnsAsync(stream);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

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
        var materialId = "PCD-123456";
        long documentId = 1;
        var mdsPcdArgDto = new MdsPcdArgDto();
        var pcdRequest = new Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = status,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, materialId)).Returns(mdsPcdArgDto);
        this.masterDataServiceClientMock.Setup(s => s.GetPcdRequestByPcdIdAsync(
            It.IsAny<GetPcdRequestByPcdIdCoreRequest>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(pcdRequest);
        this.convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(It.IsAny<Common.Dto.Response.Case.PreCharge.PcdRequestDto>())).ReturnsAsync(stream);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

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
        var materialId = "DAC-123456";
        long documentId = 1;
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var defendantsResponse = new DefendantsResponse();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = PdfConversionStatus.DocumentConverted,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId)).Returns(mdsCaseIdentifiersArgDto);
        this.masterDataServiceClientMock
            .Setup(x => x.GetCaseDefendantsAsync(
                It.IsAny<ListCaseDefendantsRequest>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(defendantsResponse);
        this.convertModelToHtmlServiceMock.Setup(s => s.ConvertAsync(It.IsAny<DefendantsAndChargesListDto>())).ReturnsAsync(stream);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

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
        var materialId = "DAC-123456";
        long documentId = 1;
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var defendantsResponse = new DefendantsResponse();
        var stream = new MemoryStream();
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = status,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId)).Returns(mdsCaseIdentifiersArgDto);
        this.masterDataServiceClientMock
            .Setup(x => x.GetCaseDefendantsAsync(
                It.IsAny<ListCaseDefendantsRequest>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(defendantsResponse);
        this.convertModelToHtmlServiceMock.Setup(x => x.ConvertAsync(It.IsAny<DefendantsAndChargesListDto>())).ReturnsAsync(stream);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, stream, FileTypeHelper.PseudoDocumentFileType)).ReturnsAsync(pdfResult);

        //act
        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

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
        var materialId = "CMS-123456";
        long documentId = 1;
        var mdsDocumentIdAndVersionIdArgDto = new MdsMaterialIdAndDocumentIdArgDto();
        var fileResult = new FileResult()
        {
            FileName = $"name.{fileType}",
            Stream = new MemoryStream(),
        };
        var pdfResult = new ConvertToPdfResponse()
        {
            PdfStream = new MemoryStream(),
            Status = PdfConversionStatus.DocumentConverted,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, materialId, documentId)).Returns(mdsDocumentIdAndVersionIdArgDto);
        this.mdsClientMock.Setup(s => s.GetDocumentAsync(mdsDocumentIdAndVersionIdArgDto)).ReturnsAsync(fileResult);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, fileResult.Stream, fileType)).ReturnsAsync(pdfResult);

        //act
        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

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
        var materialId = "CMS-123456";
        long documentId = 1;
        var mdsDocumentIdAndVersionIdArgDto = new MdsMaterialIdAndDocumentIdArgDto();
        var fileResult = new FileResult()
        {
            FileName = "name.nonFileType",
            Stream = new MemoryStream(),
        };

        this.mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, materialId, documentId)).Returns(mdsDocumentIdAndVersionIdArgDto);
        this.mdsClientMock.Setup(s => s.GetDocumentAsync(mdsDocumentIdAndVersionIdArgDto)).ReturnsAsync(fileResult);

        //act
        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

        //assert
        Assert.Equal(PdfConversionStatus.DocumentTypeUnsupported, result.Status);
    }
}
