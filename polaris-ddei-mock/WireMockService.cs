#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoFixture;
using Common.Extensions;
using Common.Dto.Response;
using Ddei.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WireMock.Admin.Requests;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using static Common.Extensions.AssemblyExtensions;

namespace WireMock.Net.WebApplication;

public class WireMockService : IWireMockService
{
    private WireMockServer? _server;
    private readonly ILogger _logger;
    private readonly WireMockServerSettings _settings;

    private class Logger : IWireMockLogger
    {
        private readonly ILogger _logger;

        public Logger(ILogger logger)
        {
            _logger = logger;
        }

        public void Debug(string formatString, params object[] args)
        {
            _logger.LogDebug(formatString, args);
        }

        public void Info(string formatString, params object[] args)
        {
            _logger.LogInformation(formatString, args);
        }

        public void Warn(string formatString, params object[] args)
        {
            _logger.LogWarning(formatString, args);
        }

        public void Error(string formatString, params object[] args)
        {
            _logger.LogError(formatString, args);
        }

        public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminrequest)
        {
            string message = JsonConvert.SerializeObject(logEntryModel, Formatting.Indented);
            _logger.LogDebug("Admin[{0}] {1}", isAdminrequest, message);
        }

        public void Error(string formatString, Exception exception)
        {
            _logger.LogError(formatString, exception.Message);
        }
    }

    public WireMockService(ILogger<WireMockService> logger, IOptions<WireMockServerSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _settings.Logger = new Logger(logger);
    }

    public void Start()
    {
        _logger.LogInformation("WireMock.Net server starting");

        _server = WireMockServer.Start(_settings);

        var fixture = new Fixture();

        var dirSeparator = Path.DirectorySeparatorChar;

        MockStatus(_server);
        MockCheckoutDocument(_server);
        MockTestCases(_server, fixture, dirSeparator);
        MockDirectoryCases(_server, fixture, dirSeparator);

        _logger.LogInformation($"WireMock.Net server settings {JsonConvert.SerializeObject(_settings)}");
    }

    public void Stop()
    {
        _logger.LogInformation("WireMock.Net server stopping");
        _server?.Stop();
    }

    static void MockStatus(WireMockServer server)
    {
        var request = Request.Create()
            .WithPath("/api/status")
            .UsingGet();

        var response = Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(JsonConvert.SerializeObject(Assembly.GetExecutingAssembly().CurrentStatus()));

        server
            .Given(request)
            .RespondWith(response);
    }

    static void MockValue(WireMockServer server, string value)
    {
        var request = Request.Create()
            .WithPath("/api/value")
            .UsingGet();

        var response = Response.Create()
            .WithStatusCode(200)
            .WithBody(value);

        server
            .Given(request)
            .RespondWith(response);
    }

    static void MockTestCases(WireMockServer server, Fixture fixture, char separator)
    {
        try
        {
            var testCases = new List<TestCase>
            { 
                // Basic scaling test cases
                new TestCase{ Urn = "TEST001", CaseId = 1, DocumentCount = 1 },
                new TestCase{ Urn = "TEST002", CaseId = 2, DocumentCount = 2 },
                new TestCase{ Urn = "TEST005", CaseId = 5, DocumentCount = 5 },
                new TestCase{ Urn = "TEST010", CaseId = 10, DocumentCount = 10 },
                new TestCase{ Urn = "TEST020", CaseId = 20, DocumentCount = 20 },
                new TestCase{ Urn = "TEST050", CaseId = 50, DocumentCount = 50 },
                new TestCase{ Urn = "TEST100", CaseId = 100, DocumentCount = 100 },
                new TestCase{ Urn = "TEST200", CaseId = 200, DocumentCount = 200 },
                new TestCase{ Urn = "TEST500", CaseId = 500, DocumentCount = 500 },
                new TestCase{ Urn = "TEST1000", CaseId = 1000, DocumentCount = 1000 },
                new TestCase{ Urn = "TEST2000", CaseId = 2000, DocumentCount = 2000 },
                new TestCase{ Urn = "TEST5000", CaseId = 5000, DocumentCount = 5000 },
                new TestCase{ Urn = "TEST10000", CaseId = 10000, DocumentCount = 10000 },

                // 10 Test cases with 100 documents, varying URNs for concurrency test
                new TestCase{ Urn = "TEST100-001", CaseId = 101, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-002", CaseId = 102, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-003", CaseId = 103, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-004", CaseId = 104, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-005", CaseId = 105, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-006", CaseId = 106, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-007", CaseId = 107, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-008", CaseId = 108, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-009", CaseId = 109, DocumentCount = 100 },
                new TestCase{ Urn = "TEST100-010", CaseId = 110, DocumentCount = 100 },
            };
            foreach (var testCase in testCases)
            {
                MockCases(server, fixture, testCase.Urn, new List<int> { testCase.CaseId });
                MockCase(server, fixture, testCase.Urn, testCase.CaseId);

                var category = DdeiCmsDocCategory.Review;
                var documentIds = new Dictionary<DdeiCmsDocCategory, List<int>>();
                var categoryDocumentIds = Enumerable.Range(1, testCase.DocumentCount).ToList();
                documentIds[category] = categoryDocumentIds;

                var documentsDto = fixture.Build<DdeiCaseDocumentResponse>()
                    .CreateMany(testCase.DocumentCount);

                for (var i = 0; i < testCase.DocumentCount; i++)
                {
                    var documentId = categoryDocumentIds[i];
                    MockGetFile(server, $"urns/{testCase.Urn}/cases/{testCase.CaseId}/documents/{category}/{documentId}", $"Documents{separator}TestDocument.pdf");

                    var documentDto = documentsDto.ElementAt(i);

                    documentDto.Id = documentId;
                    documentDto.VersionId = int.Parse((documentId % 1000000) + (i + 1).ToString("00"));
                    documentDto.CmsDocCategory = category.ToString();

                    // 1 file per directory - dir name has ID, directory has file
                    var filename = "TestDocument.pdf";

                    documentDto.OriginalFileName = Path.GetFileNameWithoutExtension(filename);
                    documentDto.FileExtension = Path.GetExtension(filename).Replace(".", string.Empty);
                    documentDto.Path = $"/{documentDto.CmsDocCategory}/{documentDto.OriginalFileName}.{documentDto.FileExtension}";
                    // TODO
                    documentDto.MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                    MockDocument(server, documentDto.CmsDocCategory, $"{documentDto.OriginalFileName}.{documentDto.FileExtension}");
                }

                MockGetJson(server, $"/api/urns/{testCase.Urn}/cases/{testCase.CaseId}/documents", documentsDto);
            }
        }
        catch (Exception e)
        {
            MockValue(server, e.ToString());
            return;
        }
    }

    static void MockDirectoryCases(WireMockServer server, Fixture fixture, char separator)
    {
        try
        {
            var urns = Directory
                         .EnumerateDirectories("urns", "*")
                         .Select(x => x.Replace($"urns{separator}", string.Empty))
                         .ToList();

            foreach (var urn in urns)
            {
                var cases = Directory
                                .EnumerateDirectories($"urns{separator}{urn}{separator}cases", "*")
                                .Select(x => x.Replace($"urns{separator}{urn}{separator}cases{separator}", string.Empty))
                                .Select(x => int.Parse(x))
                                .ToList();

                var value = string.Join(".", cases.Select(x => x.ToString()));

                MockCases(server, fixture, urn, cases);

                foreach (var @case in cases)
                {
                    MockCase(server, fixture, urn, @case);

                    var categories = Directory
                                        .EnumerateDirectories($"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents", "*")
                                        .Select(x => x.Replace($"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents{separator}", string.Empty))
                                        .Select(x => Enum.Parse<DdeiCmsDocCategory>(x))
                                        .ToList();

                    var documentIds = new Dictionary<DdeiCmsDocCategory, List<int>>();

                    foreach (var category in categories)
                    {
                        var categoryDocumentIds = Directory
                                            .EnumerateDirectories($"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents{separator}{category}", "*")
                                            .Select(x => x.Replace($"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents{separator}{category}{separator}", string.Empty))
                                            .Select(x => int.Parse(x))
                                            .ToList();

                        documentIds[category] = categoryDocumentIds;
                    }

                    MockDocuments(server, fixture, urn, @case, documentIds, separator);
                }
            }
        }
        catch (Exception e)
        {
            MockValue(server, e.ToString());
            return;
        }
    }

    static void MockCases(WireMockServer server, Fixture fixture, string urn, List<int> cases)
    {
        var casesDto = fixture.Build<DdeiCaseIdentifiersDto>()
            .With(x => x.Urn, urn)
            .CreateMany(cases.Count);

        for (int i = 0; i < cases.Count; i++)
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

    static void MockDocuments(WireMockServer server, Fixture fixture, string urn, int @case, Dictionary<DdeiCmsDocCategory, List<int>> documentIds, char separator)
    {
        int documentsCount = documentIds.Values.Sum(list => list.Count);

        var documentsDto = fixture.Build<DdeiCaseDocumentResponse>()
            .CreateMany(documentsCount)
            .ToList();

        for (int i = 0; i < documentIds.Keys.Count; i++)
        {
            var category = documentIds.Keys.ElementAt(i);

            for (int j = 0; j < documentIds[category].Count; j++)
            {
                var documentDto = documentsDto.ElementAt(j);
                var documentId = documentIds[category][j];

                documentDto.Id = documentId;
                documentDto.VersionId = int.Parse((documentId % 1000000) + (j + 1).ToString("00"));
                documentDto.CmsDocCategory = category.ToString();

                // 1 file per directory - dir name has ID, directory has file
                var filename = Directory
                                .EnumerateFiles($"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents{separator}{category}{separator}{documentId}", "*")
                                .Select(x => x.Replace($"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents{separator}{category}{separator}{documentId}{separator}", string.Empty))
                                .First();

                documentDto.OriginalFileName = Path.GetFileNameWithoutExtension(filename);
                documentDto.FileExtension = Path.GetExtension(filename).Replace(".", string.Empty);
                documentDto.Path = $"/{documentDto.CmsDocCategory}/{documentDto.OriginalFileName}.{documentDto.FileExtension}";
                // TODO
                documentDto.MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                MockDocument(server, urn, @case, category, documentId, $"urns{separator}{urn}{separator}cases{separator}{@case}{separator}documents{separator}{category}{separator}{documentId}{separator}{filename}");
                MockDocument(server, documentDto.CmsDocCategory, documentDto.OriginalFileName);
            }
        }

        MockGetJson(server, $"/api/urns/{urn}/cases/{@case}/documents", documentsDto);
    }

    static void MockDocument(WireMockServer server, string urn, int @case, DdeiCmsDocCategory category, int documentId, string filename)
    {
        MockGetFile(server, $"urns/{urn}/cases/{@case}/documents/{category}/{documentId}", filename);
    }

    static void MockDocument(WireMockServer server, string path, string filename)
    {
        MockGetFileStore(server, path, filename);
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
            .WithPath($"/api/{path}")
            .UsingGet();
        var response = Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/pdf")
            .WithBodyFromFile($"{filename}");
        server
            .Given(request)
            .RespondWith(response);
    }

    static void MockGetFileStore(WireMockServer server, string path, string filename)
    {
        var request = Request.Create()
            .WithPath($"/api/file-store/{path}/{filename}")
            .UsingGet();
        var response = Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/pdf")
            .WithBodyFromFile($"Documents\\{filename}");
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
}