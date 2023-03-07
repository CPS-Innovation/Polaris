using AutoFixture;
using Common.Domain.SearchIndex;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.Tests.Clients;

public class StreamlinedSearchResultFactoryTests
{
    private readonly Fixture _fixture;

    private readonly Mock<IStreamlinedSearchLineMapper> _streamlinedSearchLineMapper;
    private readonly Mock<IStreamlinedSearchWordMapper> _streamlinedSearchWordMapper;
    private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;
    private readonly Guid _correlationId;

    public StreamlinedSearchResultFactoryTests()
    {
        var loggerMock = new Mock<ILogger<StreamlinedSearchResultFactory>>();

        _fixture = new Fixture();
        _correlationId = _fixture.Create<Guid>();
        _streamlinedSearchLineMapper = new Mock<IStreamlinedSearchLineMapper>();
        _streamlinedSearchWordMapper = new Mock<IStreamlinedSearchWordMapper>();
        _streamlinedSearchResultFactory = new StreamlinedSearchResultFactory(_streamlinedSearchLineMapper.Object, _streamlinedSearchWordMapper.Object, loggerMock.Object);
    }

    [Fact]
    public void WhenASearchResultIsFound_TheFactoryCreatesAStreamlinedObjectWithBoundingBoxValues()
    {
        var searchTerm = _fixture.Create<string>();
        var fakeSearchResults = _fixture.Create<SearchLine>();
        fakeSearchResults.Words = _fixture.CreateMany<Word>(2).ToList();

        var wordsFound = _fixture.CreateMany<StreamlinedWord>(2).ToList();
        foreach (var word in wordsFound)
        {
            word.Text = string.Join(" ", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), searchTerm, Guid.NewGuid().ToString());
        }

        var fakeStreamlinedResult = _fixture.Create<StreamlinedSearchLine>();
        fakeStreamlinedResult.Words = wordsFound;

        _streamlinedSearchLineMapper.Setup(s => s.Map(It.IsAny<SearchLine>(), It.IsAny<Guid>()))
            .Returns(fakeStreamlinedResult);

        _streamlinedSearchWordMapper.Setup(s => s.Map(It.IsAny<Word>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(wordsFound[0]);

        var result = _streamlinedSearchResultFactory.Create(fakeSearchResults, searchTerm, _correlationId);

        using (new AssertionScope())
        {
            result.Id.Should().Be(fakeStreamlinedResult.Id);
            //result.DocumentId.Should().Be(fakeStreamlinedResult.DocumentId);
            result.Words.Count.Should().Be(4);
            result.Words[0].BoundingBox.Should().NotBeNull();
            result.Words[1].BoundingBox.Should().NotBeNull();
        }
    }

    [Fact]
    public void WhenASearchResultIsNotFound_TheFactoryCreatesAStreamlinedObjectWithoutBoundingBoxValues()
    {
        var fakeSearchResults = _fixture.Create<SearchLine>();
        fakeSearchResults.Words = _fixture.CreateMany<Word>(2).ToList();

        var wordsFound = _fixture.CreateMany<StreamlinedWord>(2).ToList();
        foreach (var word in wordsFound)
        {
            word.Text = string.Join(" ", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            word.BoundingBox = null;
        }

        var fakeStreamlinedResult = _fixture.Create<StreamlinedSearchLine>();
        fakeStreamlinedResult.Words = wordsFound;

        _streamlinedSearchLineMapper.Setup(s => s.Map(It.IsAny<SearchLine>(), It.IsAny<Guid>()))
            .Returns(fakeStreamlinedResult);

        _streamlinedSearchWordMapper.Setup(s => s.Map(It.IsAny<Word>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(wordsFound[0]);

        var result = _streamlinedSearchResultFactory.Create(fakeSearchResults, _fixture.Create<string>(), _correlationId);

        using (new AssertionScope())
        {
            result.Id.Should().Be(fakeStreamlinedResult.Id);
            //result.DocumentId.Should().Be(fakeStreamlinedResult.DocumentId);
            result.Words.Count.Should().Be(4);
            result.Words[0].BoundingBox.Should().BeNull();
            result.Words[1].BoundingBox.Should().BeNull();
        }
    }
}