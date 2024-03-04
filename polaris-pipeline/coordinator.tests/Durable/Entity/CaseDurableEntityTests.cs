using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using coordinator.Durable.Payloads.Domain;
using FluentAssertions;
using Xunit;

namespace coordinator.Durable.Entity;

public class CaseDurableEntityTests
{
    private readonly Fixture _fixture;

    private readonly CaseDurableEntity _caseDurableEntity;

    public CaseDurableEntityTests()
    {
        _fixture = new Fixture();

        _caseDurableEntity = new CaseDurableEntity();
    }

    [Fact]
    public async Task GetCaseDocumentChanges_ReturnsNoChangesIfNothingHasChanged()
    {
        // Arrange
        var sut = new CaseDurableEntity();

        // Act
        var result = await sut.GetCaseDocumentChanges((new CmsDocumentDto[] { }, new PcdRequestDto[] { }, new DefendantsAndChargesListDto()));

        // Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCaseDocumentChanges_ReturnsANewDocumentIfANewDocumentIsPresent()
    {
        // Arrange
        var existingDocId = _fixture.Create<string>();
        var newDocId = _fixture.Create<string>();

        var existingDocInEntity = _fixture.Create<CmsDocumentEntity>();
        existingDocInEntity.CmsDocumentId = existingDocId;
        var existingDocInIncoming = _fixture.Create<CmsDocumentDto>();
        existingDocInIncoming.DocumentId = existingDocId;

        var newDocInIncoming = _fixture.Create<CmsDocumentDto>();
        newDocInIncoming.DocumentId = newDocId;

        var sut = new CaseDurableEntity
        {
            CmsDocuments = new List<CmsDocumentEntity> {
                existingDocInEntity,
            }
        };

        var incomingDocs = new CmsDocumentDto[] {
            existingDocInIncoming,
            newDocInIncoming
        };

        //Act
        var result = await sut.GetCaseDocumentChanges((incomingDocs, new PcdRequestDto[] { }, new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().HaveCount(1);
        result.CreatedCmsDocuments.First().Item1.CmsDocumentId.Should().Be(newDocId);
        result.CreatedCmsDocuments.First().Item2.Should().Be(DocumentDeltaType.RequiresIndexing);

        // for the time being this method also mutates the entity: subject to being refactored
        sut.CmsDocuments.Count.Should().Be(2);
        sut.CmsDocuments.Should().Contain(x => x.CmsDocumentId == newDocId);
    }

    [Fact]
    public async Task GetCaseDocumentChanges_WhenPresentationTitleChanges_EntityIsUpdatedAndNoDeltaReturned()
    {
        // Arrange
        var newDocTitle = _fixture.Create<string>();

        var docInEntity = _fixture.Create<CmsDocumentEntity>();

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        // make sure triggers for different delta types are not found
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.CmsVersionId;
        docInIncoming.IsOcrProcessed = docInEntity.IsOcrProcessed;
        // our operative change
        docInIncoming.PresentationTitle = newDocTitle;

        var sut = new CaseDurableEntity
        {
            CmsDocuments = new List<CmsDocumentEntity> {
                docInEntity,
            }
        };

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges((incomingDocs, new PcdRequestDto[] { }, new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
        result.UpdatedCmsDocuments.Should().BeEmpty();

        sut.CmsDocuments.First().PresentationTitle.Should().Be(newDocTitle);
    }

    [Fact]
    public async Task GetCaseDocumentChanges_WhenCategoryListOrderChanges_EntityIsUpdatedAndNoDeltaReturned()
    {
        // Arrange
        var newCategoryListOrder = _fixture.Create<int>();
        var docInEntity = _fixture.Create<CmsDocumentEntity>();

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        // make sure triggers for different delta types are not found
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.CmsVersionId;
        docInIncoming.IsOcrProcessed = docInEntity.IsOcrProcessed;
        // our operative change
        docInIncoming.CategoryListOrder = newCategoryListOrder;

        var sut = new CaseDurableEntity
        {
            CmsDocuments = new List<CmsDocumentEntity> {
                docInEntity,
            }
        };

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges((incomingDocs, new PcdRequestDto[] { }, new DefendantsAndChargesListDto()));

        //Assert
        result.CreatedCmsDocuments.Should().BeEmpty();
        result.UpdatedCmsDocuments.Should().BeEmpty();

        sut.CmsDocuments.First().CategoryListOrder.Should().Be(newCategoryListOrder);
    }

    [Fact]
    public async Task GetCaseDocumentChanges_WhenIsOcrProcessedChanges_EntityIsUpdatedAndRequiresPdfRefreshIsReturned()
    {
        // Arrange

        var docInEntity = _fixture.Create<CmsDocumentEntity>();
        docInEntity.IsOcrProcessed = false;

        var docInIncoming = _fixture.Create<CmsDocumentDto>();
        docInIncoming.DocumentId = docInEntity.CmsDocumentId;
        docInIncoming.VersionId = docInEntity.CmsVersionId;
        docInIncoming.IsOcrProcessed = true;
        // note: presentationTitle and categoryListOrder are going to be different between the two docs
        // due to AutoFixture but the Ocr flag change should be strong enough to return RequiresPdfRefresh

        var sut = new CaseDurableEntity
        {
            CmsDocuments = new List<CmsDocumentEntity> {
                docInEntity,
            }
        };

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges((incomingDocs, new PcdRequestDto[] { }, new DefendantsAndChargesListDto()));

        //Assert
        result.UpdatedCmsDocuments.Should().HaveCount(1);
        result.UpdatedCmsDocuments.First().Item1.CmsDocumentId.Should().Be(docInEntity.CmsDocumentId);
        result.UpdatedCmsDocuments.First().Item2.Should().Be(DocumentDeltaType.RequiresPdfRefresh);

        sut.CmsDocuments.First().IsOcrProcessed.Should().BeTrue();
    }

    [Fact]
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

        var sut = new CaseDurableEntity
        {
            CmsDocuments = new List<CmsDocumentEntity> {
                docInEntity,
            }
        };

        var incomingDocs = new CmsDocumentDto[] {
            docInIncoming,
        };

        //Act
        var result = await sut.GetCaseDocumentChanges((incomingDocs, new PcdRequestDto[] { }, new DefendantsAndChargesListDto()));

        //Assert
        result.UpdatedCmsDocuments.Should().HaveCount(1);
        result.UpdatedCmsDocuments.First().Item1.CmsDocumentId.Should().Be(docInEntity.CmsDocumentId);
        result.UpdatedCmsDocuments.First().Item2.Should().Be(DocumentDeltaType.RequiresIndexing);

        sut.CmsDocuments.First().IsOcrProcessed.Should().BeTrue();
    }
}

