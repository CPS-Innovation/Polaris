using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Aspose.Pdf;
using coordinator.Clients.PdfRedactor;
using Codeuctivity.ImageSharpCompare;
using Common.Streaming;
using pdf_redactor.integration.tests.Helpers;

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
        private static string _pdfRedactorUrl = string.Empty;

        public static async Task<int> Main()
        {
            var serviceProvider = BuildServiceProvider();

            StartupHelpers.SetAsposeLicence();

            var redactorClient = serviceProvider.GetRequiredService<IPdfRedactorClient>();


            Console.WriteLine($"Running tests against PDF Redactor: '{_pdfRedactorUrl}'");
            Console.WriteLine("Asserting Document Image Redactions - Started");
            await AssertRedactedPdf(redactorClient, "pdf_redactor.integration.tests.Resources.image_document_redactions.json", "pdf_redactor.integration.tests.Resources.image_document.pdf", "pdf_redactor.integration.tests.Resources.image_document_page_1.png", "pdf_redactor.integration.tests.Resources.image_document_page_2.png");
            Console.WriteLine("Asserting Document Image Redactions - Completed");
            Console.WriteLine("Asserting Overlapping Redactions - Started");
            await AssertRedactedPdf(redactorClient, "pdf_redactor.integration.tests.Resources.overlapping_redaction_redactions.json", "pdf_redactor.integration.tests.Resources.overlapping_redaction.pdf", "pdf_redactor.integration.tests.Resources.overlapping_redaction_page_1.png", null);
            Console.WriteLine("Asserting Overlapping Redactions - Completed");
            Console.WriteLine("Asserting Broken OCR Redactions - Started");
            await AssertRedactedPdf(redactorClient, "pdf_redactor.integration.tests.Resources.broken_ocr_redactions.json", "pdf_redactor.integration.tests.Resources.broken_ocr.pdf", "pdf_redactor.integration.tests.Resources.broken_ocr_page_1.png", null);
            Console.WriteLine("Asserting Broken OCR Redactions - Completed");
            Console.WriteLine("Asserting Single Page Removal - Started");
            await AssertPagesRemovedFromPdf(redactorClient, "pdf_redactor.integration.tests.Resources.page_removal.pdf", new int[] { 2 }, "pdf_redactor.integration.tests.Resources.page_removal_page_1.png", "pdf_redactor.integration.tests.Resources.page_removal_page_3.png");
            Console.WriteLine("Asserting Single Page Removal - Completed");

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

        private static async Task AssertPagesRemovedFromPdf(IPdfRedactorClient redactorClient, string pdfResourceName, int[] pageIndexesToRemove, string assertionOneResourceName, string? assertionTwoResourceName = null)
        {
            var documentStream = typeof(Program).Assembly.GetManifestResourceStream(pdfResourceName) ?? throw new Exception($"{pdfResourceName} not found");
            var originalDocument = new Document(documentStream);

            var pageIndexData = DocumentManipulationHelper.LoadPageRemovalDataForPdf(documentStream, pdfResourceName, pageIndexesToRemove);
            var manipulatedDocumentStream = await redactorClient.RemoveDocumentPagesAsync(pageIndexData);

            var fullStream = await manipulatedDocumentStream.EnsureSeekableAsync();
            var manipulatedDocument = new Document(fullStream);

            if (manipulatedDocument.Pages.Count == originalDocument.Pages.Count)
            {
                throw new Exception("The page count of the manipulated document equals the original after page removal");
            }

            // var fullpath = typeof(Program).Assembly.Location;

            // using (var fileStream = File.Create($"{Path.GetDirectoryName(fullpath)}/test.pdf"))
            // {
            //     fullStream.Seek(0, SeekOrigin.Begin);
            //     fullStream.CopyTo(fileStream);
            // }

            // var pdfImageStreams = await PdfConversionHelper.ConvertAndSavePdfToImages(manipulatedDocument);

            // await using var assertionImageStreamOne = typeof(Program).Assembly.GetManifestResourceStream(assertionOneResourceName) ?? throw new ArgumentException($"{assertionOneResourceName} not found");
            // var pageOneDiff = ImageSharpCompare.CalcDiff(pdfImageStreams[0], assertionImageStreamOne, ResizeOption.Resize);

            // if (pageOneDiff.AbsoluteError > 0)
            // {
            //     throw new Exception($"Mean ImageComparison diff for page 1 is {pageOneDiff.AbsoluteError} should be 0");
            // }

            // if (assertionTwoResourceName != null)
            // {
            //     await using var assertionImageStreamTwo = typeof(Program).Assembly.GetManifestResourceStream(assertionTwoResourceName) ?? throw new ArgumentException($"{assertionTwoResourceName} not found");
            //     var pageTwoDiff = ImageSharpCompare.CalcDiff(pdfImageStreams[1], assertionImageStreamTwo, ResizeOption.Resize);

            //     if (pageTwoDiff.AbsoluteError > 0)
            //     {
            //         throw new Exception($"Mean ImageComparison diff for page 2 is {pageTwoDiff.AbsoluteError} should be 0");
            //     }
            // }
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            _pdfRedactorUrl = configuration["PdfRedactorUrl"] ?? throw new ArgumentException("PdfRedactorUrl not found in configuration");

            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IRequestFactory, RequestFactory>();
            services.AddHttpClient<IPdfRedactorClient, Clients.PdfRedactorClient>(client =>
            {
                client.BaseAddress = new Uri(_pdfRedactorUrl);
            });
            services.AddTransient<IRequestFactory, RequestFactory>();

            return services.BuildServiceProvider();
        }
    }
}
