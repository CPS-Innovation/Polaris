using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseLockInfoTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseLockInfo_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/case-lock-info
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseLockInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseLockInfo_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = string.Empty;
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseLockInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseLockInfo_ShouldReturnOkWithCaseLockedStatusResult()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseLockInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        Assert.That(obj.IsLocked, Is.TypeOf<bool>());
        Assert.That(obj.IsLockedByCurrentUser, Is.TypeOf<bool>());
    }

    [Test]
    public async Task GetCaseLockInfo_ShouldReturnOK_AndValidateLockFieldsAreConsistent()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseLockInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        // Consistency rules (avoid brittle "exact strings" assertions)
        Assert.Multiple(() =>
        {
            if (obj.IsLocked)
            {
                // When locked, these are typically populated
                Assert.That(obj.LockedByUser, Is.Not.Null, "Expected LockedByUser to be present when IsLocked=true.");
                Assert.That(obj.CaseLockedMessage, Is.Not.Null, "Expected CaseLockedMessage to be present when IsLocked=true.");
            }
            else
            {
                // When not locked, IsLockedByCurrentUser should not be true.
                Assert.That(obj.IsLockedByCurrentUser, Is.False, "Expected IsLockedByCurrentUser=false when IsLocked=false.");
            }
        });
    }
}