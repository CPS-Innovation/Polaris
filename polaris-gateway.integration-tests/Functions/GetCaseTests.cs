using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCase_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "54KR7689125";
        var caseId = 0;

        //act
        var result = await PolarisGatewayApiClient.GetCaseAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCase_ShouldReturnOkWithCaseDto()
    {
        //arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        //act
        var result = await PolarisGatewayApiClient.GetCaseAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
        Assert.That(result.ResponseObject.Id, Is.EqualTo(caseId));
        Assert.That(result.ResponseObject.UniqueReferenceNumber, Is.EqualTo(urn));
    }


    [Test]
    public async Task GetCase_ShouldReturnOK_AndValidateData()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject;

        Assert.Multiple(() =>
        {
            // top-level
            Assert.That(obj.Id, Is.EqualTo(caseId));
            Assert.That(obj.UniqueReferenceNumber, Is.EqualTo(urn));
            Assert.That(obj.IsCaseCharged, Is.False);
            Assert.That(obj.NumberOfDefendants, Is.EqualTo(5));
            Assert.That(obj.OwningUnit, Is.EqualTo("Hull TU"));

            // leadDefendantDetails
            Assert.That(obj.LeadDefendantDetails, Is.Not.Null);
            Assert.That(obj.LeadDefendantDetails.Id, Is.EqualTo(2824597));
            Assert.That(obj.LeadDefendantDetails.ListOrder, Is.EqualTo(1));
            Assert.That(obj.LeadDefendantDetails.FirstNames, Is.EqualTo("John"));
            Assert.That(obj.LeadDefendantDetails.Surname, Is.EqualTo("DOE"));
            Assert.That(obj.LeadDefendantDetails.OrganisationName, Is.EqualTo("DOE"));
            Assert.That(obj.LeadDefendantDetails.Dob, Is.Null);
            Assert.That(obj.LeadDefendantDetails.Age, Is.EqualTo(string.Empty));
            Assert.That(obj.LeadDefendantDetails.IsYouth, Is.False);
            Assert.That(obj.LeadDefendantDetails.Type, Is.EqualTo("Person"));

            // headlineCharge
            Assert.That(obj.HeadlineCharge, Is.Not.Null);
            Assert.That(obj.HeadlineCharge.Charge, Is.EqualTo("TH68027 Burglary other than dwelling with intent to steal"));
            Assert.That(obj.HeadlineCharge.Date, Is.Null);
            Assert.That(obj.HeadlineCharge.EarlyDate, Is.EqualTo("2025-07-03"));
            Assert.That(obj.HeadlineCharge.LateDate, Is.EqualTo("2025-07-04"));
            Assert.That(obj.HeadlineCharge.NextHearingDate, Is.Null);

            // defendants collection
            Assert.That(obj.DefendantsAndCharges, Is.Not.Null);
            Assert.That(obj.DefendantsAndCharges.Count, Is.EqualTo(5));

            // witnesses
            Assert.That(obj.Witnesses, Is.Not.Null);
            Assert.That(obj.Witnesses.Count, Is.EqualTo(0));

            // preChargeDecisionRequests
            Assert.That(obj.PreChargeDecisionRequests, Is.Not.Null);
            Assert.That(obj.PreChargeDecisionRequests.Count, Is.EqualTo(1));
        });

        // ===== Defendants (validate each one) =====
        var defendants = obj.DefendantsAndCharges.OrderBy(d => d.ListOrder).ToList();

        // 1) John DOE
        AssertDefendant(
            defendants[0],
            expectedId: 2824597,
            expectedListOrder: 1,
            expectedFirstNames: "John",
            expectedSurname: "DOE",
            expectedOrganisationName: "DOE");

        // 2) Jane SMITH
        AssertDefendant(
            defendants[1],
            expectedId: 2824598,
            expectedListOrder: 2,
            expectedFirstNames: "Jane",
            expectedSurname: "SMITH",
            expectedOrganisationName: "SMITH");

        // 3) Robert BROWN
        AssertDefendant(
            defendants[2],
            expectedId: 2824599,
            expectedListOrder: 3,
            expectedFirstNames: "Robert",
            expectedSurname: "BROWN",
            expectedOrganisationName: "BROWN");

        // 4) Emily CLARK
        AssertDefendant(
            defendants[3],
            expectedId: 2824600,
            expectedListOrder: 4,
            expectedFirstNames: "Emily",
            expectedSurname: "CLARK",
            expectedOrganisationName: "CLARK");

        // 5) Michael JOHNSON
        AssertDefendant(
            defendants[4],
            expectedId: 2824601,
            expectedListOrder: 5,
            expectedFirstNames: "Michael",
            expectedSurname: "JOHNSON",
            expectedOrganisationName: "JOHNSON");

        // ===== Pre-charge decision request deep validation =====
        var pcd = obj.PreChargeDecisionRequests.Single();

        Assert.Multiple(() =>
        {
            Assert.That(pcd.Id, Is.EqualTo(156619));
            Assert.That(pcd.VersionId, Is.EqualTo(0));

            Assert.That(pcd.DecisionRequiredBy, Is.EqualTo("2025-07-28"));
            Assert.That(pcd.DecisionRequested, Is.EqualTo("2025-07-18"));
            Assert.That(pcd.PresentationFlags, Is.Null);

            // Case outline
            Assert.That(pcd.CaseOutline, Is.Not.Null);
            Assert.That(pcd.CaseOutline.Count, Is.EqualTo(1));
            Assert.That(pcd.CaseOutline[0].Heading, Is.EqualTo("Summary of Key Evidence"));
            Assert.That(pcd.CaseOutline[0].Text, Does.Contain("This is a test message generated for the CPS Testing Tool system."));
            Assert.That(pcd.CaseOutline[0].TextWithCmsMarkup, Is.EqualTo(pcd.CaseOutline[0].Text));

            // Comments
            Assert.That(pcd.Comments, Is.Not.Null);
            Assert.That(pcd.Comments.Text, Is.Null);
            Assert.That(pcd.Comments.TextWithCmsMarkup, Is.Null);

            // Suspects
            Assert.That(pcd.Suspects, Is.Not.Null);
            Assert.That(pcd.Suspects.Count, Is.EqualTo(5));
        });

        // Validate suspects and their proposed charges (by surname makes it stable)
        var suspectsBySurname = pcd.Suspects.ToDictionary(s => s.Surname);

        AssertSuspect(suspectsBySurname["BROWN"], "Robert", location1: "4 Newton Road, Newton, Surrey, SM4 4DN, UK");
        AssertSuspect(suspectsBySurname["CLARK"], "Emily", location1: "5 Newton Road, Newton, Surrey, SM4 4DN, UK");
        AssertSuspect(suspectsBySurname["DOE"], "John", location1: "2 Newton Road, Newton, Surrey, SM4 4DN, UK");
        AssertSuspect(suspectsBySurname["JOHNSON"], "Michael", location1: "6 Newton Road, Newton, Surrey, SM4 4DN, UK");
        AssertSuspect(suspectsBySurname["SMITH"], "Jane", location1: "3 Newton Road, Newton, Surrey, SM4 4DN, UK");
    }


    [Test]
    public async Task GetCase_ShouldReturnBadRequest_WhenCaseUrnIsMissingOrInvalid()
    {
        // arrange
        var urn = "";
        var caseId = 12345;

        // act
        var result = await PolarisGatewayApiClient.GetCaseAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    [Test]
    public async Task GetCase_ShouldReturnInternalServerError_WhenDownstreamThrows()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private static void AssertDefendant(
        DefendantAndChargesDto defendant,
       int expectedId,
       int expectedListOrder,
       string expectedFirstNames,
       string expectedSurname,
       string expectedOrganisationName)
    {
        Assert.Multiple(() =>
        {
            Assert.That(defendant.Id, Is.EqualTo(expectedId));
            Assert.That(defendant.ListOrder, Is.EqualTo(expectedListOrder));

            Assert.That(defendant.DefendantDetails, Is.Not.Null);
            Assert.That(defendant.DefendantDetails.Id, Is.EqualTo(expectedId));
            Assert.That(defendant.DefendantDetails.ListOrder, Is.EqualTo(expectedListOrder));
            Assert.That(defendant.DefendantDetails.FirstNames, Is.EqualTo(expectedFirstNames));
            Assert.That(defendant.DefendantDetails.Surname, Is.EqualTo(expectedSurname));
            Assert.That(defendant.DefendantDetails.OrganisationName, Is.EqualTo(expectedOrganisationName));
            Assert.That(defendant.DefendantDetails.Dob, Is.Null);
            Assert.That(defendant.DefendantDetails.Age, Is.EqualTo(string.Empty));
            Assert.That(defendant.DefendantDetails.IsYouth, Is.False);
            Assert.That(defendant.DefendantDetails.Type, Is.EqualTo("Person"));

            // custody time limit
            Assert.That(defendant.CustodyTimeLimit, Is.Not.Null);
            Assert.That(defendant.CustodyTimeLimit.ExpiryDate, Is.Null);
            Assert.That(defendant.CustodyTimeLimit.ExpiryDays, Is.Null);
            Assert.That(defendant.CustodyTimeLimit.ExpiryIndicator, Is.Null);

            // charges (NYC placeholder)
            Assert.That(defendant.Charges, Is.Not.Null);
            Assert.That(defendant.Charges.Count, Is.EqualTo(1));

            var ch = defendant.Charges.FirstOrDefault();
            Assert.That(ch.IsCharged, Is.True);
            Assert.That(ch.NextHearingDate, Is.Null);
            Assert.That(ch.EarlyDate, Is.Null);
            Assert.That(ch.LateDate, Is.Null);
            Assert.That(ch.Code, Is.EqualTo("NYC"));
            Assert.That(ch.ShortDescription, Is.EqualTo("Not Yet Charged"));
            Assert.That(ch.LongDescription, Is.EqualTo("Not Yet Charged"));
            Assert.That(ch.CustodyTimeLimit, Is.Not.Null);
            Assert.That(ch.CustodyTimeLimit.ExpiryDate, Is.Null);
            Assert.That(ch.CustodyTimeLimit.ExpiryDays, Is.Null);
            Assert.That(ch.CustodyTimeLimit.ExpiryIndicator, Is.Null);

            // proposed charges (two items)
            Assert.That(defendant.ProposedCharges, Is.Not.Null);
            Assert.That(defendant.ProposedCharges.Count, Is.EqualTo(2));

            var proposed = defendant.ProposedCharges.ToList();
            Assert.That(proposed[0].Charge, Is.EqualTo("TH68027 Burglary other than dwelling with intent to steal"));
            Assert.That(proposed[0].EarlyDate, Is.EqualTo("2025-07-03"));
            Assert.That(proposed[0].LateDate, Is.EqualTo("2025-07-04"));

            Assert.That(proposed[1].Charge, Is.EqualTo("FA97031 Theft"));
            Assert.That(proposed[1].EarlyDate, Is.EqualTo("2025-07-03"));
            Assert.That(proposed[1].LateDate, Is.EqualTo("2025-07-04"));
        });
    }

    private static void AssertSuspect(PcdRequestSuspectDto suspect, string expectedFirstNames, string location1)
    {
        Assert.Multiple(() =>
        {
            Assert.That(suspect.FirstNames, Is.EqualTo(expectedFirstNames));
            Assert.That(suspect.Dob, Is.Null);
            Assert.That(suspect.BailConditions, Is.Null);
            Assert.That(suspect.BailDate, Is.Null);
            Assert.That(suspect.RemandStatus, Is.Null);

            Assert.That(suspect.ProposedCharges, Is.Not.Null);
            Assert.That(suspect.ProposedCharges.Count, Is.EqualTo(2));

            var charges = suspect.ProposedCharges.ToList();

            // 1) Burglary
            Assert.That(charges[0].Charge, Is.EqualTo("TH68027 Burglary other than dwelling with intent to steal"));
            Assert.That(charges[0].EarlyDate, Is.EqualTo("2025-07-03"));
            Assert.That(charges[0].LateDate, Is.EqualTo("2025-07-04"));
            Assert.That(NormalizeCmsText(charges[0].Location), Is.EqualTo(location1));
            Assert.That(charges[0].Category, Is.EqualTo("EW"));

            // 2) Theft
            Assert.That(charges[1].Charge, Is.EqualTo("FA97031 Theft"));
            Assert.That(charges[1].EarlyDate, Is.EqualTo("2025-07-03"));
            Assert.That(charges[1].LateDate, Is.EqualTo("2025-07-04"));
            Assert.That(NormalizeCmsText(charges[1].Location), Is.EqualTo(location1));
            Assert.That(charges[1].Category, Is.EqualTo("SUM"));
        });
    }
    protected static string NormalizeCmsText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .Replace('\u00A0', ' ')  // non-breaking space → space
            .Trim();
    }
}