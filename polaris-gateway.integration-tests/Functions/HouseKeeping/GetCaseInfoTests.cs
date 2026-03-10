using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseInfoTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseInfo_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseInfo_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        // missing urn fails routing => NotFound
        var urn = "";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseInfo_ShouldReturnOkWithCaseSummaryResponse()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        Assert.Multiple(() =>
        {
            Assert.That(obj.CaseId, Is.EqualTo(caseId));
            Assert.That(obj.Urn, Is.EqualTo(urn));
            Assert.That(obj.UnitName, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task GetCaseInfo_ShouldReturnOK_AndValidateData()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseInfoAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        Assert.Multiple(() =>
        {
            Assert.That(obj.CaseId, Is.EqualTo(caseId));
            Assert.That(obj.Urn, Is.EqualTo(urn));

            // These should be stable for your seeded test dataset (same case as GetCaseTests)
            Assert.That(obj.LeadDefendantFirstNames, Is.EqualTo("John"));
            Assert.That(obj.LeadDefendantSurname, Is.EqualTo("DOE"));
            Assert.That(obj.NumberOfDefendants, Is.EqualTo(5));
            Assert.That(obj.UnitName, Is.EqualTo("Hull TU"));
        });
    }
}