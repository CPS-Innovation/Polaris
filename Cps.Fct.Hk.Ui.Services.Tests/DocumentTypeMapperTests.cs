// <copyright file="DocumentTypeMapperTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.Hk.Ui.Services.Tests;

using Castle.Core.Configuration;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Services.Constants;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="DocumentTypeMapper"/> class.
/// </summary>
public class DocumentTypeMapperTests
{
    private readonly DocumentTypeMapper documentTypeMapper;
    private readonly TestLogger<DocumentTypeMapper> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeMapperTests"/> class.
    /// </summary>
    public DocumentTypeMapperTests()
    {
        this.mockLogger = new TestLogger<DocumentTypeMapper>();
        this.documentTypeMapper = new DocumentTypeMapper(this.mockLogger);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapDocumentType"/> returns the correct <see cref="DocumentTypeInfo"/>
    /// when a valid document type ID is provided.
    /// </summary>
    [Fact]
    public void MapDocumentType_ValidId_ReturnsCorrectDocumentTypeInfo()
    {
        // Arrange
        int documentTypeId = 1201; // A valid documentTypeId from the dictionary

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapDocumentType(documentTypeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABE", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapDocumentType"/> returns the correct <see cref="DocumentTypeInfo"/>
    /// for another valid document type ID.
    /// </summary>
    [Fact]
    public void MapDocumentType_AnotherValidId_ReturnsCorrectDocumentTypeInfo()
    {
        // Arrange
        int documentTypeId = 1055; // Another valid documentTypeId

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapDocumentType(documentTypeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DREP", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.Communication, result.Category);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapDocumentType"/> returns "Unknown" values for an invalid document type ID.
    /// </summary>
    [Fact]
    public void MapDocumentType_InvalidId_ReturnsUnknownDocumentTypeInfo()
    {
        // Arrange
        int invalidDocumentTypeId = 9999; // A documentTypeId that does not exist in the mapping

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapDocumentType(invalidDocumentTypeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
        Assert.Equal(DocumentTypeGroups.Other, result.Group);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapMaterialType"/> returns correct DocumentTypeInfo when valid string ID is provided.
    /// </summary>
    [Fact]
    public void MapMaterialType_ValidStringId_ReturnsCorrectDocumentTypeInfo()
    {
        // Arrange
        string materialType = "1201"; // String representation of a valid documentTypeId

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapMaterialType(materialType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABE", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapMaterialType"/> returns "Unknown" values when invalid string ID is provided.
    /// </summary>
    [Fact]
    public void MapMaterialType_InvalidStringId_ReturnsUnknownDocumentTypeInfo()
    {
        // Arrange
        string invalidMaterialType = "9999"; // String representation of an invalid documentTypeId

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapMaterialType(invalidMaterialType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
        Assert.Equal(DocumentTypeGroups.Other, result.Group);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapMaterialType"/> returns "Unknown" values when non-numeric string is provided.
    /// </summary>
    [Fact]
    public void MapMaterialType_NonNumericString_ReturnsUnknownDocumentTypeInfo()
    {
        // Arrange
        string nonNumericMaterialType = "InvalidType"; // Non-numeric string

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapMaterialType(nonNumericMaterialType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
        Assert.Equal(DocumentTypeGroups.Other, result.Group);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapMaterialType"/> returns "Unknown" values when empty string is provided.
    /// </summary>
    [Fact]
    public void MapMaterialType_EmptyString_ReturnsUnknownDocumentTypeInfo()
    {
        // Arrange
        string emptyMaterialType = string.Empty;

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapMaterialType(emptyMaterialType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
        Assert.Equal(DocumentTypeGroups.Other, result.Group);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapDocumentType"/> returns "Unknown" for zero as document type ID.
    /// </summary>
    [Fact]
    public void MapDocumentType_ZeroId_ReturnsUnknownDocumentTypeInfo()
    {
        // Arrange
        int zeroDocumentTypeId = 0;

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapDocumentType(zeroDocumentTypeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
        Assert.Equal(DocumentTypeGroups.Other, result.Group);
    }

    /// <summary>
    /// Tests that <see cref="DocumentTypeMapper.MapDocumentType"/> returns "Unknown" for negative document type ID.
    /// </summary>
    [Fact]
    public void MapDocumentType_NegativeId_ReturnsCorrectOrUnknownDocumentTypeInfo()
    {
        // Arrange - negative IDs can be valid in the mapping (e.g., -1, -2, etc.)
        int negativeDocumentTypeId = -999; // A negative ID that doesn't exist

        // Act
        DocumentTypeInfo result = this.documentTypeMapper.MapDocumentType(negativeDocumentTypeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.DocumentType);
        Assert.Equal(DocumentTypeCategories.OtherMaterial, result.Category);
        Assert.Equal(DocumentTypeGroups.Other, result.Group);
    }

    /// <summary>
    /// Tests that GetDocumentTypeGroups method returns document types which have association with a group.
    /// </summary>
    [Fact]
    public void GetDocumentTypeGroups_ReturnsDocumentTypesAssociatedWithAClassificationGroup()
    {
        // Act
        var result = this.documentTypeMapper.GetDocumentTypesWithClassificationGroup();

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain(result, g => g.Group == null);
    }

    /// <summary>
    ///  Tests that GetDocumentTypeGroups method returns all expected reclassifiable document types.
    /// </summary>
    [Fact]
    public void GetDocumentTypeGroups_ReturnsAllReclassifiableDocumentTypes()
    {
        // Act
        var result = this.documentTypeMapper.GetDocumentTypesWithClassificationGroup();
        Assert.NotNull(result);

        // Total
        Assert.Equal(73, result.Count);

        // Statement
        Assert.Equal(5, result.Count(x => x.Group == DocumentTypeGroups.Statement));

        // Exhibit
        Assert.Equal(12, result.Count(x => x.Group == DocumentTypeGroups.Exhibit));

        // MG Form
        Assert.Equal(43, result.Count(x => x.Group == DocumentTypeGroups.MgForm));

        // Other
        Assert.Equal(13, result.Count(x => x.Group == DocumentTypeGroups.Other));
    }
}
