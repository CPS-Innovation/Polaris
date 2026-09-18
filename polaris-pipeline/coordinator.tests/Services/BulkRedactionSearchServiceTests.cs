// <copyright file="BulkRedactionSearchServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.tests.Services;

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
using coordinator.Search;
using coordinator.Services;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class BulkRedactionSearchServiceTests
{
    private readonly Mock<IOrchestrationProvider> orchestrationProviderMock;
    private readonly Mock<IPolarisBlobStorageService> polarisBlobStorageServiceMock;
    private readonly Mock<IBulkRedactionSearchResponseBuilder> bulkRedactionSearchResponseBuilderMock;
    private readonly Mock<IOcrDocumentSearch> ocrDocumentSearchMock;
    private readonly Mock<IMdsClient> mdsClientMock;
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly BulkRedactionSearchService bulkRedactionSearchService;
    private readonly Mock<ILogger<BulkRedactionSearchService>> loggerMock;

    public BulkRedactionSearchServiceTests()
    {
        this.orchestrationProviderMock = new Mock<IOrchestrationProvider>();
        this.polarisBlobStorageServiceMock = new Mock<IPolarisBlobStorageService>();
        this.bulkRedactionSearchResponseBuilderMock = new Mock<IBulkRedactionSearchResponseBuilder>();
        this.ocrDocumentSearchMock = new Mock<IOcrDocumentSearch>();
        this.mdsClientMock = new Mock<IMdsClient>();
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.loggerMock = new Mock<ILogger<BulkRedactionSearchService>>();

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(s => s[StorageKeys.BlobServiceContainerNameDocuments]).Returns(string.Empty);
        var blobStorageServiceFactoryMock = new Mock<Func<string, IPolarisBlobStorageService>>();
        blobStorageServiceFactoryMock.Setup(s => s.Invoke(string.Empty)).Returns(this.polarisBlobStorageServiceMock.Object);

        this.bulkRedactionSearchService = new BulkRedactionSearchService(blobStorageServiceFactoryMock.Object, this.orchestrationProviderMock.Object, this.bulkRedactionSearchResponseBuilderMock.Object, this.ocrDocumentSearchMock.Object, configurationMock.Object, this.mdsClientMock.Object, this.mdsArgFactoryMock.Object, this.loggerMock.Object);
    }

    [Theory]
    [InlineData("PCD")]
    [InlineData("DAC")]
    public async Task BulkRedactionSearchAsync_DocumentIsNotRedactable_ShouldReturnBulkRedactionSearchResponse(string documentPrefix)
    {
        // arrange
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var failureReason = "Document is not redactable";
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = $"{documentPrefix}-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };

        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, false)).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_DocumentNotFoundInListDocument_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>();
        var failureReason = "Document not found in list document";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, true)).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OrchestrationProviderStatusesInitiated_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        this.orchestrationProviderMock.Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(),cancellationToken))
            .ReturnsAsync((OrchestrationProviderStatus.Initiated,"instance-123"));
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshInitiated()).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        this.polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OrchestrationProviderStatusesProcessing_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        this.orchestrationProviderMock
            .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken))
            .ReturnsAsync((OrchestrationProviderStatus.Processing, "instance-123"));
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshProcessing()).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        this.polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OrchestrationProviderStatusesFailed_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "Orchestration failure";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        this.orchestrationProviderMock.Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken))
            .ReturnsAsync((OrchestrationProviderStatus.Failed, "instance-123"));
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, false)).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        this.polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_OcrDocumentNotFound_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "OCR Document Not Found";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.polarisBlobStorageServiceMock .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        this.orchestrationProviderMock.Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken))
            .ReturnsAsync((OrchestrationProviderStatus.Completed, "instance-123"));
        this.polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>()))
            .ReturnsAsync((AnalyzeResults)null);
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, true)).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshCompleted())
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        this.polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_SearchFailure_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "SearchFailed";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };
        var results = new AnalyzeResults();
        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse()
        {
            FailureReason = failureReason,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        this.orchestrationProviderMock.Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken))
            .ReturnsAsync((OrchestrationProviderStatus.Completed, "instance-123"));
        this.polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>())).ReturnsAsync(results);
        this.ocrDocumentSearchMock.Setup(s => s.Search(bulkRedactionSearchDto.SearchText, results)).Returns(ocrDocumentSearchResponse);
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshFailed(failureReason, false)).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildDocumentRefreshCompleted())
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        this.polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task BulkRedactionSearchAsync_Completed_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var failureReason = "SearchFailed";
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var listDocumentResponse = new List<CmsDocumentDto>()
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };
        var results = new AnalyzeResults();
        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse();
        this.mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(bulkRedactionSearchDto.CmsAuthValues, bulkRedactionSearchDto.CorrelationId, bulkRedactionSearchDto.Urn, bulkRedactionSearchDto.CaseId)).Returns(mdsCaseIdentifiersArgDto);
        this.mdsClientMock.Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(listDocumentResponse);
        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>())).ReturnsAsync((CaseDurableEntityDocumentsState)null);
        this.orchestrationProviderMock
        .Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<DocumentPayload>(), cancellationToken))
            .ReturnsAsync((OrchestrationProviderStatus.Completed, "instance-123"));
        this.polarisBlobStorageServiceMock.Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>())).ReturnsAsync(results);
        this.ocrDocumentSearchMock.Setup(s => s.Search(bulkRedactionSearchDto.SearchText, results)).Returns(ocrDocumentSearchResponse);
        this.bulkRedactionSearchResponseBuilderMock
            .Setup(v => v.BuildDocumentRefreshCompleted())
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object); 
        this.bulkRedactionSearchResponseBuilderMock.Setup(v => v.BuildRedactionDefinitions(ocrDocumentSearchResponse.RedactionDefinitionDtos)).Returns(this.bulkRedactionSearchResponseBuilderMock.Object);
        this.bulkRedactionSearchResponseBuilderMock.Setup(s => s.Build(bulkRedactionSearchDto)).Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClientMock.Object, cancellationToken);

        // assert
        this.polarisBlobStorageServiceMock.Verify(v => v.UploadObjectAsync(It.IsAny<CaseDurableEntityDocumentsState>(), It.IsAny<BlobIdType>()));
        Assert.Same(result, bulkRedactionSearchResponse);
    }


    [Theory]
    [InlineData("PCD")]
    [InlineData("DAC")]
    public async Task GetOcrSearchResults_DocumentIsNotRedactable_ShouldReturnBulkRedactionSearchResponse(string documentPrefix)
    {
        // arrange
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        const string failureReason = "Document is not redactable";

        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = $"{documentPrefix}-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(v => v.BuildDocumentRefreshFailed(failureReason, false))
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(s => s.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task GetOcrSearchResults_Processing_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = CreateValidDto();
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;

        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();

        var listDocumentResponse = new List<CmsDocumentDto>
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };

        this.mdsArgFactoryMock
            .Setup(x => x.CreateCaseIdentifiersArg(
                bulkRedactionSearchDto.CmsAuthValues,
                bulkRedactionSearchDto.CorrelationId,
                bulkRedactionSearchDto.Urn,
                bulkRedactionSearchDto.CaseId))
            .Returns(mdsCaseIdentifiersArgDto);

        this.mdsClientMock
            .Setup(x => x.ListDocumentsAsync(mdsCaseIdentifiersArgDto))
            .ReturnsAsync(listDocumentResponse);

        this.orchestrationProviderMock
            .Setup(x => x.GetOrchestrationProviderStatus(
                orchestrationClientMock.Object,
                It.IsAny<DocumentPayload>(),
                cancellationToken))
            .ReturnsAsync(OrchestrationProviderStatus.Processing);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(x => x.BuildDocumentRefreshProcessing())
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(x => x.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task GetOcrSearchResults_Failed_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = CreateValidDto();
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;

        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        const string failureReason = "Orchestration failure";

        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();

        var listDocumentResponse = new List<CmsDocumentDto>
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };

        this.mdsArgFactoryMock.Setup(x => x.CreateCaseIdentifiersArg(
            bulkRedactionSearchDto.CmsAuthValues,
            bulkRedactionSearchDto.CorrelationId,
            bulkRedactionSearchDto.Urn,
            bulkRedactionSearchDto.CaseId))
            .Returns(mdsCaseIdentifiersArgDto);

        this.mdsClientMock.Setup(x => x.ListDocumentsAsync(mdsCaseIdentifiersArgDto))
            .ReturnsAsync(listDocumentResponse);

        this.orchestrationProviderMock
            .Setup(x => x.GetOrchestrationProviderStatus(
                orchestrationClientMock.Object,
                It.IsAny<DocumentPayload>(),
                cancellationToken))
            .ReturnsAsync(OrchestrationProviderStatus.Failed);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(x => x.BuildDocumentRefreshFailed(failureReason, false))
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(x => x.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task GetOcrSearchResults_NotStarted_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = CreateValidDto();
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;

        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        const string failureReason = "Orchestration instance Id invalid";

        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();

        var listDocumentResponse = new List<CmsDocumentDto>
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };

        this.mdsArgFactoryMock.Setup(x => x.CreateCaseIdentifiersArg(
            bulkRedactionSearchDto.CmsAuthValues,
            bulkRedactionSearchDto.CorrelationId,
            bulkRedactionSearchDto.Urn,
            bulkRedactionSearchDto.CaseId))
            .Returns(mdsCaseIdentifiersArgDto);

        this.mdsClientMock.Setup(x => x.ListDocumentsAsync(mdsCaseIdentifiersArgDto))
            .ReturnsAsync(listDocumentResponse);

        this.orchestrationProviderMock
            .Setup(x => x.GetOrchestrationProviderStatus(
                orchestrationClientMock.Object,
                It.IsAny<DocumentPayload>(),
                cancellationToken))
            .ReturnsAsync(OrchestrationProviderStatus.NotStarted);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(x => x.BuildDocumentRefreshFailed(failureReason, true))
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(x => x.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task GetOcrSearchResults_OcrDocumentNotFound_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };

        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;

        const string failureReason = "OCR Document Not Found";

        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();

        var listDocumentResponse = new List<CmsDocumentDto>
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };

        this.mdsArgFactoryMock
            .Setup(s => s.CreateCaseIdentifiersArg(
                bulkRedactionSearchDto.CmsAuthValues,
                bulkRedactionSearchDto.CorrelationId,
                bulkRedactionSearchDto.Urn,
                bulkRedactionSearchDto.CaseId))
            .Returns(mdsCaseIdentifiersArgDto);

        this.mdsClientMock
            .Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto))
            .ReturnsAsync(listDocumentResponse);

        this.orchestrationProviderMock
            .Setup(s => s.GetOrchestrationProviderStatus(
                orchestrationClientMock.Object,
                It.IsAny<DocumentPayload>(),
                cancellationToken))
            .ReturnsAsync(OrchestrationProviderStatus.Completed);

        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>()))
            .ReturnsAsync((AnalyzeResults)null);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(v => v.BuildDocumentRefreshFailed(failureReason, true))
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(s => s.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task GetOcrSearchResults_SearchFailure_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };

        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;

        const string failureReason = "SearchFailed";

        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();

        var listDocumentResponse = new List<CmsDocumentDto>
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };

        var results = new AnalyzeResults();

        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse
        {
            FailureReason = failureReason,
        };

        this.mdsArgFactoryMock
            .Setup(s => s.CreateCaseIdentifiersArg(
                bulkRedactionSearchDto.CmsAuthValues,
                bulkRedactionSearchDto.CorrelationId,
                bulkRedactionSearchDto.Urn,
                bulkRedactionSearchDto.CaseId))
            .Returns(mdsCaseIdentifiersArgDto);

        this.mdsClientMock
            .Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto))
            .ReturnsAsync(listDocumentResponse);

        this.orchestrationProviderMock
            .Setup(s => s.GetOrchestrationProviderStatus(
                orchestrationClientMock.Object,
                It.IsAny<DocumentPayload>(),
                cancellationToken))
            .ReturnsAsync(OrchestrationProviderStatus.Completed);

        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(results);

        this.ocrDocumentSearchMock
            .Setup(s => s.Search(bulkRedactionSearchDto.SearchText, results))
            .Returns(ocrDocumentSearchResponse);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(v => v.BuildDocumentRefreshFailed(failureReason, false))
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(s => s.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    [Fact]
    public async Task GetOcrSearchResults_Completed_ShouldReturnBulkRedactionSearchResponse()
    {
        // arrange
        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };

        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var cancellationToken = CancellationToken.None;

        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse();
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();

        var listDocumentResponse = new List<CmsDocumentDto>
        {
            new ()
            {
                DocumentId = 12345,
                VersionId = bulkRedactionSearchDto.DocumentId,
            },
        };

        var results = new AnalyzeResults();

        var ocrDocumentSearchResponse = new OcrDocumentSearchResponse();

        this.mdsArgFactoryMock
            .Setup(s => s.CreateCaseIdentifiersArg(
                bulkRedactionSearchDto.CmsAuthValues,
                bulkRedactionSearchDto.CorrelationId,
                bulkRedactionSearchDto.Urn,
                bulkRedactionSearchDto.CaseId))
            .Returns(mdsCaseIdentifiersArgDto);

        this.mdsClientMock
            .Setup(s => s.ListDocumentsAsync(mdsCaseIdentifiersArgDto))
            .ReturnsAsync(listDocumentResponse);

        this.orchestrationProviderMock
            .Setup(s => s.GetOrchestrationProviderStatus(
                orchestrationClientMock.Object,
                It.IsAny<DocumentPayload>(),
                cancellationToken))
            .ReturnsAsync(OrchestrationProviderStatus.Completed);

        this.polarisBlobStorageServiceMock
            .Setup(s => s.TryGetObjectAsync<AnalyzeResults>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(results);

        this.ocrDocumentSearchMock
            .Setup(s => s.Search(bulkRedactionSearchDto.SearchText, results))
            .Returns(ocrDocumentSearchResponse);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(v => v.BuildDocumentRefreshCompleted())
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(v => v.BuildRedactionDefinitions(ocrDocumentSearchResponse.RedactionDefinitionDtos))
            .Returns(this.bulkRedactionSearchResponseBuilderMock.Object);

        this.bulkRedactionSearchResponseBuilderMock
            .Setup(s => s.Build(bulkRedactionSearchDto))
            .Returns(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearchService.GetOcrSearchResults(
            bulkRedactionSearchDto,
            orchestrationClientMock.Object,
            cancellationToken);

        // assert
        Assert.Same(result, bulkRedactionSearchResponse);
    }

    private static BulkRedactionSearchDto CreateValidDto()
    {
        return new BulkRedactionSearchDto
        {
            Urn = "urn",
            CaseId = 1,
            MaterialId = "CMS-12345",
            DocumentId = 2L,
            SearchText = "searchText",
            CmsAuthValues = "cmsAuthValues",
            CorrelationId = Guid.NewGuid(),
        };
    }
}
