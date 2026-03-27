using System.Net;
using Common.Enums;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetPcdReviewCoreTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetPcdReviewCore_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/pcd-review
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdReviewCore_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldReturnOk_WithPcdReviewCoreResponse()
    {
        // arrange
        // Endpoint: GET {{url}}/urns/54KR7689125/cases/2179140/pcd-review
        // Test data from MDS Client API contains:
        // - ID 4472751: "PCD Analysis" (type: 2) - should be marked as EarlyAdvice
        // - ID 4472765: "Initial Review" (type: 1)
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldReturnOK_AndValidateResponseIsCollection()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        // Should return 2 items: PCD Analysis and Initial Review
        Assert.That(items.Count, Is.EqualTo(2),
            "Should return 2 PCD review items (PCD Analysis with Early Advice and Initial Review)");
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldReturnOK_AndValidateResponseHasRequiredProperties()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        Assert.That(items.Count, Is.GreaterThan(0));

        foreach (var item in items)
        {
            Assert.Multiple(() =>
            {
                // Validate required properties
                Assert.That(item.Id, Is.Not.Null.And.Not.Empty, "Id should not be null or empty");
                Assert.That(item.Date, Is.Not.Null.And.Not.Empty, "Date should not be null or empty");
                Assert.That(item.Type, Is.Not.Null, "Type should not be null");

                // Validate Type enum values
                Assert.That(item.Type, Is.AnyOf(
                    PcdReviewCoreType.EarlyAdvice,
                    PcdReviewCoreType.InitialReview,
                    PcdReviewCoreType.PreChargeDecisionAnalysis),
                    $"Type should be one of the valid enum values");
            });
        }
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldReturnCorrectItemsWithExpectedIds()
    {
        // arrange
        // Based on MDS Client API data:
        // - ID 4472751: "PCD Analysis" (type: 2) - First PCD event, should become EarlyAdvice
        // - ID 4472765: "Initial Review" (type: 1)
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();
        Assert.That(items.Count, Is.EqualTo(2), "Should have 2 PCD review items");

        // Validate first item: PCD Analysis marked as EarlyAdvice (ID: 4472751)
        Assert.Multiple(() =>
        {
            var firstItem = items[0];
            Assert.That(firstItem.Id, Is.EqualTo("4472751"), "First item should be PCD Analysis with ID 4472751");
            Assert.That(firstItem.Type, Is.EqualTo(PcdReviewCoreType.EarlyAdvice),
                "PCD Analysis should be marked as EarlyAdvice when it appears before Initial Review");
            Assert.That(firstItem.Date, Is.EqualTo("27/01/2026"), "Date should be 27/01/2026");
        });

        // Validate second item: Initial Review (ID: 4472765)
        Assert.Multiple(() =>
        {
            var secondItem = items[1];
            Assert.That(secondItem.Id, Is.EqualTo("4472765"), "Second item should be Initial Review with ID 4472765");
            Assert.That(secondItem.Type, Is.EqualTo(PcdReviewCoreType.InitialReview),
                "Second item should be InitialReview type");
            Assert.That(secondItem.Date, Is.EqualTo("27/01/2026"), "Date should be 27/01/2026");
        });
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldReturnItemsOrderedById()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        if (items.Count > 1)
        {
            // Verify items are ordered by ID in ascending order
            for (int i = 0; i < items.Count - 1; i++)
            {
                var currentId = int.Parse(items[i].Id);
                var nextId = int.Parse(items[i + 1].Id);
                Assert.That(currentId, Is.LessThanOrEqualTo(nextId),
                    $"Items should be ordered by ID. Item {i} has ID {currentId} and item {i + 1} has ID {nextId}");
            }
        }
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldCorrectlyDetectEarlyAdvice()
    {
        // arrange
        // Early Advice logic: If PCD Analysis (type 2) appears before Initial Review (type 1),
        // the PCD Analysis should be marked as EarlyAdvice
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        // Verify Early Advice detection logic
        var earlyAdviceItem = items.FirstOrDefault(i => i.Type == PcdReviewCoreType.EarlyAdvice);

        if (earlyAdviceItem != null)
        {
            // If EarlyAdvice is present, verify it's ordered correctly
            var earlyAdviceIndex = items.IndexOf(earlyAdviceItem);
            var initialReviewItem = items.FirstOrDefault(i => i.Type == PcdReviewCoreType.InitialReview);

            if (initialReviewItem != null)
            {
                var initialReviewIndex = items.IndexOf(initialReviewItem);
                Assert.That(earlyAdviceIndex, Is.LessThan(initialReviewIndex),
                    "EarlyAdvice should appear before InitialReview in the collection");
            }
        }
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldHaveValidDateFormat()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        foreach (var item in items)
        {
            // Validate date format (dd/mm/yyyy)
            Assert.That(item.Date, Does.Match(@"^\d{2}/\d{2}/\d{4}$"),
                $"Date '{item.Date}' should be in format dd/mm/yyyy");

            // Validate date is the expected value from test data
            Assert.That(item.Date, Is.EqualTo("27/01/2026"),
                "All test data items should have date 27/01/2026");
        }
    }

    [Test]
    public async Task GetPcdReviewCore_InvalidCaseId_ShouldReturnUnprocessableEntityOrNotFound()
    {
        // arrange
        var urn = "54KR7689125";
        var invalidCaseId = 9999999;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            invalidCaseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, 
            Is.EqualTo(HttpStatusCode.UnprocessableEntity)
            .Or.EqualTo(HttpStatusCode.OK)
            .Or.EqualTo(HttpStatusCode.NotFound),
            "Invalid case should return UnprocessableEntity, OK (empty), or NotFound");
    }

    [Test]
    public async Task GetPcdReviewCore_ShouldReturnOnlyPreChargeDecisionRelatedEvents()
    {
        // arrange
        // MDS API returns 13 events, but GetPcdReviewCore should filter to only return:
        // - PreChargeDecisionAnalysis events (type 2, name "PCD Analysis")
        // - InitialReview events (type 1, name "Initial Review")
        // Filtering out: Registration, PCD Request, PCD Triage, Pre-charge Decision, Case Status Change, 
        // PCD Response, MG3, etc.
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        // Only EarlyAdvice, InitialReview, and PreChargeDecisionAnalysis types should be present
        foreach (var item in items)
        {
            Assert.That(item.Type, Is.AnyOf(
                PcdReviewCoreType.EarlyAdvice,
                PcdReviewCoreType.InitialReview,
                PcdReviewCoreType.PreChargeDecisionAnalysis),
                $"Item with ID {item.Id} has unexpected type {item.Type}. Only PCD-related events should be returned");
        }

        // Verify count matches expected filtered items (PCD Analysis and Initial Review)
        Assert.That(items.Count, Is.EqualTo(2),
            "Should return exactly 2 items after filtering for PCD-related events only");
    }

    [Test]
    public async Task GetPcdReviewCore_AllItemsShouldHaveDateFrom27January2026()
    {
        // arrange
        // All test data items have date "27/01/2026"
        var urn = "54KR7689125";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetPcdReviewCoreAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();

        Assert.Multiple(() =>
        {
            foreach (var item in items)
            {
                Assert.That(item.Date, Is.EqualTo("27/01/2026"),
                    $"Item {item.Id} should have date from test data (27/01/2026)");
            }
        });
    }
}

