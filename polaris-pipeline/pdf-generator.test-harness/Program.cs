using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Telemetry;
using Common.Telemetry.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdf_generator.Services.DocumentRedaction;
using pdf_generator.Services.Extensions;
using pdf_generator.Services.PdfService;
using AppInsights = Microsoft.ApplicationInsights;

namespace pdf_generator.test_harness;

internal static class Program
{
  public static async Task Main(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);

    SetAsposeLicence();

    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
    builder.Configuration.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);

    builder.Services.AddSingleton<AppInsights.TelemetryClient>();
    builder.Services.AddSingleton<ITelemetryClient, TelemetryClient>();

    builder.Services.AddPdfGenerator();
    builder.Services.AddRedactionServices(builder.Configuration);
    builder.Services.AddHttpClient(
      "testClient",
      client =>
      {
        client.BaseAddress = new Uri("http://localhost:7073/api/");
      });
    builder.Services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
    using var host = builder.Build();

    var mode = args[0];

    if (!Enum.TryParse(mode, out Mode modeEnum))
      throw new Exception("Unknown mode");
      
    using var serviceScope = host.Services.CreateScope();
    switch (modeEnum)
    {
      case Mode.LibraryCallRedactPdf:
        RedactPdfFile(serviceScope.ServiceProvider);
        break;
      case Mode.LibraryCallConvertToPdf:
        ConvertFileToPdf(serviceScope.ServiceProvider);
        break;
      case Mode.FunctionCallConvertToPdf:
        await ConvertFileToPdfUsingFunctionCall(serviceScope.ServiceProvider);
        break;
      default:
        throw new Exception("Unknown mode");
    }

    return;

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
      var filePath = Console.ReadLine();
      Console.WriteLine("Enter the output file path:");
      var outputFilePath = Console.ReadLine() ?? throw new Exception("Output file path is required");

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
          var redactionDefinitions = new List<RedactionDefinitionDto>();
          for (var pageIndex = 1; pageIndex <= numberOfPagesToRedact; pageIndex++)
          {
            redactionDefinitions.Add(new RedactionDefinitionDto
            {
              PageIndex = pageIndex,
              Width = 842,
              Height = 595,
              RedactionCoordinates =
              [
                new RedactionCoordinatesDto
                {
                  X1 = 228.5,
                  Y1 = 241.5,
                  X2 = 475.71,
                  Y2 = 441.5
                }
              ]
            });
          }

          var redactPdf = new RedactPdfRequestDto
          {
            FileName = filePath,
            CaseId = 1234,
            VersionId = 1,
            RedactionDefinitions = redactionDefinitions
          };

          var pdfStream = redactionService.Redact(fileStream, redactPdf, currentCorrelationId);

          // Write the PDF stream to the file system
          byte[] pdfBytes;
          using (var ms = new MemoryStream())
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
      var filePath = Console.ReadLine();
      Console.WriteLine("Enter the output file path:");
      var outputFilePath = Console.ReadLine() ?? throw new Exception("Output file path is required");

      if (File.Exists(filePath))
      {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        PdfManager.BeginConversion(filePath, orchestratorService, outputFilePath);
        watch.Stop();
        Console.WriteLine($"Conversion time: {watch.ElapsedMilliseconds} ms");
      }
      else
      {
        throw new Exception("File does not exist, check path");
      }
    }
    
    static async Task ConvertFileToPdfUsingFunctionCall(IServiceProvider serviceProvider)
    {
      var pipelineClientRequestFactory = serviceProvider.GetRequiredService<IPipelineClientRequestFactory>();
      var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
      
      Console.WriteLine("Enter the input file path:");
      var filePath = Console.ReadLine();
      Console.WriteLine("Enter the output file path:");
      var outputFilePath = Console.ReadLine() ?? throw new Exception("Output file path is required");

      if (File.Exists(filePath))
      {
        try
        {
          await using var fileStream = File.OpenRead(filePath);

          var extension = Path.GetExtension(filePath).Replace(".", string.Empty).ToUpperInvariant();

          var fileType = Enum.Parse<FileType>(extension);
          
          var request = pipelineClientRequestFactory.Create(HttpMethod.Post, $"test-convert-to-pdf", Guid.NewGuid());
          request.Headers.Add(HttpHeaderKeys.Filetype, fileType.ToString());
          request.Headers.Add(HttpHeaderKeys.DocumentId, "test-document-id");
          request.Headers.Add(HttpHeaderKeys.VersionId, "test-version-id");
          request.Headers.Add(HttpHeaderKeys.CaseId, "test-case-id");

          using (var requestContent = new StreamContent(fileStream))
          {
            request.Content = requestContent;

            using var client = httpClientFactory.CreateClient("testClient");
            using var pdfStream = new MemoryStream();
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
              response.EnsureSuccessStatusCode();
              await response.Content.CopyToAsync(pdfStream);
              pdfStream.Seek(0, SeekOrigin.Begin);
            }
            
            // Write the PDF stream to the file system
            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
              await pdfStream.CopyToAsync(ms);
              pdfBytes = ms.ToArray();
            }

            await File.WriteAllBytesAsync(outputFilePath, pdfBytes);
          }

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

internal static class PdfManager
{
  internal static void BeginConversion(string filePath, IPdfOrchestratorService orchestratorService, string outputFilePath)
  {
    try
    {
      using var fileStream = File.OpenRead(filePath);

      Guid currentCorrelationId = default;
      var extension = Path.GetExtension(filePath).Replace(".", string.Empty).ToUpperInvariant();
      const string documentId = "test-doc-1";

      var fileType = Enum.Parse<FileType>(extension);

      var conversionResult = orchestratorService.ReadToPdfStream(fileStream, fileType, documentId, currentCorrelationId);

      if (conversionResult.ConversionStatus == PdfConversionStatus.DocumentConverted)
      {
        // Write the PDF stream to the file system
        byte[] pdfBytes;
        using (var ms = new MemoryStream())
        {
          conversionResult.ConvertedDocument.CopyTo(ms);
          pdfBytes = ms.ToArray();
        }

        File.WriteAllBytes(outputFilePath, pdfBytes);

        Console.WriteLine("PDF conversion successful.");
      }
      else
      {
        Console.WriteLine($"PDF conversion Failed - Status: {conversionResult.ConversionStatus.GetEnumValue()}, Feedback: {conversionResult.Feedback}");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine($"PDF conversion failed: {e.Message}");
    }
  }
}