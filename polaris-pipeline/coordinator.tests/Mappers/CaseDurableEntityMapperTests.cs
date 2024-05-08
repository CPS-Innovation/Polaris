using Microsoft.Extensions.DependencyInjection;
using Xunit;
using coordinator.Mappers;
using AutoFixture;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Durable.Entity;
using FluentAssertions;
using System.Linq;
using Common.Dto.Tracker;

namespace coordinator.tests.Mappers;

public class CaseDurableEntityMapperTests
{
    private readonly Fixture _fixture;

    private readonly CaseDurableEntityMapper _caseDurableEntityMapper;

    public CaseDurableEntityMapperTests()
    {
        new ServiceCollection().RegisterMapsterConfiguration();
        _fixture = new Fixture();
        _caseDurableEntityMapper = new CaseDurableEntityMapper();
    }

    [Fact]
    public void Populated_TrackerEntity_MapsTo_TrackerDto()
    {
        // Arrange
        var caseEntity = _fixture.Create<CaseDurableEntity>();
        caseEntity.CmsDocuments[0].CategoryListOrder = 1;

        // Act
        var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity);


        // Assert
        trackerDto.Documents.Count.Should().Be(caseEntity.CmsDocuments.Count + caseEntity.PcdRequests.Count + 1);
        trackerDto.Documents.Count(d => d.CategoryListOrder != null).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Empty_TrackerEntity_MapsTo_TrackerDto()
    {
        // Arrange
        var caseEntity = new CaseDurableEntity();

        // Act
        var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity);

        // Assert
        trackerDto.Documents.Count.Should().Be(0);
        trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
    }

    [Fact]
    public void Null_TrackerEntity_MapsTo_TrackerDto()
    {
        // Arrange
        CaseDurableEntity caseEntity = null;

        // Act
        var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity);

        // Assert
        trackerDto.Documents.Count.Should().Be(0);
        trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
    }
}