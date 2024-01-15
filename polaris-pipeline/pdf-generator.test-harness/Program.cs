using Common.Domain.Document;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using pdf_generator.Services.DocumentRedaction;
using pdf_generator.Services.Extensions;
using pdf_generator.Services.PdfService;
using AppInsights = Microsoft.ApplicationInsights;

var builder = Host.CreateApplicationBuilder(args);

SetAsposeLicence();

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);

//builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddSingleton<AppInsights.TelemetryClient>();
builder.Services.AddSingleton<ITelemetryClient, TelemetryClient>();

builder.Services.AddPdfGenerator(builder.Configuration);
builder.Services.AddRedactionServices(builder.Configuration);
using var host = builder.Build();

var mode = args[0];

Enum.TryParse(mode, out Mode modeEnum);
using var serviceScope = host.Services.CreateScope();
switch (modeEnum)
{
  case Mode.RedactPdf:
    RedactPdfFile(serviceScope.ServiceProvider);
    break;
  case Mode.ConvertToPdf:
    ConvertFileToPdf(serviceScope.ServiceProvider);
    break;
  default:
    throw new Exception("Unknown mode");
}

static void SetAsposeLicence()
{
    try
    {
        const string licenceFileName = "Aspose.Total.NET.lic";
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

static void RedactPdfFile(IServiceProvider serviceProvider)
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
          Width = 842,
          Height = 595,
          RedactionCoordinates = new List<RedactionCoordinatesDto>
            {
                new RedactionCoordinatesDto
                {
                    X1 = 228.5,
                    Y1 = 241.5,
                    X2 = 475.71,
                    Y2 = 441.5
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

static void ConvertFileToPdf(IServiceProvider serviceProvider)
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