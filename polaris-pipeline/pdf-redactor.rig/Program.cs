using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Common.Streaming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using coordinator.Clients.PdfRedactor;

namespace pdf_redactor.rig
{
    using IPdfRedactorClient = Clients.IPdfRedactorClient;

    internal static class Program
    {
        private static string _pdfRedactorUrl = string.Empty;
        private static string _csvFilePath = "/Users/rhysbridges/Documents/CPS/Polaris/polaris-pipeline/pdf-redactor.rig/output.csv";
        private static string _folderPath = "/Users/rhysbridges/Documents/CPS/Polaris/polaris-pipeline/pdf-redactor.rig/Resources";
        public static async Task<int> Main()
        {
            var serviceProvider = BuildServiceProvider();

            var redactorClient = serviceProvider.GetRequiredService<IPdfRedactorClient>();

            Console.WriteLine($"Running redactions against PDF Redactor: '{_pdfRedactorUrl}'");

            await RedactPdfs(redactorClient);
            Console.WriteLine("Successfully redacted all pdf test cases");
            return 0;
        }

        private static async Task RedactPdfs(IPdfRedactorClient redactorClient)
        {

            var pdfFiles = Directory.GetFiles(_folderPath, "*.pdf");
            pdfFiles = pdfFiles.Where(file => Path.GetFileName(file).Count(c => c == '_') == 1).ToArray();

            using (var writer = new StreamWriter(_csvFilePath))
            {
                await writer.WriteLineAsync($"File Name,Redaction Type,Size Before (mb),Size After (mb),Time Taken (milliseconds)");

                foreach (var pdfFile in pdfFiles)
                {
                    foreach (RedactionType redactionType in Enum.GetValues(typeof(RedactionType)))
                    {
                        Console.WriteLine($"Redacting {pdfFile} with {redactionType} method");

                        try
                        {
                            var documentStream = File.OpenRead(pdfFile);
                            var document = new Document(documentStream);

                            var sizeBefore = documentStream.Length / (1024.0 * 1024.0);

                            var redactionData = RedactionHelper.LoadRedactionDataForPdf(document, documentStream, documentStream.Name);
                            redactionData.RedactionType = redactionType;

                            var startTime = DateTime.Now;

                            var redactedPdfStream = await redactorClient.RedactPdfAsync(redactionData);

                            var endTime = DateTime.Now;
                            var timeTaken = (endTime - startTime).TotalMilliseconds;

                            var fullStream = await redactedPdfStream.EnsureSeekableAsync();

                            // var outputFilePath = pdfFile.Replace(".pdf", $"_{redactionType}.pdf");
                            // using (var fileStream = File.Create(outputFilePath))
                            // {
                            //     fullStream.Seek(0, SeekOrigin.Begin);
                            //     fullStream.CopyTo(fileStream);
                            // }

                            var fileName = Path.GetFileName(pdfFile);
                            var sizeAfter = fullStream.Length / (1024.0 * 1024.0);

                            // Write to CSV
                            await writer.WriteLineAsync($"{fileName},{redactionType},{sizeBefore},{sizeAfter},{timeTaken}");

                            // Close the documentStream
                            documentStream.Close();

                            Console.WriteLine($"Finished Redacting {pdfFile} with {redactionType} method");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error redacting {pdfFile} with {redactionType} method: {e.Message}");
                        }
                    }
                }
            }
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            StartupHelpers.SetAsposeLicence();

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
