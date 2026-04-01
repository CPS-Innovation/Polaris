// <copyright file="CompleteReclassificationRequestValidatorTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests.Validators;

using Common.Dto.Request.HouseKeeping;
using Cps.Fct.Hk.Ui.Services.Validators;
using Xunit;

/// <summary>
/// Tests complete reclassification request validator.
/// </summary>
public class CompleteReclassificationRequestValidatorTests
{
    private readonly CompleteReclassificationRequestValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteReclassificationRequestValidatorTests"/> class.
    /// </summary>
    public CompleteReclassificationRequestValidatorTests()
    {
        this.validator = new CompleteReclassificationRequestValidator();
    }

    /// <summary>
    /// Tests classification property.
    /// </summary>
    [Fact]
    public void CompleteReclassificationRequestValidator_Error_When_Classification_Not_Supplied()
    {
        // Arrange
        var reclassifyCaseMaterialRequest = new ReclassifyCaseMaterialRequest(
            "1212122",
            string.Empty,
            123,
            432,
            true,
            "this is a test subject",
            new ReclassifyStatementRequest(),
            exhibitRequest: null);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            null,
            null);

        // Act
        var result = this.validator.Validate(completeReclassificationRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "reclassification.classification");
    }

    /// <summary>
    /// Tests subject.
    /// </summary>
    [Fact]
    public void CompleteReclassificationRequestValidator_Error_When_Subject_Not_Supplied()
    {
        // Arrange
        var reclassifyCaseMaterialRequest = new ReclassifyCaseMaterialRequest(
            "1212122",
            "OTHER",
            123,
            -1,
            true,
            string.Empty,
            new ReclassifyStatementRequest(),
            exhibitRequest: null);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            null,
            null);

        // Act
        var result = this.validator.Validate(completeReclassificationRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "reclassification.subject");
    }

    /// <summary>
    /// Tests subject.
    /// </summary>
    [Fact]
    public void CompleteReclassificationRequestValidator_Error_When_DocumentTypeId_Not_Supplied()
    {
        // Arrange
        var reclassifyCaseMaterialRequest = new ReclassifyCaseMaterialRequest(
            "1212122",
            "OTHER",
            123,
            0,
            true,
            "this is subject.",
            new ReclassifyStatementRequest(),
            exhibitRequest: null);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            null,
            null);

        // Act
        var result = this.validator.Validate(completeReclassificationRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "reclassification.documentTypeId");
    }

    /// <summary>
    /// Tests subject.
    /// </summary>
    [Fact]
    public void CompleteReclassificationRequestValidator_Error_When_ReclassiyingToStatemment_But_StatementBodyIsNull()
    {
        // Arrange
        var reclassifyCaseMaterialRequest = new ReclassifyCaseMaterialRequest(
            "1212122",
            "STATEMENT",
            123,
            543,
            true,
            "this is subject.",
            statementRequest: null,
            exhibitRequest: null);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            null,
            null);

        // Act
        var result = this.validator.Validate(completeReclassificationRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Statement body must be provided.");
    }

    /// <summary>
    /// Tests subject.
    /// </summary>
    [Fact]
    public void CompleteReclassificationRequestValidator_Error_When_ReclassiyingToExhibit_But_ExhibitBodyIsNull()
    {
        // Arrange
        var reclassifyCaseMaterialRequest = new ReclassifyCaseMaterialRequest(
            "1212122",
            "EXHIBIT",
            123,
            543,
            true,
            "this is subject.",
            statementRequest: null,
            exhibitRequest: null);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            null,
            null);

        // Act
        var result = this.validator.Validate(completeReclassificationRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Exhibit body must be provided.");
    }
}
