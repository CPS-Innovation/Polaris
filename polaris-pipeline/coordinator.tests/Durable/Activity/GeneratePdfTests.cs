// <copyright file="GeneratePdfTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_generator.tests.Durable.Activity;

using Common.Clients.PdfGenerator;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GeneratePdfFromDocumentTests
{
    private readonly Mock<IPdfGeneratorClient> pdfGeneratorClientMock;
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<Func<string, IPolarisBlobStorageService>> blobStorageServiceFactoryMock;
    private readonly Mock<IConfiguration> configurationMock;
    private readonly Mock<IMdsClient> mdsClientMock;
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly Mock<IPolarisBlobStorageService> polarisBlobStorageServiceMock;
    private readonly GeneratePdfFromDocument generatePdfFromDocument;

    public GeneratePdfFromDocumentTests()
    {
        this.pdfGeneratorClientMock = new Mock<IPdfGeneratorClient>();
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.blobStorageServiceFactoryMock = new Mock<Func<string, IPolarisBlobStorageService>>();
        this.configurationMock = new Mock<IConfiguration>();
        this.mdsClientMock = new Mock<IMdsClient>();
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        this.configurationMock.Setup(s => s[StorageKeys.BlobServiceContainerNameDocuments]).Returns(string.Empty);
        this.blobStorageServiceFactoryMock.Setup(s => s.Invoke(It.IsAny<string>())).Returns(this.polarisBlobStorageServiceMock.Object);
        this.generatePdfFromDocument = new GeneratePdfFromDocument(this.pdfGeneratorClientMock.Object, this.mdsArgFactoryMock.Object, this.blobStorageServiceFactoryMock.Object, this.configurationMock.Object, this.mdsClientMock.Object, this.masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_BlobExists_ShouldReturnPdfConversionsResponse()
    {
        // arrange
        var payload = new DocumentPayload();
        this.polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(true);

        // act
        var result = await this.generatePdfFromDocument.Run(payload);

        // assert
        Assert.True(result.BlobAlreadyExists);
        Assert.Equal(PdfConversionStatus.DocumentConverted, result.PdfConversionStatus);
    }

    [Fact]
    public async Task Run_FileTypeIsNull_ShouldReturnPdfConversionsResponse()
    {
        // arrange
        var payload = new DocumentPayload();
        this.polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(false);

        // act
        var result = await this.generatePdfFromDocument.Run(payload);

        // assert
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
        // arrange
        var payload = new DocumentPayload()
        {
            DocumentNatureType = DocumentNature.Types.Document,
            Path = "path.txt",
        };
        var arg = new MdsMaterialIdAndDocumentIdArgDto();
        var fileResult = new FileResult();
        var convertToPdfResponse = new ConvertToPdfResponse()
        {
            Status = pdfConversionStatus,
        };
        this.polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(false);
        this.mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(payload.CmsAuthValues, payload.CorrelationId, payload.Urn, payload.CaseId, payload.MaterialId, payload.DocumentId)).Returns(arg);
        this.masterDataServiceClientMock
            .Setup(s => s.GetDocumentByDocumentIdAsync(
                arg,
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileResult);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(payload.CorrelationId, payload.Urn, payload.CaseId, payload.MaterialId, payload.DocumentId, fileResult.Stream, payload.FileType.Value)).ReturnsAsync(convertToPdfResponse);

        // act
        var result = await this.generatePdfFromDocument.Run(payload);

        // assert
        Assert.False(result.BlobAlreadyExists);
        Assert.Equal(pdfConversionStatus, result.PdfConversionStatus);
    }

    [Fact]
    public async Task Run_PdfGeneratorResponseIsDocumentConverted_ShouldReturnPdfConversionsResponse()
    {
        // arrange
        var payload = new DocumentPayload()
        {
            DocumentNatureType = DocumentNature.Types.Document,
            Path = "path.txt",
        };
        var arg = new MdsMaterialIdAndDocumentIdArgDto();
        var fileResult = new FileResult();
        var convertToPdfResponse = new ConvertToPdfResponse()
        {
            Status = PdfConversionStatus.DocumentConverted,
            PdfStream = new MemoryStream(),
        };
        this.polarisBlobStorageServiceMock.Setup(s => s.BlobExistsAsync(It.IsAny<BlobIdType>(), payload.IsOcredProcessedPreference)).ReturnsAsync(false);
        this.mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(payload.CmsAuthValues, payload.CorrelationId, payload.Urn, payload.CaseId, payload.MaterialId, payload.DocumentId)).Returns(arg);
        this.masterDataServiceClientMock
            .Setup(s => s.GetDocumentByDocumentIdAsync(
                arg,
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileResult);
        this.pdfGeneratorClientMock.Setup(s => s.ConvertToPdfAsync(payload.CorrelationId, payload.Urn, payload.CaseId, payload.MaterialId, payload.DocumentId, fileResult.Stream, payload.FileType.Value)).ReturnsAsync(convertToPdfResponse);

        // act
        var result = await this.generatePdfFromDocument.Run(payload);

        // assert
        Assert.False(result.BlobAlreadyExists);
        Assert.Equal(PdfConversionStatus.DocumentConverted, result.PdfConversionStatus);
        this.polarisBlobStorageServiceMock.Verify(s => s.UploadBlobAsync(convertToPdfResponse.PdfStream, It.IsAny<BlobIdType>(),payload.IsOcredProcessedPreference), Times.Once);
    }
}
