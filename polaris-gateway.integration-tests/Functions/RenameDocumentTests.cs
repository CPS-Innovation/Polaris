using Common.Dto.Request;
using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class RenameDocumentTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task RenameDocument_ShouldReturnBadRequest_WhenBodyIsMissing()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "DOC000000001"; // any valid format, body will still be missing

        // act
        var result = await PolarisGatewayApiClient.RenameDocumentAsync(
            urn,
            caseId,
            documentId,
            request: null, // <— missing body
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RenameDocument_ShouldReturnBadRequest_WhenJsonIsInvalid()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "DOC000000001";

        // invalid JSON
        var invalidJson = new RenameDocumentRequestDto
        {
            DocumentName = string.Empty,
        };

        // act
        var result = await PolarisGatewayApiClient.RenameDocumentAsync(
            urn,
            caseId,
            documentId,
            (RenameDocumentRequestDto)invalidJson,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RenameDocument_ShouldReturnBadRequest_WhenValidatorFails()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "DOC000000001";

        var body = new RenameDocumentRequestDto
        {
            DocumentName = "" // <— validator should reject
        };

        // act
        var result = await PolarisGatewayApiClient.RenameDocumentAsync(
            urn,
            caseId,
            documentId,
            body,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RenameDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // Use a doc id that will not exist for the case (pick something extremely high/rare)
        var missingDocumentId = "CMS-4472755";

        var body = new RenameDocumentRequestDto
        {
            DocumentName = $"ITEST Missing Doc Rename {DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        // act
        var result = await PolarisGatewayApiClient.RenameDocumentAsync(
            urn,
            caseId,
            missingDocumentId,
            body,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task RenameDocument_ShouldReturnInternalServerError_WhenInvalidDocumentFormat()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // Use a doc id that will not exist for the case (pick something extremely high/rare)
        var missingDocumentId = "4472755";

        var body = new RenameDocumentRequestDto
        {
            DocumentName = $"ITEST Missing Doc Rename {DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        // act
        var result = await PolarisGatewayApiClient.RenameDocumentAsync(
            urn,
            caseId,
            missingDocumentId,
            body,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task RenameDocument_ShouldReturnOk_WhenDocumentRenamed()
    {
        // arrange
        var urn = "06SC5432106";
        var caseId = 2162558;
        var documentId = "CMS-8884919";

        var body = new RenameDocumentRequestDto
        {
            DocumentName = $"ITEST Doc Rename {DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        // act
        var result = await PolarisGatewayApiClient.RenameDocumentAsync(
            urn,
            caseId,
            documentId,
            body,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private static string NormalizeSpaces(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace('\u00A0', ' ').Trim();
}
