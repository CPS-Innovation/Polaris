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

MockTestCases(server, fixture);
MockCheckoutDocument(server);

Console.WriteLine("Press any key to stop the server");
Console.ReadKey();

Console.WriteLine("Displaying all requests");
var allRequests = server.LogEntries;
Console.WriteLine(JsonConvert.SerializeObject(allRequests, Formatting.Indented));

Console.WriteLine("Press any key to quit");
Console.ReadKey();

static void MockTestCases(WireMockServer server, Fixture fixture)
{
    var urns = Directory
                 .EnumerateDirectories("urns", "*")
                 .Select(x => x.Replace("urns\\", string.Empty))
                 .ToList();

    foreach (var urn in urns)
    {
        var cases = Directory
                        .EnumerateDirectories($"urns\\{urn}\\cases", "*")
                        .Select(x => x.Replace($"urns\\{urn}\\cases\\", string.Empty))
                        .Select(x => int.Parse(x))
                        .ToList();

        MockCases(server, fixture, urn, cases);

        foreach (var @case in cases)
        {
            MockCase(server, fixture, urn, @case);

            var categories = Directory
                                .EnumerateDirectories($"urns\\{urn}\\cases\\{@case}\\documents", "*")
                                .Select(x => x.Replace($"urns\\{urn}\\cases\\{@case}\\documents\\", string.Empty))
                                .Select(x => Enum.Parse<DdeiCmsDocCategory>(x))
                                .ToList();

            var documentIds = new Dictionary<DdeiCmsDocCategory, List<int>>();

            foreach (var category in categories)
            {
                var categoryDocumentIds = Directory
                                    .EnumerateDirectories($"urns\\{urn}\\cases\\{@case}\\documents\\{category}", "*")
                                    .Select(x => x.Replace($"urns\\{urn}\\cases\\{@case}\\documents\\{category}\\", string.Empty))
                                    .Select(x => int.Parse(x))
                                    .ToList();

                documentIds[category] = categoryDocumentIds;
            }

             MockDocuments(server, fixture, urn, @case, documentIds);
        }
    }
}

static void MockCases(WireMockServer server, Fixture fixture, string urn, List<int> cases)
{
    var casesDto = fixture.Build<DdeiCaseIdentifiersDto>()
        .With(x => x.Urn, urn)
        .CreateMany(cases.Count);

    for(int i=0; i < cases.Count; i++)
    {
        casesDto.ElementAt(i).Id = cases[i];
    }

    MockGetJson(server, $"/api/urns/{urn}/cases", casesDto);
}

static void MockCase(WireMockServer server, Fixture fixture, string urn, int @case)
{
    var caseSummaryDto = fixture.Build<DdeiCaseSummaryDto>()
        .With(x => x.Id, @case)
        .With(x => x.Urn, urn)
        .Create();

    var caseDto = fixture.Build<DdeiCaseDetailsDto>()
        .With(x => x.Summary, caseSummaryDto)
        .Create();

    caseDto.Summary.NumberOfDefendants = caseDto.Defendants.Count();

    MockGetJson(server, $"/api/urns/{urn}/cases/{@case}", caseDto);
}

static void MockDocuments(WireMockServer server, Fixture fixture, string urn, int @case, Dictionary<DdeiCmsDocCategory, List<int>> documentIds)
{
    int documentsCount = documentIds.Values.Sum(list => list.Count);

    var documentsDto = fixture.Build<DdeiDocumentDetailsDto>()
        .CreateMany(documentsCount);

    for(int i=0; i < documentIds.Keys.Count; i++)
    {
        var category = documentIds.Keys.ElementAt(i);

        for(int j=0; j < documentIds[category].Count; j++)
        {
            var documentDto = documentsDto.ElementAt(j);
            var documentId = documentIds[category][j];

            documentDto.Id = documentId;
            documentDto.VersionId = int.Parse((documentId % 1000000) + (j+1).ToString("00"));
            documentDto.CmsDocCategory = category;

            // 1 file per directory - dir name has ID, directory has file
            var filename = Directory
                            .EnumerateFiles($"urns\\{urn}\\cases\\{@case}\\documents\\{category}\\{documentId}", "*")
                            .Select(x => x.Replace($"urns\\{urn}\\cases\\{@case}\\documents\\{category}\\{documentId}\\", string.Empty))
                            .First();

            documentDto.OriginalFileName = Path.GetFileNameWithoutExtension(filename);
            // TODO
            documentDto.MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            MockDocument(server, fixture, urn, @case, category, documentId, $"urns\\{urn}\\cases\\{@case}\\documents\\{category}\\{documentId}\\{filename}");
        }
    }

    MockGetJson(server, $"/api/urns/{urn}/cases/{@case}/documents", documentsDto);
}

static void MockDocument(WireMockServer server, Fixture fixture, string urn, int @case, DdeiCmsDocCategory category, int documentId, string filename)
{
    MockGetFile(server, $"/api/urns/{urn}/cases/{@case}/documents/{category}/{documentId}", filename);
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