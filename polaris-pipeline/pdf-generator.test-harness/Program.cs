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
    static void Main(string[] args)
    {
      SetAsposeLicence();

      // Build configuration
      var configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("local.settings.json")
          .Build();

      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging(logging => logging.AddConsole());
      serviceCollection.AddSingleton<AppInsights.TelemetryClient>();
      serviceCollection.AddSingleton<ITelemetryClient, TelemetryClient>();

      serviceCollection.AddPdfGenerator(configuration);
      serviceCollection.AddRedactionServices(configuration);

      var serviceProvider = serviceCollection.BuildServiceProvider();


      string mode = args[0];

      Enum.TryParse(mode, out Mode modeEnum);
      switch (modeEnum)
      {
        case Mode.RedactPdf:
          RedactPdfFile(serviceProvider);
          break;
        case Mode.ConvertToPdf:
          ConvertFileToPdf(serviceProvider);
          break;
        default:
          throw new Exception("Unknown mode");
      }
    }
    private static void SetAsposeLicence()
    {
      try
      {
        var licenceFileName = "Aspose.Total.NET.lic";
        new Aspose.Cells.License().SetLicense(licenceFileName);
        new Aspose.Diagram.License().SetLicense(licenceFileName);
        new Aspose.Email.License().SetLicense(licenceFileName);
        new Aspose.Imaging.License().SetLicense(licenceFileName);
        new Aspose.Pdf.License().SetLicense(licenceFileName);
        new Aspose.Slides.License().SetLicense(licenceFileName);
        new Aspose.Words.License().SetLicense(licenceFileName);
      }
      catch (Exception exception)
      {
        throw new Exception(exception.Message);
      }
    }

    private static void RedactPdfFile(ServiceProvider serviceProvider)
    {
      var redactionService = serviceProvider.GetRequiredService<IRedactionProvider>();

      Console.WriteLine("Enter the input file path:");
      string? filePath = Console.ReadLine();
      Console.WriteLine("Enter the output file path:");
      string? outputFilePath = Console.ReadLine() ?? throw new Exception("Output file path is required");

      Console.WriteLine("Enter the number of pages to redact:");
      if (!int.TryParse(Console.ReadLine(), out int numberOfPagesToRedact) || numberOfPagesToRedact <= 0)
      {
        Console.WriteLine("Invalid input for the number of pages. Exiting.");
        return;
      }


      if (File.Exists(filePath))
      {
        try
        {
          using var fileStream = File.OpenRead(filePath);

          Guid currentCorrelationId = default;
          var extension = Path.GetExtension(filePath).Replace(".", string.Empty).ToUpperInvariant();

          var fileType = Enum.Parse<FileType>(extension);

          var redactionDefinitions = new List<RedactionDefinitionDto>();
          for (int pageIndex = 1; pageIndex <= numberOfPagesToRedact; pageIndex++)
          {
            redactionDefinitions.Add(new RedactionDefinitionDto
            {
              PageIndex = pageIndex,
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
            });
          }

          RedactPdfRequestDto redactPdf = new RedactPdfRequestDto
          {
            FileName = filePath,
            CaseId = 1234,
            VersionId = 1,
            RedactionDefinitions = redactionDefinitions
          };

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
          Console.WriteLine($"PDF conversion failed: {e.Message}");
        }
      }
      else
      {
        throw new Exception("File does not exist, check path");
      }
    }

    private static void ConvertFileToPdf(ServiceProvider serviceProvider)
    {
      var orchestratorService = serviceProvider.GetRequiredService<IPdfOrchestratorService>();

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
          var documentId = "test-doc-1";

          var fileType = Enum.Parse<FileType>(extension);

          var pdfStream = orchestratorService.ReadToPdfStream(fileStream, fileType, documentId, currentCorrelationId);

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
          Console.WriteLine($"PDF conversion failed: {e.Message}");
        }
      }
      else
      {
        throw new Exception("File does not exist, check path");
      }
    }
  }
}