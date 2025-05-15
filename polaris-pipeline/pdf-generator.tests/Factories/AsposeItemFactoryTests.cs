﻿using System;
using System.IO;
using System.Text;
using AutoFixture;
using FluentAssertions;
using pdf_generator.Factories;
using Xunit;

namespace pdf_generator.tests.Factories;

public class AsposeItemFactoryTests
{
    private readonly Guid _correlationId;
    private readonly AsposeItemFactory _asposeItemFactory;

    public AsposeItemFactoryTests()
    {
        var fixture = new Fixture();
        _correlationId = fixture.Create<Guid>();
        _asposeItemFactory = new AsposeItemFactory();
    }

    [Fact]
    public void CreateWorkbook_ReturnsValidObject()
    {
        using var testStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestBook.xlsx");
        var result = _asposeItemFactory.CreateWorkbook(testStream, _correlationId);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateDiagram_ReturnsValidObject()
    {
        using var testStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestDiagram.vsd");
        var result = _asposeItemFactory.CreateDiagram(testStream, _correlationId);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateMailMessage_ReturnsValidObject()
    {
        using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
        var result = _asposeItemFactory.CreateMailMessage(testStream, _correlationId);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateMhtmlDocument_ReturnsValidObject()
    {
        using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
        var result = _asposeItemFactory.CreateMhtmlDocument(testStream, _correlationId);

        result.Should().NotBeNull();
    }

    // todo: following test fails on mac (at least Stef's mac at time of writing)
#if Windows
    [Fact]
    public void CreateHtmlDocument_ReturnsValidObject()
    {
        using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
        var result = _asposeItemFactory.CreateHtmlDocument(testStream, _correlationId);

        result.Should().NotBeNull();
    }
#endif
    [Fact]
    public void CreateImage_ReturnsValidObject()
    {
        using var testStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestImage.png");
        var result = _asposeItemFactory.CreateImage(testStream, _correlationId);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CreatePresentation_ReturnsValidObject()
    {
        using var testStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestPresentation.pptx");
        var result = _asposeItemFactory.CreatePresentation(testStream, _correlationId);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateWords_ReturnsValidObject()
    {
        using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
        var result = _asposeItemFactory.CreateWordsDocument(testStream, _correlationId);

        result.Should().NotBeNull();
    }
}
