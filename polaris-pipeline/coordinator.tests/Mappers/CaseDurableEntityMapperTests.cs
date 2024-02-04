using System.Linq;
using AutoFixture;
using coordinator.Domain.Mapper;
using FluentAssertions;
using Xunit;
using coordinator.Functions.Durable.Entity;
using Common.Dto.Tracker;
using Microsoft.Extensions.DependencyInjection;

namespace coordinator.Mappers
{
    public class CaseDurableEntityMapperTests
    {
        private readonly Fixture _fixture;
        private readonly IServiceCollection _services;

        private readonly CaseDurableEntityMapper _caseDurableEntityMapper;
        public CaseDurableEntityMapperTests()
        {
            _fixture = new Fixture();
            _services = new ServiceCollection();

            _caseDurableEntityMapper = new CaseDurableEntityMapper();
        }

        [Fact]
        public void Populated_TrackerEntity_MapsTo_TrackerDto()
        {
            // Arrange
            _services.RegisterMapsterConfiguration();
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
            _services.RegisterMapsterConfiguration();
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
            _services.RegisterMapsterConfiguration();
            CaseDurableEntity caseEntity = null;

            // Act
            var trackerDto = _caseDurableEntityMapper.MapCase(caseEntity);

            // Assert
            trackerDto.Documents.Count.Should().Be(0);
            trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
        }
    }
}