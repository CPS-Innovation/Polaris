using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Mappers;
using Common.Services.BlobStorage.Factories;
using Common.Services.BlobStorage;
using coordinator.Durable.Activity;
using Microsoft.Extensions.Configuration;
using Moq;
using Common.Configuration;
using Common.Domain.Ocr;
using Xunit;
using coordinator.Durable.Payloads;
using Common.Exceptions;
using Common.Dto.Request.Redaction;

namespace coordinator.tests.Durable.Activity;

public class BulkRedactionSearchActivityTests
{
    private readonly Mock<IPolarisBlobStorageService> _polarisBlobStorageServiceMock;
    private readonly Mock<IRedactionSearchDtoMapper> _redactionSearchDtoMapperMock;
    private readonly Mock<IBlobTypeIdFactory> _blobTypeIdFactoryMock;
    private readonly BulkRedactionSearchActivity _bulkRedactionSearchActivity;

    public BulkRedactionSearchActivityTests()
    {
        var containerName = "containerName";
        _polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        _redactionSearchDtoMapperMock = new Mock<IRedactionSearchDtoMapper>();
        _blobTypeIdFactoryMock = new Mock<IBlobTypeIdFactory>();
        var blobStorageServiceFactoryMock = new Mock<Func<string, IPolarisBlobStorageService>>();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(s => s[StorageKeys.BlobServiceContainerNameDocuments]).Returns(containerName);
        blobStorageServiceFactoryMock.Setup(s => s(containerName)).Returns(_polarisBlobStorageServiceMock.Object);
        _bulkRedactionSearchActivity = new BulkRedactionSearchActivity(blobStorageServiceFactoryMock.Object, _redactionSearchDtoMapperMock.Object, _blobTypeIdFactoryMock.Object, configurationMock.Object);
    }

    [Fact]
    public async Task Run_AnalyzeResultsIsNull_ShouldThrowOcrDocumentNotFoundException()
    {
        //arrange
        var payload = new BulkRedactionSearchPayload()
        {
            SearchText = "Sample Document"
        };
        var blobIdType = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
        var results = (AnalyzeResults)null;
        _blobTypeIdFactoryMock.Setup(s => s.CreateBlobId(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr)).Returns(blobIdType);
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(blobIdType)).ReturnsAsync(results);

        //act & assert
        await Assert.ThrowsAsync<OcrDocumentNotFoundException>(() => _bulkRedactionSearchActivity.Run(payload));
    }

    [Fact]
    public async Task Run_WordOnDifferentPages_Should()
    {
        //arrange
        var payload = new BulkRedactionSearchPayload()
        {
            SearchText = "Sample Document"
        };
        var blobIdType = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
        var results = new AnalyzeResults();
        var redactionSearchDtos = new List<RedactionSearchDto>()
        {
            new()
            {
                Word = "Sample",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 1,
                    Y1 = 2,
                    X2 = 3,
                    Y2 = 4
                },
                PageIndex = 1,
                Height = 5,
                Width = 6
            },
            new()
            {
                Word = "Document",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 7,
                    Y1 = 8,
                    X2 = 9,
                    Y2 = 10
                },
                PageIndex = 2,
                Height = 11,
                Width = 12
            }
        };
        _blobTypeIdFactoryMock.Setup(s => s.CreateBlobId(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr)).Returns(blobIdType);
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(blobIdType)).ReturnsAsync(results);
        _redactionSearchDtoMapperMock.Setup(s => s.Map(results.ReadResults)).Returns(redactionSearchDtos);

        //act
        var result = (await _bulkRedactionSearchActivity.Run(payload)).ToList();

        //assert
        Assert.Equal(redactionSearchDtos[0].Height, result[0].Height);
        Assert.Equal(redactionSearchDtos[0].Width, result[0].Width);
        Assert.Equal(redactionSearchDtos[0].PageIndex, result[0].PageIndex);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.X1, result[0].RedactionCoordinates[0].X1);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.X2, result[0].RedactionCoordinates[0].X2);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.Y1, result[0].RedactionCoordinates[0].Y1);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.Y1, result[0].RedactionCoordinates[0].Y1);
        Assert.Equal(redactionSearchDtos[1].Height, result[1].Height);
        Assert.Equal(redactionSearchDtos[1].Width, result[1].Width);
        Assert.Equal(redactionSearchDtos[1].PageIndex, result[1].PageIndex);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.X1, result[1].RedactionCoordinates[0].X1);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.X2, result[1].RedactionCoordinates[0].X2);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.Y1, result[1].RedactionCoordinates[0].Y1);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.Y1, result[1].RedactionCoordinates[0].Y1);
    }
    
    [Fact]
    public async Task Run_WordOnSamePages_Should()
    {
        //arrange
        var payload = new BulkRedactionSearchPayload()
        {
            SearchText = "Sample Document"
        };
        var blobIdType = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
        var results = new AnalyzeResults();
        var redactionSearchDtos = new List<RedactionSearchDto>()
        {
            new()
            {
                Word = "Sample",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 1,
                    Y1 = 2,
                    X2 = 3,
                    Y2 = 4
                },
                PageIndex = 1,
                Height = 5,
                Width = 6
            },
            new()
            {
                Word = "Document",
                RedactionCoordinates = new RedactionCoordinatesDto
                {
                    X1 = 7,
                    Y1 = 8,
                    X2 = 9,
                    Y2 = 10
                },
                PageIndex = 1,
                Height = 11,
                Width = 12
            }
        };
        _blobTypeIdFactoryMock.Setup(s => s.CreateBlobId(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr)).Returns(blobIdType);
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(blobIdType)).ReturnsAsync(results);
        _redactionSearchDtoMapperMock.Setup(s => s.Map(results.ReadResults)).Returns(redactionSearchDtos);

        //act
        var result = (await _bulkRedactionSearchActivity.Run(payload)).ToList();

        //assert
        Assert.Equal(redactionSearchDtos[0].Height, result[0].Height);
        Assert.Equal(redactionSearchDtos[0].Width, result[0].Width);
        Assert.Equal(redactionSearchDtos[0].PageIndex, result[0].PageIndex);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.X1, result[0].RedactionCoordinates[0].X1);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.X2, result[0].RedactionCoordinates[0].X2);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.Y1, result[0].RedactionCoordinates[0].Y1);
        Assert.Equal(redactionSearchDtos[0].RedactionCoordinates.Y1, result[0].RedactionCoordinates[0].Y1);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.X1, result[0].RedactionCoordinates[1].X1);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.X2, result[0].RedactionCoordinates[1].X2);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.Y1, result[0].RedactionCoordinates[1].Y1);
        Assert.Equal(redactionSearchDtos[1].RedactionCoordinates.Y1, result[0].RedactionCoordinates[1].Y1);
    }
}