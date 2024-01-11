using pdf_generator.Services.PdfService;
using Common.Domain.Document;
using Microsoft.Extensions.Configuration;
using pdf_generator.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Services.DocumentRedaction;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using AppInsights = Microsoft.ApplicationInsights;

namespace pdf_generator.test_harness
{
  internal class Program
  {
    static void Main()
    {
      // Build configuration
      var configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("local.settings.json")
          .Build();

      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging(logging => logging.AddConsole());
      serviceCollection.AddSingleton<AppInsights.TelemetryClient>();
      serviceCollection.AddSingleton<ITelemetryClient, TelemetryClient>();

      serviceCollection.AddRedactionServices(configuration);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var redactionService = serviceProvider.GetRequiredService<IRedactionProvider>();

      Console.WriteLine("Enter the input file path:");
      string? filePath = Console.ReadLine();
      Console.WriteLine("Enter the output file path:");
      string? outputFilePath = Console.ReadLine() ?? throw new Exception("Output file path is required");

      if (File.Exists(filePath))
      {
        try
        {
          using var fileStream = File.OpenRead(filePath);

          Guid currentCorrelationId = default;
          var extension = Path.GetExtension(filePath).Replace(".", string.Empty).ToUpperInvariant();

          var fileType = Enum.Parse<FileType>(extension);

          RedactPdfRequestDto redactPdf = new RedactPdfRequestDto
          {
            FileName = filePath,
            CaseId = 1234,
            VersionId = 1,
            RedactionDefinitions = new List<RedactionDefinitionDto>
              {
                new RedactionDefinitionDto
                {
                  PageIndex = 1,
                  Width = 1473.15,
                  Height = 1041,
                  RedactionCoordinates = new List<RedactionCoordinatesDto>
                  {
                    new RedactionCoordinatesDto
                    {
                      X1 = 228.5,
                      Y1 = 441.5,
                      X2 = 1175.71,
                      Y2 = 1151.71
                    }
                  }
                }
              }
          };

          Console.WriteLine(redactPdf.RedactionDefinitions[0].Width);
          Console.WriteLine(redactPdf.RedactionDefinitions[0].Height);

          var pdfStream = redactionService.Redact(fileStream, redactPdf, currentCorrelationId);

          // Write the PDF stream to the file system
          byte[] pdfBytes;
          using (MemoryStream ms = new MemoryStream())
          {
            pdfStream.CopyTo(ms);
            pdfBytes = ms.ToArray();
          }

          File.WriteAllBytes(outputFilePath, pdfBytes);

          Console.WriteLine("PDF conversion successful.");
        }
        catch (Exception e)
        {
          Console.WriteLine("PDF conversion failed.");
          Console.WriteLine(e.Message);
        }
      }
      else
      {
        throw new Exception("File does not exist, check path");
      }
    }
  }
}