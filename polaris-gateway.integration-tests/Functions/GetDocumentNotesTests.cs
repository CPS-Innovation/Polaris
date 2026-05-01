using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetDocumentNotesTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);

    }

    [Test]
    public async Task GetDocumentNotes_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        var materialId = "CMS-12345";

        //act
        var result = await PolarisGatewayApiClient.GetDocumentNotesAsync(urn, caseId, materialId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetDocumentNotes_MaterialIdIs0_ShouldReturnBadRequest()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var materialId = "CMS-0";

        //act
        var result = await PolarisGatewayApiClient.GetDocumentNotesAsync(urn, caseId, materialId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetDocumentNotes_ShouldReturnOkAndDocumentNoteList()
    {
        //arrange
        var urn = "54KR7689125";
        var caseId = 2160797;
        var materialId = "CMS-8880088";

        //act
        var result = await PolarisGatewayApiClient.GetDocumentNotesAsync(urn, caseId, materialId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject.Count(), Is.AtLeast(3));
        Assert.That(result.ResponseObject.ToList()[0].Id, Is.EqualTo(int.Parse(materialId.Split('-')[1])));
    }
}