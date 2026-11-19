// <copyright file="CmsPrefixUsageEventTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.TelemetryEvents;

using FluentAssertions;
using PolarisGateway.TelemetryEvents;
using Xunit;

public class CmsPrefixUsageEventTests
{
    [Fact]
    public void Constructor_SetsCaseIdDocumentIdAndOperationName()
    {
        // arrange
        var caseId = 12345;
        var documentId = "CMS-98765";
        var operationName = "GetPdf";

        // act
        var sut = new CmsPrefixUsageEvent(caseId, documentId, operationName);

        // assert
        sut.CaseId.Should().Be(caseId);
        sut.DocumentId.Should().Be(documentId);
        sut.OperationName.Should().Be(operationName);
    }

    [Fact]
    public void EventName_ReturnsTypeName()
    {
        var sut = new CmsPrefixUsageEvent(1, "CMS-1", "GetOcr");

        sut.EventName.Should().Be(nameof(CmsPrefixUsageEvent));
    }

    [Fact]
    public void ToTelemetryEventProps_IncludesCaseIdDocumentIdAndOperationName()
    {
        // arrange
        var caseId = 555;
        var documentId = "CMS-111";
        var operationName = "GetThumbnail";
        var sut = new CmsPrefixUsageEvent(caseId, documentId, operationName);

        // act
        var (properties, metrics) = sut.ToTelemetryEventProps();

        // assert
        properties.Should().ContainKey("CaseId").WhoseValue.Should().Be(caseId.ToString());
        properties.Should().ContainKey("DocumentId").WhoseValue.Should().Be(documentId);
        sut.OperationName.Should().Be(operationName);
        metrics.Should().NotBeNull();
    }
}
