using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using coordinator.Clients.PdfRedactor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;

namespace pdf_redactor.integration.tests
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
            builder.Configuration.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);

            builder.Services.AddHttpClient(
              "testClient",
              client =>
              {
                  client.BaseAddress = new Uri(builder.Configuration["RedactorUrl"]);
              });

            builder.Services.AddTransient<IRequestFactory, RequestFactory>();

            StartupHelpers.SetAsposeLicence();

            using var host = builder.Build();
            using var serviceScope = host.Services.CreateScope();

            // call http function for redaction
            using var redactedPdf = await RedactPdfUsingFunctionCall(serviceScope.ServiceProvider, builder.Configuration["RedactorKey"]);


            // convert to images

            // compare stream from call to local assertion file
        }

        private static async Task<Stream> RedactPdfUsingFunctionCall(IServiceProvider serviceProvider, string redactorKey)
        {
            Guid currentCorrelationId = Guid.NewGuid();

            var requestFactory = serviceProvider.GetRequiredService<IRequestFactory>();
            var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("testClient");

            // get pdf coordinates from dto
            var redactionData = LoadRedactionData("pdf_redactor.integration.tests.redactionData.json", "pdf_redactor.integration.tests.document.pdf");

            var requestMessage = new StringContent(JsonSerializer.Serialize(redactionData), Encoding.UTF8, "application/json");

            var redactRequest = requestFactory.Create(HttpMethod.Put, $"{RestApi.GetPdfRedactorPath("pdf-redactor.integration.tests.urn", "pdf-redactor.integration.tests.caseId", "pdf-redactor.integration.tests.documentId")}?code={redactorKey}", currentCorrelationId);
            redactRequest.Content = requestMessage;

            using var pdfStream = new MemoryStream();
            using (var response = await client.SendAsync(redactRequest, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await response.Content.CopyToAsync(pdfStream);
                pdfStream.Seek(0, SeekOrigin.Begin);
            }

            return pdfStream;
        }

        private static RedactPdfRequestWithDocumentDto LoadRedactionData(string jsonPath, string documentPath)
        {
            using var redactedStream = typeof(Program).Assembly.GetManifestResourceStream(jsonPath) ?? throw new Exception($"{jsonPath} not found");
            using var streamReader = new StreamReader(redactedStream);
            var jsonText = streamReader.ReadToEnd();
            var redactionData = JsonSerializer.Deserialize<RedactionData>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("Failed to deserialize redaction data");

            var redactionDefinitions = new List<RedactionDefinitionDto>();

            foreach (var redaction in redactionData.Redactions)
            {
                var redactionDefinition = new RedactionDefinitionDto
                {
                    PageIndex = redaction.PageIndex,
                    Width = redaction.Width,
                    Height = redaction.Height,
                    RedactionCoordinates = redaction.RedactionCoordinates.Select(rc => new RedactionCoordinatesDto
                    {
                        X1 = rc.X1,
                        Y1 = rc.Y1,
                        X2 = rc.X2,
                        Y2 = rc.Y2
                    }).ToList()
                };

                redactionDefinitions.Add(redactionDefinition);
            }

            var base64Document = Convert.ToBase64String(File.ReadAllBytes(documentPath));

            return new RedactPdfRequestWithDocumentDto
            {
                //FileName = fileName,
                Document = base64Document,
                VersionId = 1,
                RedactionDefinitions = redactionDefinitions
            };
        }
    }
}
