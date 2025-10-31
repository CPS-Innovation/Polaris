// <copyright file="DocumentTypeMapperTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.Hk.Ui.Services.Tests;

using Castle.Core.Configuration;
using Common.Dto.Response.HouseKeeping;
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
        Assert.Equal("Other Material", result.Category);
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
        Assert.Equal("MG Form", result.Category);
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
        Assert.Equal("Unknown", result.Category);
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
        Assert.Equal(68, result.Count);

        // Statement
        Assert.Equal(1, result.Count(x => x.Group == "Statement"));

        // Exhibit
        Assert.Equal(5, result.Count(x => x.Group == "Exhibit"));

        // MG Form
        Assert.Equal(47, result.Count(x => x.Group == "MG Form"));

        // Other
        Assert.Equal(15, result.Count(x => x.Group == "Other"));
    }
}
