using coordinator.Clients.PdfRedactor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Codeuctivity.ImageSharpCompare;
using Aspose.Pdf;
using Common.Streaming;

/* 
Aspose.PDF.Drawing 24.2.0 behaves differently when running unit tests compared to in production env
It loses JPEG2000 images when converting to pdf in xunit tests so we use a console app that hits the deployed function app to assert the redacted pdfs
ImageSharpCompare is then used to assert the AbsoluteError is 0 comparing to our known set of redacted pngs.
*/
namespace pdf_redactor.integration.tests
{
    using IPdfRedactorClient = Clients.IPdfRedactorClient;

    internal static class Program
    {
        public static async Task<int> Main()
        {
            var serviceProvider = BuildServiceProvider();

            StartupHelpers.SetAsposeLicence();


            var redactorClient = serviceProvider.GetRequiredService<IPdfRedactorClient>();

            await AssertRedactedPdf(redactorClient, "pdf_redactor.integration.tests.Resources.image_document_redactions.json", "pdf_redactor.integration.tests.Resources.image_document.pdf", "pdf_redactor.integration.tests.Resources.image_document_page_1.png", "pdf_redactor.integration.tests.Resources.image_document_page_2.png");
            await AssertRedactedPdf(redactorClient, "pdf_redactor.integration.tests.Resources.overlapping_redaction_redactions.json", "pdf_redactor.integration.tests.Resources.overlapping_redaction.pdf", "pdf_redactor.integration.tests.Resources.overlapping_redaction_page_1.png", null);
            await AssertRedactedPdf(redactorClient, "pdf_redactor.integration.tests.Resources.broken_ocr_redactions.json", "pdf_redactor.integration.tests.Resources.broken_ocr.pdf", "pdf_redactor.integration.tests.Resources.broken_ocr_page_1.png", null);

            Console.WriteLine("Successfully asserted all pdf test cases");
            return 0;
        }

        private static async Task AssertRedactedPdf(IPdfRedactorClient redactorClient, string redactionsResourceName, string pdfResourceName, string assertionOneResourceName, string? assertionTwoResourceName)
        {
            var redactionJsonStream = typeof(Program).Assembly.GetManifestResourceStream(redactionsResourceName) ?? throw new Exception($"{redactionsResourceName} not found");
            var documentStream = typeof(Program).Assembly.GetManifestResourceStream(pdfResourceName) ?? throw new Exception($"{pdfResourceName} not found");

            var redactionData = RedactionHelper.LoadRedactionDataForPdf(redactionJsonStream, documentStream, pdfResourceName);
            var redactedPdfStream = await redactorClient.RedactPdfAsync(redactionData);

            var fullStream = await redactedPdfStream.EnsureSeekableAsync();
            var redactedDocument = new Document(fullStream);

            var redactedImageStreams = await PdfConversionHelper.ConvertAndSavePdfToImages(redactedDocument);

            await using var assertionImageStreamOne = typeof(Program).Assembly.GetManifestResourceStream(assertionOneResourceName) ?? throw new ArgumentException($"{assertionOneResourceName} not found");
            var pageOneDiff = ImageSharpCompare.CalcDiff(redactedImageStreams[0], assertionImageStreamOne, ResizeOption.Resize);

            if (pageOneDiff.AbsoluteError > 0)
            {
                throw new Exception($"Mean ImageComparison diff for page 1 is {pageOneDiff.AbsoluteError} should be 0");
            }

            if (assertionTwoResourceName != null)
            {
                await using var assertionImageStreamTwo = typeof(Program).Assembly.GetManifestResourceStream(assertionTwoResourceName) ?? throw new ArgumentException($"{assertionTwoResourceName} not found");
                var pageTwoDiff = ImageSharpCompare.CalcDiff(redactedImageStreams[1], assertionImageStreamTwo, ResizeOption.Resize);

                if (pageTwoDiff.AbsoluteError > 0)
                {
                    throw new Exception($"Mean ImageComparison diff for page 2 is {pageTwoDiff.AbsoluteError} should be 0");
                }
            }
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            var redactorUrl = configuration.GetSection("Values")["PdfRedactorUrl"] ?? throw new ArgumentException("PdfRedactorUrl not found in configuration");

            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IRequestFactory, RequestFactory>();
            services.AddHttpClient<IPdfRedactorClient, Clients.PdfRedactorClient>(client =>
            {
                client.BaseAddress = new Uri(redactorUrl);
            });
            services.AddTransient<IRequestFactory, RequestFactory>();

            return services.BuildServiceProvider();
        }
    }
}
