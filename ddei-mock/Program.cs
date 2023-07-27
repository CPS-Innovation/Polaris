using AutoFixture;
using Ddei.Domain;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

int port;
if (args.Length == 0 || !int.TryParse(args[0], out port))
    port = 7071;

var server = WireMockServer.Start(port);
Console.WriteLine("WireMockServer running at {0}", string.Join(",", server.Ports));

var fixture = new Fixture();

MockTestCases(server, fixture, 100);
MockIssue22734(server, fixture);
MockCheckoutDocument(server);

Console.WriteLine("Press any key to stop the server");
Console.ReadKey();

Console.WriteLine("Displaying all requests");
var allRequests = server.LogEntries;
Console.WriteLine(JsonConvert.SerializeObject(allRequests, Formatting.Indented));

Console.WriteLine("Press any key to quit");
Console.ReadKey();

static void MockTestCases(WireMockServer server, Fixture fixture, int count)
{
    for (var i = 1; i <= count; i++)
    {
        var num = i.ToString("000");
        var urn = $"TST{num}";
        var id = int.Parse($"{num}001");

        // GET Case IDs by URN
        var cases = fixture.Build<DdeiCaseIdentifiersDto>()
            .With(x => x.Id, id)
            .With(x => x.Urn, urn)
            .CreateMany(1);

        MockGetJson(server, $"/api/urns/{urn}/cases", cases);

        // GET Case by Case ID
        var caseSummary = fixture.Build<DdeiCaseSummaryDto>()
            .With(x => x.Id, id)
            .With(x => x.Urn, urn)
            .Create();
        var @case = fixture.Build<DdeiCaseDetailsDto>()
            .With(x => x.Summary, caseSummary)
            .Create();
        @case.Summary.NumberOfDefendants = @case.Defendants.Count();

        MockGetJson(server, $"/api/urns/{urn}/cases/{id}", @case);  
    }
}

// Issue 22734, Pipeline corrupts PDF coming back from CMS after a redaction
// https://dev.azure.com/CPSDTS/Information%20Management/_sprints/taskboard/Polaris%20-%20Private%20to%20Public%20Beta/Information%20Management/Polaris%20-%20Release%202/Sprint%202?workitem=22734
static void MockIssue22734(WireMockServer server, Fixture fixture)
{
    var caseId = 22734;
    var urn = $"ISS{caseId}";

    // GET Case IDs by URN
    var cases = fixture.Build<DdeiCaseIdentifiersDto>()
        .With(x => x.Id, caseId)
        .With(x => x.Urn, urn)
        .CreateMany(1);

    MockGetJson(server, $"/api/urns/{urn}/cases", cases);

    // GET Case by Case ID
    var caseSummary = fixture.Build<DdeiCaseSummaryDto>()
        .With(x => x.Id, caseId)
        .With(x => x.Urn, urn)
        .Create();
    var @case = fixture.Build<DdeiCaseDetailsDto>()
        .With(x => x.Summary, caseSummary)
        .Create();
    @case.Summary.NumberOfDefendants = @case.Defendants.Count();

    MockGetJson(server, $"/api/urns/{urn}/cases/{caseId}", @case);

    var category = DdeiCmsDocCategory.UsedStatement;
    var documentId = 2273401;

    // GET Document(s) List by Case ID
    var documents = fixture.Build<DdeiDocumentDetailsDto>()
        .With(x => x.Id, documentId)
        .With(x => x.VersionId, 22734011)
        .With(x => x.OriginalFileName, "Document 1")
        .With(x => x.MimeType, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
        .With(x => x.CmsDocCategory, category)
        .CreateMany(1);

    MockGetJson(server, $"/api/urns/{urn}/cases/{caseId}/documents", documents);

    // GET Document by Document ID
    MockGetFile(server, $"/api/urns/{urn}/cases/{caseId}/documents/{category}/{documentId}", "1437428004_PreConvertToPdf-GeneratePdf_2145688-pdfs-CMS-8666648.pdf");
}

static void MockGetJson(WireMockServer server, string path, object result)
{
    var request = Request.Create()
        .WithPath(path)
        .UsingGet();
    var body = JsonConvert.SerializeObject(result);
    var response = Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(body);

    server
        .Given(request)
        .RespondWith(response);
}

static void MockGetFile(WireMockServer server, string path, string filename)
{
    var request = Request.Create()
        .WithPath(path)
        .UsingGet();
    var response = Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/pdf")
        .WithBodyFromFile(filename);

    server
        .Given(request)
        .RespondWith(response);
}

static void MockCheckoutDocument(WireMockServer server)
{
    var request = Request.Create()
         .WithPath($"/api/urns/*/cases/*/documents/*/*/*/checkout")
         .UsingPost();
    var response = Response.Create()
        .WithStatusCode(200);

    server
        .Given(request)
        .RespondWith(response);
}


