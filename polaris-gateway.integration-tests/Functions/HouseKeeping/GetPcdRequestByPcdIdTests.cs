using System.Net;
using Common.Dto.Response.HouseKeeping.Pcd;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetPcdRequestByPcdIdTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetPcdRequestByPcdId_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var pcdId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdRequestByPcdId_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var pcdId = 156619;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdRequestByPcdId_ShouldReturnOk_WithPcdRequestDto()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var pcdId = 156619;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
    }

    [Test]
    public async Task GetPcdRequestByPcdId_ShouldReturnOK_AndValidateKeyCollectionsNotNull()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var pcdId = 156619;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        Assert.Multiple(() =>
        {
            Assert.That(obj.CaseOutline, Is.Not.Null);
            Assert.That(obj.Suspects, Is.Not.Null);
            Assert.That(obj.PoliceContactDetails, Is.Not.Null);
            Assert.That(obj.MaterialProvided, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetPcdRequestByPcdId_ShouldReturnOK_AndValidateData_StableSeededCase()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179141;
        var pcdId = 156620;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;


        // case outline
        Assert.That(obj.CaseOutline, Is.Not.Null);

        var firstOutline = obj.CaseOutline.First();
        Assert.That(firstOutline.Heading, Is.Not.Null.And.Not.Empty);
        Assert.That(firstOutline.TextWithCmsMarkup, Is.Not.Null);

        Assert.That(obj.Comments, Is.Null);

        // suspects
        Assert.That(obj.Suspects, Is.Not.Null);
        Assert.That(obj.Suspects!.Count, Is.EqualTo(5));

        Assert.That(obj.Suspects.All(s => s.ProposedCharges != null), Is.True);

        var suspectsBySurname = obj.Suspects!.ToDictionary(s => s.Surname);

        Assert.That(suspectsBySurname.ContainsKey("DOE"), Is.True);
        Assert.That(suspectsBySurname.ContainsKey("SMITH"), Is.True);
        Assert.That(suspectsBySurname.ContainsKey("BROWN"), Is.True);
        Assert.That(suspectsBySurname.ContainsKey("CLARK"), Is.True);
        Assert.That(suspectsBySurname.ContainsKey("JOHNSON"), Is.True);

        AssertSuspectBasic(suspectsBySurname["DOE"], expectedFirstNames: "John");
        AssertSuspectBasic(suspectsBySurname["SMITH"], expectedFirstNames: "Jane");
        AssertSuspectBasic(suspectsBySurname["BROWN"], expectedFirstNames: "Robert");
        AssertSuspectBasic(suspectsBySurname["CLARK"], expectedFirstNames: "Emily");
        AssertSuspectBasic(suspectsBySurname["JOHNSON"], expectedFirstNames: "Michael");
    }

    private static void AssertSuspectBasic(PcdRequestSuspect suspect, string expectedFirstNames)
    {
        Assert.Multiple(() =>
        {
            Assert.That(suspect.FirstNames, Is.EqualTo(expectedFirstNames));
            Assert.That(suspect.Surname, Is.Not.Null.And.Not.Empty);

            Assert.That(suspect.ProposedCharges, Is.Not.Null);
            Assert.That(suspect.ProposedCharges!.Count, Is.GreaterThanOrEqualTo(0));
        });
    }
}