using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using coordinator.Durable.Payloads.Domain;
using FluentAssertions;
using Xunit;

namespace coordinator.Durable.Entity;

public class CaseDurableEntityTests
{
    private readonly Fixture _fixture;

    public CaseDurableEntityTests()
    {
        _fixture = new Fixture();
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
        var existingDocId = _fixture.Create<long>();
        var newDocId = _fixture.Create<long>();

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
}

