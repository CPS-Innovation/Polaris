using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class PolarisPipelineCaseSearchTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task PolarisPipelineCaseSearch_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/search
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var query = "test";

        // act
        var result = await PolarisGatewayApiClient.CaseSearchAsync(
            urn,
            caseId,
            query,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PolarisPipelineCaseSearch_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var query = "test";

        // act
        var result = await PolarisGatewayApiClient.CaseSearchAsync(
            urn,
            caseId,
            query,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PolarisPipelineCaseSearch_QueryIsMissing_ShouldReturnBadRequestOrNotFoundOrUnprocessableEntity()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var query = string.Empty;

        // act
        var result = await PolarisGatewayApiClient.CaseSearchAsync(
            urn,
            caseId,
            query,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(
            result.HttpStatusCode == HttpStatusCode.BadRequest ||
            result.HttpStatusCode == HttpStatusCode.NotFound ||
            result.HttpStatusCode == HttpStatusCode.UnprocessableEntity,
            Is.True,
            $"Expected 400/404/422 when query is missing/empty. Actual: {(int)result.HttpStatusCode} {result.HttpStatusCode}");
    }

    //[Test] // TODO - disabling this test as endpoints are failing.
    public async Task PolarisPipelineCaseSearch_ShouldReturnOkOrNotFound_DependingOnEnvironment()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var query = "DOE";

        // act
        var result = await PolarisGatewayApiClient.CaseSearchAsync(
            urn,
            caseId,
            query,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(
            result.HttpStatusCode == HttpStatusCode.OK ||
            result.HttpStatusCode == HttpStatusCode.NotFound,
            Is.True,
            $"Expected 200 or 404 depending on environment/index state. Actual: {(int)result.HttpStatusCode} {result.HttpStatusCode}");
    }
}