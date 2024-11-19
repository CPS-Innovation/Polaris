﻿using AutoFixture;
using Common.Domain.SearchIndex;
using text_extractor.Factories;
using text_extractor.Factories.Contracts;
using text_extractor.Mappers.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Moq;
using Xunit;
using System.Linq;
using System;

namespace text_extractor.tests.Factories;

public class StreamlinedSearchResultFactoryTests
{
    private readonly Fixture _fixture;

    private readonly Mock<IStreamlinedSearchLineMapper> _streamlinedSearchLineMapper;
    private readonly Mock<IStreamlinedSearchWordMapper> _streamlinedSearchWordMapper;
    private readonly IStreamlinedSearchResultFactory _streamlinedSearchResultFactory;

    public StreamlinedSearchResultFactoryTests()
    {
        _fixture = new Fixture();
        _streamlinedSearchLineMapper = new Mock<IStreamlinedSearchLineMapper>();
        _streamlinedSearchWordMapper = new Mock<IStreamlinedSearchWordMapper>();
        _streamlinedSearchResultFactory = new StreamlinedSearchResultFactory(_streamlinedSearchLineMapper.Object, _streamlinedSearchWordMapper.Object);
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

        _streamlinedSearchLineMapper.Setup(s => s.Map(It.IsAny<SearchLine>()))
            .Returns(fakeStreamlinedResult);

        _streamlinedSearchWordMapper.Setup(s => s.Map(It.IsAny<Word>(), It.IsAny<string>()))
            .Returns(wordsFound[0]);

        var result = _streamlinedSearchResultFactory.Create(fakeSearchResults, searchTerm);

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

        _streamlinedSearchLineMapper.Setup(s => s.Map(It.IsAny<SearchLine>()))
            .Returns(fakeStreamlinedResult);

        _streamlinedSearchWordMapper.Setup(s => s.Map(It.IsAny<Word>(), It.IsAny<string>()))
            .Returns(wordsFound[0]);

        var result = _streamlinedSearchResultFactory.Create(fakeSearchResults, _fixture.Create<string>());

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