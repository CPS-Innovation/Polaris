// <copyright file="CommunicationMapperTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using Cps.Fct.Hk.Ui.Interfaces.Enums;
using Xunit;

/// <summary>
/// Unit tests for CommunicationMapper.
/// </summary>
public class CommunicationMapperTests
{
    private readonly CommunicationMapper systemUnderTest;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunicationMapperTests"/> class.
    /// </summary>
    public CommunicationMapperTests()
    {
        this.systemUnderTest = new CommunicationMapper();
    }

    /// <summary>
    /// Test each communication party mappings.
    /// </summary>
    /// <param name="name">The value passed from DDEI to map.</param>
    /// <param name="expected">The expected value to dispay for HKUI.</param>
    [InlineData("Other", CommunicationParty.Other)]
    [InlineData("Counsel", CommunicationParty.Counsel)]
    [InlineData("Court", CommunicationParty.Court)]
    [InlineData("Defence", CommunicationParty.Defence)]
    [InlineData("Police", CommunicationParty.Police)]
    [InlineData("DigitalCaseSystem", CommunicationParty.DCS)]
    [InlineData("Wcu", CommunicationParty.WCU)]
    [Theory]
    public void MapCommunicationParty_ReturnsExpectedResult_WhenSuppliedPartyNameIsValid(string name, CommunicationParty expected)
    {
        // Act
        var result = this.systemUnderTest.MapCommunicationParty(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result);
    }

    /// <summary>
    /// Test for invalid input.
    /// </summary>
    /// <param name="name">The value passed from DDEI.</param>
    [InlineData("not found")]
    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public void MapCommunicationParty_ReturnsUnknownWhenSuppliedValueIsNotMatched(string? name)
    {
        // Act
        var result = this.systemUnderTest.MapCommunicationParty(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result);
    }

    /// <summary>
    /// Test each communication method mappings.
    /// </summary>
    /// <param name="name">The DDEI name to map.</param>
    /// <param name="expected">The display value.</param>
    [InlineData("EMAIL",  CommunicationMethod.Email)]
    [InlineData("PHONE", CommunicationMethod.Phone)]
    [InlineData("BUNDLE", CommunicationMethod.Bundle)]
    [InlineData("PHYSICAL_ITEM", CommunicationMethod.Item)]
    [InlineData("DOCUMENT", CommunicationMethod.Document)]
    [InlineData("MEETING", CommunicationMethod.Meeting)]
    [Theory]
    public void MapCommunicationMethod_ReturnsExpectedResult_WhenSuppliedMethodNameIsValid(string name, CommunicationMethod expected)
    {
        // Act
        var result = this.systemUnderTest.MapCommunicationMethod(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result);
    }

    /// <summary>
    /// Test invalid direction mappings.
    /// </summary>
    /// <param name="name">The DDEI direction to map.</param>
    [InlineData("not found")]
    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public void MapCommunicationMethod_ReturnsUnknownWhenSuppliedValueIsNotMatched(string? name)
    {
        // Act
        var result = this.systemUnderTest.MapCommunicationMethod(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result);
    }

    /// <summary>
    /// Test each valid direction mapppings.
    /// </summary>
    /// <param name="direction">The direction to map.</param>
    /// <param name="expected">The expected result.</param>
    [InlineData("In", "Incoming")]
    [InlineData("Out", "Outgoing")]
    [Theory]
    public void MapDirection_ReturnsExpectedResult_WhenSuppliedValueIsValid(string direction, string expected)
    {
        // Act
        var result = this.systemUnderTest.MapDirection(direction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result);
    }

    /// <summary>
    /// Test invalid direction mappings.
    /// </summary>
    /// <param name="name">The invalid value provided.</param>
    [InlineData("not found")]
    [InlineData("")]
    [InlineData(null)]
    [Theory]
    public void MapDirection_ReturnsUnknownWhenSuppliedValueIsNotMatched(string? name)
    {
        // Act
        var result = this.systemUnderTest.MapCommunicationMethod(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result);
    }
}
