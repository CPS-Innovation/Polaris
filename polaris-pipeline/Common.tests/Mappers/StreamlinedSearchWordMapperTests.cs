using AutoFixture;
using Common.Domain.SearchIndex;
using Common.Mappers;
using Common.Mappers.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Xunit;

namespace Common.Tests.Mappers;

public class StreamlinedSearchWordMapperTests
{
    private readonly Fixture _fixture;
    private readonly string _searchTerm;
    private readonly Guid _correlationId;

    public StreamlinedSearchWordMapperTests()
    {
        _fixture = new Fixture();

        _searchTerm = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();
    }

    [Fact]
    public void GivenASearchWordThatMatchesTheSearchTerm_ThenTheMapperProvidesAllDetail()
    {
        var searchLine = _fixture.Create<SearchLine>();
        searchLine.Words = _fixture.CreateMany<Word>(1).ToList();
        searchLine.Words[0].Text = _searchTerm;

        IStreamlinedSearchWordMapper mapper = new StreamlinedSearchWordMapper();
        var result = mapper.Map(searchLine.Words[0], _searchTerm, _correlationId);

        using (new AssertionScope())
        {
            result.Text.Should().Be(_searchTerm);
            result.BoundingBox.Should().BeEquivalentTo(searchLine.Words[0].BoundingBox);
        }
    }

    [Fact]
    public void GivenASearchWordThatDoesNotMatchTheSearchTerm_ThenTheMapperProvidesNoBoundingBoxDetail()
    {
        var searchLine = _fixture.Create<SearchLine>();
        searchLine.Words = _fixture.CreateMany<Word>(1).ToList();

        IStreamlinedSearchWordMapper mapper = new StreamlinedSearchWordMapper();
        var result = mapper.Map(searchLine.Words[0], _searchTerm, _correlationId);

        result.BoundingBox.Should().BeNull();
    }

    [Theory]
    [InlineData(" police. ", "police")]
    [InlineData(" police, ", "police")]
    [InlineData(" police! ", "police")]
    public void GivenASearchTermThatIsFoundInASentenceWithPunctuation_ThenTheMapperProvidesDetail(string searchResultText, string searchTerm)
    {
        var searchLine = _fixture.Create<SearchLine>();
        searchLine.Words = _fixture.CreateMany<Word>(1).ToList();
        searchLine.Words[0].Text = searchResultText;

        IStreamlinedSearchWordMapper mapper = new StreamlinedSearchWordMapper();
        var result = mapper.Map(searchLine.Words[0], searchTerm, _correlationId);

        result.BoundingBox.Should().NotBeNull();
    }

    //tests expect fuzzy matches to return "None" for now
    [Theory]
    [InlineData("friends", "then I will", false, StreamlinedMatchType.None)]
    [InlineData("friends", "the trouble with Scotland is that is full of Scots", false, StreamlinedMatchType.None)]
    [InlineData("friends", "suspect/friends/family", true, StreamlinedMatchType.Exact)]
    [InlineData("friends", "William the Conqueror", false, StreamlinedMatchType.None)]
    [InlineData("friends", "suspect/enemies/family", false, StreamlinedMatchType.None)]
    [InlineData("friends", "friends, romans, countrymen", true, StreamlinedMatchType.Exact)]
    [InlineData("friends", "my friend's dog bit me on the rear", false, StreamlinedMatchType.None)]
    [InlineData("friends", "my friends' cars are posh", true, StreamlinedMatchType.Exact)]
    [InlineData("friends", "my friend told me she loved me!", false, StreamlinedMatchType.None)]
    [InlineData("friends", "she was awfully friendly for a presbyterian", false, StreamlinedMatchType.None)]
    [InlineData("friend", "then I will", false, StreamlinedMatchType.None)]
    [InlineData("friend", "the trouble with Scotland is that is full of Scots", false, StreamlinedMatchType.None)]
    [InlineData("friend", "suspect/friends/family", false, StreamlinedMatchType.None)]
    [InlineData("friend", "William the Conqueror", false, StreamlinedMatchType.None)]
    [InlineData("friend", "suspect/enemies/family", false, StreamlinedMatchType.None)]
    [InlineData("friend", "friends, romans, countrymen", false, StreamlinedMatchType.None)]
    [InlineData("friend", "my friend's dog bit me on the rear", true, StreamlinedMatchType.Exact)]
    [InlineData("friend", "my friends' cars are posh", false, StreamlinedMatchType.None)]
    [InlineData("friend", "my friend told me she loved me!", true, StreamlinedMatchType.Exact)]
    [InlineData("friend", "she was awfully friendly for a presbyterian", false, StreamlinedMatchType.None)]
    [InlineData("then", "then I will", true, StreamlinedMatchType.Exact)]
    [InlineData("then", "the trouble with Scotland is that is full of Scots", false, StreamlinedMatchType.None)]
    [InlineData("the", "then I will", false, StreamlinedMatchType.None)]
    [InlineData("the", "the trouble with Scotland is that is full of Scots", true, StreamlinedMatchType.Exact)]
    [InlineData("friend", "you are my best-friend", true, StreamlinedMatchType.Exact)]
    [InlineData("friend", "we are best-friends", false, StreamlinedMatchType.None)]
    [InlineData("friends", "we are best-friends.", true, StreamlinedMatchType.Exact)]
    [InlineData("friends", "we are best-fwiends.", false, StreamlinedMatchType.None)]
    public void Query_TestSearchResultsSelection_For_ExpectedIdentifiers(string searchTerm, string searchText, bool isBoundingBoxSet, StreamlinedMatchType expectedMatchType)
    {
        var searchLine = _fixture.Create<SearchLine>();
        searchLine.Words = _fixture.CreateMany<Word>(1).ToList();
        searchLine.Words[0].Text = searchText;

        IStreamlinedSearchWordMapper mapper = new StreamlinedSearchWordMapper();
        var result = mapper.Map(searchLine.Words[0], searchTerm, _correlationId);

        using (new AssertionScope())
        {
            if (isBoundingBoxSet)
                result.BoundingBox.Should().NotBeNull();
            else
                result.BoundingBox.Should().BeNull();

            result.StreamlinedMatchType.Should().Be(expectedMatchType);
            result.Text.Should().Be(searchText);
        }
    }
}

