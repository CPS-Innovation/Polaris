using Microsoft.Extensions.DependencyInjection;
using Xunit;
using coordinator.Mappers;
using AutoFixture;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using FluentAssertions;
using System.Linq;
using Common.Dto.Response.Documents;
using coordinator.Domain;

namespace coordinator.tests.Mappers;

public class CaseDurableEntityMapperTests
{
    private readonly Fixture _fixture;

    private readonly CaseDurableEntityMapper _caseDurableEntityMapper;

    public CaseDurableEntityMapperTests()
    {
        new ServiceCollection().RegisterCoordinatorMapsterConfiguration();
        _fixture = new Fixture();
        _caseDurableEntityMapper = new CaseDurableEntityMapper();
    }

    [Fact]
    public void Populated_TrackerEntity_MapsTo_TrackerDto()
    {
        // Arrange
        var caseEntity = _fixture.Create<CaseDurableEntityState>();
        var caseDurableEntityDocuments = _fixture.Create<CaseDurableEntityDocumentsState>();
        caseDurableEntityDocuments.CmsDocuments[0].CategoryListOrder = 1;
        caseDurableEntityDocuments.DefendantsAndCharges.HasMultipleDefendants = true;
        // Act
        var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity, caseDurableEntityDocuments);

        // Assert
        trackerDto.Documents.Count.Should().Be(caseDurableEntityDocuments.CmsDocuments.Count + caseDurableEntityDocuments.PcdRequests.Count + 1);
        trackerDto.Documents.Count(d => d.CategoryListOrder != null).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Empty_TrackerEntity_MapsTo_TrackerDto()
    {
        // Arrange
        var caseEntity = _fixture.Create<CaseDurableEntityState>();
        var caseDurableEntityDocuments = new CaseDurableEntityDocumentsState();

        // Act
        var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity, caseDurableEntityDocuments);

        // Assert
        trackerDto.Documents.Count.Should().Be(0);
        trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
    }

    [Fact]
    public void Null_TrackerEntity_MapsTo_TrackerDto()
    {
        // Arrange
        // Act
        var trackerDto = _caseDurableEntityMapper.MapCase(null, null);

        // Assert
        trackerDto.Documents.Count.Should().Be(0);
        trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
    }
}