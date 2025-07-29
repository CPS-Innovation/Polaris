using System.Net;
using Common.Dto.Request;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class AddDocumentNoteTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task AddDocumentNote_TextIsEmptyFailed_ShouldReturnBadRequest()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var text = string.Empty;
        var addDocumentNoteRequestDto = new AddDocumentNoteRequestDto()
        {
            Text = text
        };

        //act
        var result = await PolarisGatewayApiClient.AddDocumentNote(urn, caseId, documentId, addDocumentNoteRequestDto, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task AddDocumentNote_TextIs501Characters_ShouldReturnBadRequest()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var text = new string('*', 501);
        var addDocumentNoteRequestDto = new AddDocumentNoteRequestDto()
        {
            Text = text
        };

        //act
        var result = await PolarisGatewayApiClient.AddDocumentNote(urn, caseId, documentId, addDocumentNoteRequestDto, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task AddDocumentNote_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        var documentId = "CMS-12345";
        var text = new string('*', 10);
        var addDocumentNoteRequestDto = new AddDocumentNoteRequestDto()
        {
            Text = text
        };

        //act
        var result = await PolarisGatewayApiClient.AddDocumentNote(urn, caseId, documentId, addDocumentNoteRequestDto, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task AddDocumentNote_DocumentIdIs0_ShouldReturnBadRequest()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-0";
        var text = new string('*', 10);
        var addDocumentNoteRequestDto = new AddDocumentNoteRequestDto()
        {
            Text = text
        };

        //act
        var result = await PolarisGatewayApiClient.AddDocumentNote(urn, caseId, documentId, addDocumentNoteRequestDto, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task AddDocumentNote_ShouldReturnOk()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var text = new string('*', 10);
        var addDocumentNoteRequestDto = new AddDocumentNoteRequestDto()
        {
            Text = text
        };

        //act
        var result = await PolarisGatewayApiClient.AddDocumentNote(urn, caseId, documentId, addDocumentNoteRequestDto, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}