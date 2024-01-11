using AutoFixture;
using Common.Domain.SearchIndex;
using Common.Mappers;
using Common.Mappers.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.Tests.Mappers;

public class StreamlinedSearchLineMapperTests
{
    private readonly Fixture _fixture;
    private readonly Guid _correlationId;

    public StreamlinedSearchLineMapperTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void GivenASearchLine_ReturnAStreamlinedVersion()
    {
        var searchLine = _fixture.Create<SearchLine>();

        IStreamlinedSearchLineMapper mapper = new StreamlinedSearchLineMapper();
        var streamlinedVersion = mapper.Map(searchLine);

        using (new AssertionScope())
        {
            streamlinedVersion.Id.Should().Be(searchLine.Id);
            streamlinedVersion.LineIndex.Should().Be(searchLine.LineIndex);
            streamlinedVersion.PageIndex.Should().Be(searchLine.PageIndex);
            streamlinedVersion.Text.Should().Be(searchLine.Text);
            streamlinedVersion.PageHeight.Should().Be(searchLine.PageHeight);
            streamlinedVersion.PageWidth.Should().Be(searchLine.PageWidth);
        }
    }
}
