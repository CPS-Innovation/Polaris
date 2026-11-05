using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseExhibitProducersTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseExhibitProducers_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseExhibitProducersAsync(
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseExhibitProducers_ShouldReturnOkWithResponseDto()
    {
        // arrange
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseExhibitProducersAsync(
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
        Assert.That(result.ResponseObject!.ExhibitProducers, Is.Not.Null);
    }

    [Test]
    public async Task GetCaseExhibitProducers_ShouldReturnOK_AndValidateNoDuplicateProducerIds()
    {
        // arrange
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseExhibitProducersAsync(
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;
        Assert.That(obj.ExhibitProducers, Is.Not.Null);

        var producers = obj.ExhibitProducers!.ToList();

        var duplicateIds = producers
            .GroupBy(p => p.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicateIds, Is.Empty, "Expected ExhibitProducers to contain unique producer Id values.");
    }

    [Test]
    public async Task GetCaseExhibitProducers_ShouldReturnOK_AndValidateWitnessProducersHaveNames_WhenPresent()
    {
        // arrange
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseExhibitProducersAsync(
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;
        Assert.That(obj.ExhibitProducers, Is.Not.Null);

        // Function appends witnesses as producers with IsWitnessProducer=true
        var witnessProducers = obj.ExhibitProducers!
            .Where(p => p.IsWitness)
            .ToList();

        if (witnessProducers.Any())
        {
            Assert.That(
                witnessProducers.All(p => !string.IsNullOrWhiteSpace(p.Name)),
                Is.True,
                "Expected witness producers to have a non-empty Name.");
        }
        else
        {
            Assert.Pass("No witness producers returned for this dataset/caseId; skipping name validation.");
        }
    }
}