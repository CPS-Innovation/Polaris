using Common.Domain.Document;
using Common.Exceptions;
using FluentAssertions;
using Xunit;

namespace Common.tests.Domain;

public class DocumentNatureTests
{
    [Fact]
    public void DocumentNature_GetStringPrefix_ReturnsTheExpectedPrefix()
    {
        DocumentNature.GetStringPrefix(DocumentNature.Types.Document).Should().Be("CMS");
        DocumentNature.GetStringPrefix(DocumentNature.Types.PreChargeDecisionRequest).Should().Be("PCD");
        DocumentNature.GetStringPrefix(DocumentNature.Types.DefendantsAndCharges).Should().Be("DAC");
    }

    [Fact]
    public void DocumentNature_GetStringPrefix_ThrowsExceptionForInvalidDocumentNature()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentNature.GetStringPrefix((DocumentNature.Types)3));
    }

    [Fact]
    public void DocumentNature_ToNumericDocumentId_ReturnsTheExpectedNumericDocumentId()
    {
        DocumentNature.ToNumericDocumentId("CMS-123456", DocumentNature.Types.Document).Should().Be(123456);
        DocumentNature.ToNumericDocumentId("PCD-123456", DocumentNature.Types.PreChargeDecisionRequest).Should().Be(123456);
        DocumentNature.ToNumericDocumentId("DAC-123456", DocumentNature.Types.DefendantsAndCharges).Should().Be(123456);
    }

    [Fact]
    public void DocumentNature_ToNumericDocumentId_ThrowsExceptionForInvalidDocumentId()
    {
        Assert.Throws<BadRequestException>(() => DocumentNature.ToNumericDocumentId("CMS-123456", DocumentNature.Types.PreChargeDecisionRequest));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void DocumentNature_ToNumericDocumentId_ThrowsExceptionForEmptyDocumentId(string? documentId)
    {
        Assert.Throws<ArgumentNullException>(() => DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.PreChargeDecisionRequest));
    }

    [Fact]
    public void DocumentNature_ToQualifiedStringDocumentId_ReturnsTheExpectedQualifiedStringDocumentId()
    {
        DocumentNature.ToQualifiedStringDocumentId(123456, DocumentNature.Types.Document).Should().Be("CMS-123456");
        DocumentNature.ToQualifiedStringDocumentId(123456, DocumentNature.Types.PreChargeDecisionRequest).Should().Be("PCD-123456");
        DocumentNature.ToQualifiedStringDocumentId(123456, DocumentNature.Types.DefendantsAndCharges).Should().Be("DAC-123456");
        DocumentNature.ToQualifiedStringDocumentId("123456", DocumentNature.Types.Document).Should().Be("CMS-123456");
        DocumentNature.ToQualifiedStringDocumentId("123456", DocumentNature.Types.PreChargeDecisionRequest).Should().Be("PCD-123456");
        DocumentNature.ToQualifiedStringDocumentId("123456", DocumentNature.Types.DefendantsAndCharges).Should().Be("DAC-123456");
    }

    [Fact]
    public void DocumentNature_GetDocumentNatureType_ReturnsTheExpectedDocumentNatureType()
    {
        DocumentNature.GetDocumentNatureType("CMS-123456").Should().Be(DocumentNature.Types.Document);
        DocumentNature.GetDocumentNatureType("PCD-123456").Should().Be(DocumentNature.Types.PreChargeDecisionRequest);
        DocumentNature.GetDocumentNatureType("DAC-123456").Should().Be(DocumentNature.Types.DefendantsAndCharges);
    }

    [Fact]
    public void DocumentNature_GetDocumentNatureType_ThrowsExceptionForInvalidDocumentId()
    {
        Assert.Throws<ArgumentException>(() => DocumentNature.GetDocumentNatureType("CMS123456"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void DocumentNature_GetDocumentNatureType_ThrowsExceptionForEmptyDocumentId(string? documentId)
    {
        Assert.Throws<ArgumentNullException>(() => DocumentNature.GetDocumentNatureType(documentId));
    }

    [Fact]
    public void DocumentNature_IsRedactable_ReturnsTheExpectedResult()
    {
        DocumentNature.IsRedactable(DocumentNature.Types.Document).Should().BeTrue();
        DocumentNature.IsRedactable(DocumentNature.Types.PreChargeDecisionRequest).Should().BeFalse();
        DocumentNature.IsRedactable(DocumentNature.Types.DefendantsAndCharges).Should().BeFalse();
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("0")]
    public void DocumentNature_Parse_ReturnsCmsIdentifier_ForBareNumericId(string materialId)
    {
        var result = DocumentNature.Parse(materialId);

        result.Nature.Should().Be(DocumentNature.Types.Document);
        result.NumericId.Should().Be(long.Parse(materialId));
        result.QualifiedId.Should().Be($"CMS-{materialId}");
        result.IsRedactable.Should().BeTrue();
    }

    [Fact]
    public void DocumentNature_Parse_ReturnsCmsIdentifier_ForCmsPrefixedId()
    {
        var result = DocumentNature.Parse("CMS-123456");

        result.Nature.Should().Be(DocumentNature.Types.Document);
        result.NumericId.Should().Be(123456);
        result.QualifiedId.Should().Be("CMS-123456");
        result.IsRedactable.Should().BeTrue();
    }

    [Fact]
    public void DocumentNature_Parse_ReturnsPcdIdentifier_ForPcdPrefixedId()
    {
        var result = DocumentNature.Parse("PCD-123456");

        result.Nature.Should().Be(DocumentNature.Types.PreChargeDecisionRequest);
        result.NumericId.Should().Be(123456);
        result.QualifiedId.Should().Be("PCD-123456");
        result.IsRedactable.Should().BeFalse();
    }

    [Fact]
    public void DocumentNature_Parse_ReturnsDacIdentifier_ForDacPrefixedId()
    {
        var result = DocumentNature.Parse("DAC-123456");

        result.Nature.Should().Be(DocumentNature.Types.DefendantsAndCharges);
        result.NumericId.Should().Be(123456);
        result.QualifiedId.Should().Be("DAC-123456");
        result.IsRedactable.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void DocumentNature_Parse_ThrowsBadRequestExceptionForEmptyMaterialId(string? materialId)
    {
        Assert.Throws<BadRequestException>(() => DocumentNature.Parse(materialId));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("cms-123456")] // lower case prefix not accepted
    [InlineData("XYZ-123456")]
    [InlineData("CMS-")]
    [InlineData("CMS123456")]
    [InlineData("CMS-123abc")]
    public void DocumentNature_Parse_ThrowsBadRequestExceptionForInvalidMaterialId(string materialId)
    {
        Assert.Throws<BadRequestException>(() => DocumentNature.Parse(materialId));
    }

    [Theory]
    [InlineData("123456", "CMS-123456")]
    [InlineData("CMS-123456", "CMS-123456")]
    [InlineData("PCD-123456", "PCD-123456")]
    [InlineData("DAC-123456", "DAC-123456")]
    public void DocumentNature_NormalizeMaterialId_ReturnsTheExpectedQualifiedId(string materialId, string expectedQualifiedId)
    {
        DocumentNature.NormalizeMaterialId(materialId).Should().Be(expectedQualifiedId);
    }

    [Fact]
    public void DocumentNature_NormalizeMaterialId_ThrowsBadRequestExceptionForInvalidMaterialId()
    {
        Assert.Throws<BadRequestException>(() => DocumentNature.NormalizeMaterialId("PCD-"));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("CMS-123456")]
    public void DocumentNature_ParseCmsOnly_ReturnsCmsIdentifier_ForNumericOrCmsPrefixedId(string materialId)
    {
        var result = DocumentNature.ParseCmsOnly(materialId);

        result.Nature.Should().Be(DocumentNature.Types.Document);
        result.NumericId.Should().Be(123456);
        result.QualifiedId.Should().Be("CMS-123456");
    }

    [Theory]
    [InlineData("PCD-123456")]
    [InlineData("DAC-123456")]
    public void DocumentNature_ParseCmsOnly_ThrowsBadRequestExceptionForNonCmsMaterialId(string materialId)
    {
        Assert.Throws<BadRequestException>(() => DocumentNature.ParseCmsOnly(materialId));
    }

    [Fact]
    public void DocumentNature_ParseCmsOnly_ThrowsBadRequestExceptionForInvalidMaterialId()
    {
        Assert.Throws<BadRequestException>(() => DocumentNature.ParseCmsOnly("not-a-valid-id"));
    }
}