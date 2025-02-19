using Common.Domain.Document;
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
        Assert.Throws<ArgumentException>(() => DocumentNature.ToNumericDocumentId("CMS-123456", DocumentNature.Types.PreChargeDecisionRequest));
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
}