using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetDocumentTypesTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetDocumentTypes_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/document-types
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetDocumentTypesAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetDocumentTypes_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetDocumentTypesAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetDocumentTypes_ShouldReturnOk_WithDocumentTypeGroups()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetDocumentTypesAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
    }

    [Test]
    public async Task GetDocumentTypes_ShouldReturnOK_AndValidateShape()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetDocumentTypesAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        Assert.That(items.Count, Is.GreaterThan(0), "Expected at least one DocumentTypeGroup.");

        Assert.Multiple(() =>
        {
            Assert.That(items.All(i => !string.IsNullOrWhiteSpace(i.Name)), Is.True, "Expected all document types to have Name.");
            Assert.That(items.All(i => !string.IsNullOrWhiteSpace(i.Group)), Is.True, "Expected all document types to have Group.");
            Assert.That(items.All(i => !string.IsNullOrWhiteSpace(i.Category)), Is.True, "Expected all document types to have Category.");
        });

        // Uniqueness
        var duplicateIds = items
            .GroupBy(i => i.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicateIds, Is.Empty, "Expected DocumentTypeGroup Id values to be unique.");
    }
}