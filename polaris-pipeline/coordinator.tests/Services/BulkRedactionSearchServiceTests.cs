using Common.Configuration;
using Common.Domain.Ocr;
using Common.Dto.Request;
using Common.Dto.Response.Document;
using Common.Services.BlobStorage;
using coordinator.Builders;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Enums;
using coordinator.OcrDocumentSearch;
using coordinator.Services;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace coordinator.tests.Services;

public class BulkRedactionSearchServiceTests
{
    private readonly Mock<IOrchestrationProvider> _orchestrationProviderMock;
    private readonly Mock<IPolarisBlobStorageService> _polarisBlobStorageServiceMock;
    private readonly Mock<IBulkRedactionSearchResponseBuilder> _bulkRedactionSearchResponseBuilderMock;
    private readonly Mock<IOcrDocumentSearch> _ocrDocumentSearchMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly BulkRedactionSearchService _bulkRedactionSearchService;

    public BulkRedactionSearchServiceTests()
    {
        _orchestrationProviderMock = new Mock<IOrchestrationProvider>();
        _polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        _bulkRedactionSearchResponseBuilderMock = new Mock<IBulkRedactionSearchResponseBuilder>();
        _ocrDocumentSearchMock = new Mock<IOcrDocumentSearch>();
        _mdsClientMock = new Mock<IMdsClient>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(s => s[StorageKeys.BlobServiceContainerNameDocuments]).Returns(string.Empty);
        var blobStorageServiceFactoryMock = new Mock<Func<string, IPolarisBlobStorageService>>();
        blobStorageServiceFactoryMock.Setup(s => s.Invoke(string.Empty)).Returns(_polarisBlobStorageServiceMock.Object);

        _bulkRedactionSearchService = new BulkRedactionSearchService(blobStorageServiceFactoryMock.Object, _orchestrationProviderMock.Object, _bulkRedactionSearchResponseBuilderMock.Object, _ocrDocumentSearchMock.Object, configurationMock.Object, _mdsClientMock.Object, _ddeiArgFactoryMock.Object);
    }

    [Theory]
    [InlineData("PCD")]
    [InlineData("DAC")]
    public async Task BulkRedactionSearchAsync_DocumentIsNotRedactable_ShouldReturnBulkRedactionSearchResponse(string documentPrefix)
    {
        //arrange
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var failureReason = "Document is not redactable";
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = $"{documentPrefix}-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };

        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, false)).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_DocumentNotFoundInListDocument_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>();
        var failureReason = "Document not found in list document";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, true)).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);


        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OrchestrationProviderStatusesInitiated_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.VersionId
            }
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        _orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken)).ReturnsAsync(OrchestrationProviderStatuses.Initiated);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshInitiated()).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OrchestrationProviderStatusesProcessing_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.VersionId
            }
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        _orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken)).ReturnsAsync(OrchestrationProviderStatuses.Processing);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshProcessing()).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OrchestrationProviderStatusesFailed_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "Orchestration failure";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.VersionId
            }
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        _orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken)).ReturnsAsync(OrchestrationProviderStatuses.Failed);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, false)).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OcrDocumentNotFound_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "OCR Document Not Found";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.VersionId
            }
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        _orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken)).ReturnsAsync(OrchestrationProviderStatuses.Completed);
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>()))
            .ReturnsAsync((AnalyzeResults)null);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, true)).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }
    
    [Fact]
    public async Task BulkRedactionSearchAsync_SearchFailure_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "SearchFailed";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.VersionId
            }
        };
        var results = new AnalyzeResults();
        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse()
        {
            FailureReason = failureReason
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        _orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken)).ReturnsAsync(OrchestrationProviderStatuses.Completed);
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>())).ReturnsAsync(results);
        _ocrDocumentSearchMock.Setup(s => s.Search(bulkRedactionSearchDto.SearchText, results)).Returns(ocrDocumentSearchResponse);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, false)).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_Completed_ShouldReturnBulkRedactionSearchResponse()
    {
        //arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            DocumentId = "CMS-12345",
            VersionId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid()
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "SearchFailed";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.VersionId
            }
        };
        var results = new AnalyzeResults();
        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse();
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.ListDocumentsAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        _polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        _orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken)).ReturnsAsync(OrchestrationProviderStatuses.Completed);
        _polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>())).ReturnsAsync(results);
        _ocrDocumentSearchMock.Setup(s => s.Search(bulkRedactionSearchDto.SearchText, results)).Returns(ocrDocumentSearchResponse);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshCompleted()).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildRedactionDefinitions(ocrDocumentSearchResponse.redactionDefinitionDtos)).Returns(_bulkRedactionSearchResponseBuilderMock.Object);
        _bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        //assert
        _polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }
}