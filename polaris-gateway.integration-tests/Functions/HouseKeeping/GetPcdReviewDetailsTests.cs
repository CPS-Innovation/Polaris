using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetPcdReviewDetailsTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetPcdReviewDetails_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/history/{historyId}/pcd-review-details
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdReviewDetails_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnOk_WithPcdReviewDetailResponse()
    {
        // arrange
        // Endpoint: GET {{url}}/urns/54KR7689125/cases/2179140/history/4472765/pcd-review-details
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnOK_AndValidatePreChargeDecisionAnalysisOutcomeNotNull()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        Assert.Multiple(() =>
        {
            Assert.That(obj.PreChargeDecisionAnalysisOutcome, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnOK_AndValidatePreChargeDecisionAnalysisOutcomeData()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;

        // validate pcd analysis outcome
        Assert.That(obj.PreChargeDecisionAnalysisOutcome, Is.Not.Null);

        var pcdAnalysis = obj.PreChargeDecisionAnalysisOutcome!;
        Assert.Multiple(() =>
        {
            // Core identifiers and basic properties
            Assert.That(pcdAnalysis.CaseId, Is.EqualTo(2179140));
            Assert.That(pcdAnalysis.Id, Is.EqualTo(4472765));
            Assert.That(pcdAnalysis.HistoryEventType, Is.EqualTo(1));

            // Text fields - PCD Analysis Outcome
            Assert.That(pcdAnalysis.Allocation, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.CaseSummary, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.DisclosureActionsAndIssues, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.EuropeanCourtOfHumanRights, Is.EqualTo("Human rights factors are not an issue in this case at this time"));
            Assert.That(pcdAnalysis.EvidentialAssessment, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.InstructionsToOperationsDeliveryOrAdvocate, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.PublicInterestAssessment, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.TrialStrategy, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.WitnessOrVictimInformationAndActions, Is.EqualTo("test test testing test test 2"));
            Assert.That(pcdAnalysis.ConsultationType, Is.EqualTo("Full Code Test"));
            Assert.That(pcdAnalysis.ReviewSummary, Is.EqualTo("PCD Case Analysis"));
            Assert.That(pcdAnalysis.ProsecutorDeclaration, Is.EqualTo("Not Applicable"));

            // Boolean and status fields
            Assert.That(pcdAnalysis.IsCompleted, Is.True);
            Assert.That(pcdAnalysis.EventDate, Is.EqualTo("27/01/2026"));
            Assert.That(pcdAnalysis.DgSummary, Is.EqualTo("Yes"));

            // Null values
            Assert.That(pcdAnalysis.DppConsent, Is.Null);
        });
    }

    [Test]
    public async Task GetPcdReviewDetails_InvalidHistoryId_ShouldReturnUnprocessableEntity()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var invalidHistoryId = 9999999;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            invalidHistoryId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity).Or.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnMonitoringCodes()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var pcdAnalysis = result.ResponseObject!.PreChargeDecisionAnalysisOutcome!;
        Assert.That(pcdAnalysis.MonitoringCodes, Is.Not.Null);
        Assert.That(pcdAnalysis.MonitoringCodes, Has.Count.EqualTo(3));

        var monitoringCodes = pcdAnalysis.MonitoringCodes!.ToList();

        // Validate first monitoring code - Asset Recovery
        Assert.Multiple(() =>
        {
            var assetRecovery = monitoringCodes[0];
            Assert.That(assetRecovery.Code, Is.Null);
            Assert.That(assetRecovery.Description, Is.EqualTo("Asset Recovery"));
            Assert.That(assetRecovery.Type, Is.Null);
            Assert.That(assetRecovery.Disabled, Is.False);
            Assert.That(assetRecovery.IsAssigned, Is.True);
        });

        // Validate second monitoring code - Child Abuse
        Assert.Multiple(() =>
        {
            var childAbuse = monitoringCodes[1];
            Assert.That(childAbuse.Code, Is.Null);
            Assert.That(childAbuse.Description, Is.EqualTo("Child Abuse"));
            Assert.That(childAbuse.Type, Is.Null);
            Assert.That(childAbuse.Disabled, Is.False);
            Assert.That(childAbuse.IsAssigned, Is.True);
        });

        // Validate third monitoring code - Pre-Charge Decision
        Assert.Multiple(() =>
        {
            var preChargeDecision = monitoringCodes[2];
            Assert.That(preChargeDecision.Code, Is.Null);
            Assert.That(preChargeDecision.Description, Is.EqualTo("Pre-Charge Decision"));
            Assert.That(preChargeDecision.Type, Is.Null);
            Assert.That(preChargeDecision.Disabled, Is.False);
            Assert.That(preChargeDecision.IsAssigned, Is.True);
        });
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnLinkedCaseUrns()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var pcdAnalysis = result.ResponseObject!.PreChargeDecisionAnalysisOutcome!;
        Assert.That(pcdAnalysis.LinkedCaseUrns, Is.Not.Null);
        Assert.That(pcdAnalysis.LinkedCaseUrns, Has.Count.GreaterThan(350)); // 384+ entries expected

        var linkedCaseUrns = pcdAnalysis.LinkedCaseUrns!.ToList();

        // Validate specific linked case URNs from the response
        var expectedUrns = new[]
        {
            "16XL0500226", "12NY1670008", "12NY1001926", "12NY1000125", "12NY0602426",
            "12NY0602326", "12NY0602226", "12NY0602126", "12NY0602026", "12NY0601926"
        };

        Assert.Multiple(() =>
        {
            // Verify expected URNs are present
            foreach (var expectedUrn in expectedUrns)
            {
                var linkedUrn = linkedCaseUrns.FirstOrDefault(x => x.Urn == expectedUrn);
                Assert.That(linkedUrn, Is.Not.Null, $"Expected URN {expectedUrn} not found");
                Assert.That(linkedUrn!.Asn, Is.EqualTo("Yes"));
                Assert.That(linkedUrn.PncId, Is.Null);
                Assert.That(linkedUrn.PoliceCC, Is.Null);
            }

            // Validate first entry structure
            var firstUrn = linkedCaseUrns[0];
            Assert.That(firstUrn.Urn, Is.Not.Null.Or.Empty);
            Assert.That(firstUrn.Asn, Is.EqualTo("Yes"));
        });
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnDgDetailsAndAssessment()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var pcdAnalysis = result.ResponseObject!.PreChargeDecisionAnalysisOutcome!;
        var dgDetails = pcdAnalysis.DgDetails;

        Assert.Multiple(() =>
        {
            Assert.That(dgDetails, Is.Not.Null);
            Assert.That(dgDetails!.AssessmentApplicable, Is.True);
            Assert.That(dgDetails.DatePapersReceived, Is.Null);
            Assert.That(dgDetails.PrincipalOffenceCategory, Is.EqualTo("Not applicable"));
            Assert.That(dgDetails.StageAssessmentCompleted, Is.EqualTo("PCD"));
            Assert.That(dgDetails.SubmissionDgCompliant, Is.EqualTo("Yes"));
            Assert.That(dgDetails.PoliceResponse, Is.Null);
            Assert.That(dgDetails.DgAssessmentItems, Is.Not.Null);
            Assert.That(dgDetails.DgAssessmentItems, Has.Count.EqualTo(4));
        });

        // Validate each DG Assessment Item
        var assessmentItems = dgDetails!.DgAssessmentItems!.ToList();
        for (int i = 0; i < 4; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(assessmentItems[i].ItemName, Is.Null);
                Assert.That(assessmentItems[i].Title, Is.Null);
                Assert.That(assessmentItems[i].Description, Is.Null);
                Assert.That(assessmentItems[i].Comment, Is.Null);
            });
        }
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnCurrentEventAndNextEventLink()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var pcdAnalysis = result.ResponseObject!.PreChargeDecisionAnalysisOutcome!;

        // Validate current event
        Assert.That(pcdAnalysis.CurrentEvent, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(pcdAnalysis.CurrentEvent!.Id, Is.EqualTo(4472765));
            Assert.That(pcdAnalysis.CurrentEvent.Name, Is.EqualTo("Initial Review"));
            Assert.That(pcdAnalysis.CurrentEvent.Date, Is.EqualTo("27/01/2026"));
            Assert.That(pcdAnalysis.CurrentEvent.AuthorOrVenue, Is.EqualTo("Rachael McKeown"));
            Assert.That(pcdAnalysis.CurrentEvent.Type, Is.EqualTo(1));
        });

        // Validate next event link
        Assert.That(pcdAnalysis.NextEventLink, Is.Not.Null);
        Assert.That(pcdAnalysis.NextEventLink, Has.Count.GreaterThanOrEqualTo(1));

        var nextEventLink = pcdAnalysis.NextEventLink!.First();
        Assert.Multiple(() =>
        {
            Assert.That(nextEventLink.Id, Is.EqualTo(4472765));
            Assert.That(nextEventLink.Href, Is.Not.Null.Or.Empty);
            Assert.That(nextEventLink.Rel, Is.EqualTo("InitialReview"));
            Assert.That(nextEventLink.Type, Is.EqualTo("GET"));
        });
    }

    [Test]
    public async Task GetPcdReviewDetails_ShouldReturnPreChargeDecisionOutcomeWithDefendantDecisions()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var historyId = 4472765;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewDetailsAsync(
            urn,
            caseId,
            historyId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var pcdOutcome = result.ResponseObject!.PreChargeDecisionOutcome!;

        // Validate core properties
        Assert.Multiple(() =>
        {
            Assert.That(pcdOutcome.Author, Is.EqualTo("Rachael McKeown"));
            Assert.That(pcdOutcome.CaseId, Is.EqualTo(2179140));
            Assert.That(pcdOutcome.DecisionRequestedDate, Is.EqualTo("27/01/2026"));
            Assert.That(pcdOutcome.DecisionMadeDateTime, Is.EqualTo("27/01/2026 14:06"));
            Assert.That(pcdOutcome.EventDate, Is.EqualTo("27/01/2026"));
            Assert.That(pcdOutcome.Id, Is.EqualTo(4472772));
            Assert.That(pcdOutcome.IsCompleted, Is.True);
            Assert.That(pcdOutcome.HistoryEventType, Is.EqualTo(3));
            Assert.That(pcdOutcome.InvestigationStage, Is.EqualTo("Bail for Charging Decision"));
            Assert.That(pcdOutcome.Method, Is.EqualTo("Area"));
            Assert.That(pcdOutcome.DecisionMadeBy, Is.EqualTo("Rachael McKeown"));
            Assert.That(pcdOutcome.ActionPlan, Is.True);
            Assert.That(pcdOutcome.PoliceCovidUrgency, Is.EqualTo("Other"));
            Assert.That(pcdOutcome.CpsCovidUrgency, Is.EqualTo("Other"));
            Assert.That(pcdOutcome.Urn, Is.EqualTo("16XL8836126"));
        });

        // Validate defendant decisions
        Assert.That(pcdOutcome.DefendantDecisions, Is.Not.Null);
        Assert.That(pcdOutcome.DefendantDecisions, Has.Count.EqualTo(5));

        var defendantDecisions = pcdOutcome.DefendantDecisions!.ToList();

        // Expected defendant names
        var expectedDefendants = new[] { "DOE John", "SMITH Jane", "BROWN Robert", "CLARK Emily", "JOHNSON Michael" };

        Assert.Multiple(() =>
        {
            for (int i = 0; i < defendantDecisions.Count; i++)
            {
                var decision = defendantDecisions[i];
                Assert.That(decision.DefendantName, Is.EqualTo(expectedDefendants[i]));
                Assert.That(decision.DecisionDescription, Is.EqualTo("H - Request Further Evidence to Complete Evidential Report"));
                Assert.That(decision.Decision, Is.EqualTo(10));
                Assert.That(decision.Reason, Is.EqualTo("H - Request Further Evidence to Complete Evidential Report"));
                Assert.That(decision.ReasonCode, Is.EqualTo("H"));
                Assert.That(decision.ProposedCharge, Is.EqualTo("H - Request Further Evidence to Complete Evidential Report"));
                Assert.That(decision.KeyFactor, Is.Null);
                Assert.That(decision.NatureOfDecision, Is.Null);
                Assert.That(decision.PublicInterestCode, Is.Null);
                Assert.That(decision.SpecifiedCharges, Is.Null);
                Assert.That(decision.ReturnBailDate, Is.Null);
                Assert.That(decision.PcdPrincipalOffenceCategory, Is.Null);
                Assert.That(decision.ChargeDetails, Is.Not.Null.And.Empty);
            }
        });

        // Validate action plan
        Assert.That(pcdOutcome.PcdHistoryActionPlan, Is.Not.Null);
        Assert.That(pcdOutcome.PcdHistoryActionPlan, Has.Count.EqualTo(1));

        var actionPlan = pcdOutcome.PcdHistoryActionPlan!.First();
        Assert.Multiple(() =>
        {
            Assert.That(actionPlan.ActionType, Is.EqualTo("Action Plan"));
            Assert.That(actionPlan.EntryDate, Is.EqualTo("27/01/2026"));
            Assert.That(actionPlan.ActionDate, Is.EqualTo("03/02/2026"));
            Assert.That(actionPlan.Suspect, Is.EqualTo("CLARK Emily"));
            Assert.That(actionPlan.Status, Is.EqualTo("Sent on PCD Response"));
            Assert.That(actionPlan.ActionPoint, Is.Not.Null.Or.Empty);
            Assert.That(actionPlan.PoliceCovidUrgency, Is.Null);
            Assert.That(actionPlan.CpsCovidUrgency, Is.Null);
        });

        // Validate current event
        Assert.That(pcdOutcome.CurrentEvent, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(pcdOutcome.CurrentEvent!.Id, Is.EqualTo(4472772));
            Assert.That(pcdOutcome.CurrentEvent.Name, Is.EqualTo("Pre-charge Decision"));
            Assert.That(pcdOutcome.CurrentEvent.Date, Is.EqualTo("27/01/2026"));
            Assert.That(pcdOutcome.CurrentEvent.AuthorOrVenue, Is.EqualTo("Rachael McKeown"));
            Assert.That(pcdOutcome.CurrentEvent.Type, Is.EqualTo(3));
        });

        // Validate next event links
        Assert.That(pcdOutcome.NextEventLink, Is.Not.Null);
        Assert.That(pcdOutcome.NextEventLink, Has.Count.EqualTo(2));

        var nextEventLinks = pcdOutcome.NextEventLink!.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(nextEventLinks[0].Id, Is.EqualTo(4472772));
            Assert.That(nextEventLinks[0].Rel, Is.EqualTo("PreChargeDecision"));
            Assert.That(nextEventLinks[0].Type, Is.EqualTo("GET"));

            Assert.That(nextEventLinks[1].Id, Is.EqualTo(4472752));
            Assert.That(nextEventLinks[1].Rel, Is.EqualTo("PreChargeDecision"));
            Assert.That(nextEventLinks[1].Type, Is.EqualTo("GET"));
        });
    }
}
