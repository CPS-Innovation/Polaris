using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Configuration;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace coordinator.Durable.Entity;

public class CaseDurableEntityTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IPolarisBlobStorageService> _mockPolarisBlobStorageService;

    public CaseDurableEntityTests()
    {
        _fixture = new Fixture();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c[StorageKeys.BlobServiceContainerNameDocuments])
            .Returns("DocumentsContainer");
        _mockPolarisBlobStorageService = new Mock<IPolarisBlobStorageService>();
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_ReturnsNoChangesIfNothingHasChanged()
    {
        // Arrange
        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

        // Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse([], System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        // Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_ReturnsANewDocumentIfANewDocumentIsPresent()
    {
        // Arrange
        var existingDocId = _fixture.Create<long>();
        var newDocId = _fixture.Create<long>();

        var existingDocInEntity = _fixture.Create<CmsDocumentEntity>();
        existingDocInEntity.CmsDocumentId = existingDocId;

        var existingDocInIncoming = _fixture.Create<CmsDocumentDto>();
        existingDocInIncoming.DocumentId = existingDocId;

        var newDocInIncoming = _fixture.Create<CmsDocumentDto>();
        newDocInIncoming.DocumentId = newDocId;

        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
        _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(new CaseDurableEntityDocumentsState { CmsDocuments = [existingDocInEntity] });

        var incomingDocs = new CmsDocumentDto[]
        {
            existingDocInIncoming,
            newDocInIncoming
        };

        //Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse(incomingDocs, System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().HaveCount(1);
        result.CreatedCmsDocuments.First().Document.CmsDocumentId.Should().Be(newDocId);
        result.CreatedCmsDocuments.First().DeltaType.Should().Be(DocumentDeltaType.RequiresIndexing);

        // for the time being this method also mutates the entity: subject to being refactored
        var documents = await sut.GetDurableEntityDocumentsStateAsync();
        documents.CmsDocuments.Count.Should().Be(2);
        documents.CmsDocuments.Should().Contain(x => x.CmsDocumentId == newDocId);
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_WhenPresentationTitleChanges_EntityIsUpdatedAndNoDeltaReturned()
    {
        // Arrange
        var newDocTitle = _fixture.Create<string>();

        var docInEntity = _fixture.Create<CmsDocumentEntity>();

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        // make sure triggers for different delta types are not found
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.VersionId;
        docInIncoming.IsOcrProcessed = docInEntity.IsOcrProcessed;
        // our operative change
        docInIncoming.PresentationTitle = newDocTitle;

        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

        _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(new CaseDurableEntityDocumentsState { CmsDocuments = [docInEntity] });

        var incomingDocs = new CmsDocumentDto[]
        {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse(incomingDocs, System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
        result.UpdatedCmsDocuments.Should().BeEmpty();

        (await sut.GetDurableEntityDocumentsStateAsync()).CmsDocuments.First().PresentationTitle.Should().Be(newDocTitle);
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_WhenDocumentCategoryChanges_EntityIsUpdatedAndNoDeltaReturned()
    {
        // Arrange
        var newDocCategory = _fixture.Create<string>();

        var docInEntity = _fixture.Create<CmsDocumentEntity>();

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        // make sure triggers for different delta types are not found
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.VersionId;
        docInIncoming.IsOcrProcessed = docInEntity.IsOcrProcessed;
        // our operative change
        docInIncoming.CmsDocType.DocumentCategory = newDocCategory;

        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

        _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(new CaseDurableEntityDocumentsState { CmsDocuments = [docInEntity] });

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse(incomingDocs, System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
        result.UpdatedCmsDocuments.Should().BeEmpty();

        (await sut.GetDurableEntityDocumentsStateAsync()).CmsDocuments.First().CmsDocType.DocumentCategory.Should().Be(newDocCategory);
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_WhenCategoryListOrderChanges_EntityIsUpdatedAndNoDeltaReturned()
    {
        // Arrange
        var newCategoryListOrder = _fixture.Create<int>();
        var docInEntity = _fixture.Create<CmsDocumentEntity>();

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        // make sure triggers for different delta types are not found
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.VersionId;
        docInIncoming.IsOcrProcessed = docInEntity.IsOcrProcessed;
        // our operative change
        docInIncoming.CategoryListOrder = newCategoryListOrder;

        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

        _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(new CaseDurableEntityDocumentsState { CmsDocuments = [docInEntity] });

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse(incomingDocs, System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
        result.UpdatedCmsDocuments.Should().BeEmpty();

        (await sut.GetDurableEntityDocumentsStateAsync()).CmsDocuments.First().CategoryListOrder.Should().Be(newCategoryListOrder);
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_WhenIsOcrProcessedChanges_EntityIsUpdatedAndRequiresPdfRefreshIsReturned()
    {
        // Arrange

        var docInEntity = _fixture.Create<CmsDocumentEntity>();
        docInEntity.IsOcrProcessed = false;

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.VersionId;
        docInIncoming.IsOcrProcessed = true;
        // note: presentationTitle and categoryListOrder are going to be different between the two docs
        // due to AutoFixture but the Ocr flag change should be strong enough to return RequiresPdfRefresh

        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

        _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(new CaseDurableEntityDocumentsState { CmsDocuments = [docInEntity] });

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse(incomingDocs, System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        //Assert
        result.UpdatedCmsDocuments.Should().HaveCount(1);
        result.UpdatedCmsDocuments.First().Document.CmsDocumentId.Should().Be(docInEntity.CmsDocumentId);
        result.UpdatedCmsDocuments.First().DeltaType.Should().Be(DocumentDeltaType.RequiresPdfRefresh);

        (await sut.GetDurableEntityDocumentsStateAsync()).CmsDocuments.First().IsOcrProcessed.Should().BeTrue();
    }

    [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
    public async Task GetCaseDocumentChanges_WhenVersionIdChanges_EntityIsUpdatedAndRequiresIndexingIsReturned()
    {
        // Arrange

        var docInEntity = _fixture.Create<CmsDocumentEntity>();
        docInEntity.IsOcrProcessed = false;

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.IsOcrProcessed = true;
        // note: presentationTitle, categoryListOrder and IsOcrProcessed are going to be different between the two docs
        //  due to AutoFixture but the versionId change should be strong enough to return RequiresIndexing

        var sut = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

        _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.IsAny<BlobIdType>()))
            .ReturnsAsync(new CaseDurableEntityDocumentsState { CmsDocuments = [docInEntity] });

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges(new GetCaseDocumentsResponse(incomingDocs, System.Array.Empty<PcdRequestDto>(), new DefendantsAndChargesListDto()));

        //Assert
        result.UpdatedCmsDocuments.Should().HaveCount(1);
        result.UpdatedCmsDocuments.First().Document.CmsDocumentId.Should().Be(docInEntity.CmsDocumentId);
        result.UpdatedCmsDocuments.First().DeltaType.Should().Be(DocumentDeltaType.RequiresIndexing);

        (await sut.GetDurableEntityDocumentsStateAsync()).CmsDocuments.First().IsOcrProcessed.Should().BeTrue();
    }
}