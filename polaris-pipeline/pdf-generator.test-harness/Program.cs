﻿using Common.Domain.Document;
using pdf_generator.Extensions;
using pdf_redactor.Services.Extensions;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdf_redactor.Services.DocumentRedaction;
using pdf_generator.Services.Extensions;
using pdf_generator.Services.PdfService;
using AppInsights = Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Common.Clients.PdfGenerator;
using Common.Constants;

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
    builder.Services.AddTransient<IPdfGeneratorRequestFactory, PdfGeneratorRequestFactory>();
    using var host = builder.Build();

    var mode = args[0];

    if (!Enum.TryParse(mode, out Mode modeEnum))
      throw new Exception("Unknown mode");

    using var serviceScope = host.Services.CreateScope();
    switch (modeEnum)
    {
      case Mode.LibraryCallRedactPdf:
        await RedactPdfFileAsync(serviceScope.ServiceProvider);
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

    static async Task RedactPdfFileAsync(IServiceProvider serviceProvider)
    {
      var redactionService = serviceProvider.GetRequiredService<IRedactionProvider>();

      Console.WriteLine("Enter the input file path:");
      var filePath = Console.ReadLine();
      Console.WriteLine("Enter the output file path:");
      var outputFilePath = Console.ReadLine() ?? throw new Exception("Output file path is required");

      Console.WriteLine("Enter the path to the JSON file containing redaction data:");
      var jsonFilePath = Console.ReadLine() ?? throw new Exception("json file path is required");

      if (File.Exists(filePath))
      {
        try
        {
          using var fileStream = File.OpenRead(filePath);

          Guid currentCorrelationId = default;

          // Deserialize JSON data
          var jsonData = File.ReadAllText(jsonFilePath);
          var redactionData = JsonConvert.DeserializeObject<RedactionData>(jsonData) ?? throw new Exception("Redaction data is required, pass a json file");

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

          var redactPdf = new RedactPdfRequestDto
          {
            FileName = filePath,
            VersionId = 1,
            RedactionDefinitions = redactionDefinitions
          };

          var pdfStream = await redactionService.Redact(fileStream, 1234, "123", redactPdf, currentCorrelationId);

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
      var pipelineClientRequestFactory = serviceProvider.GetRequiredService<IPdfGeneratorRequestFactory>();
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

          Guid currentCorrelationId = Guid.NewGuid();
          var extension = Path.GetExtension(filePath).Replace(".", string.Empty).ToUpperInvariant();

          var fileType = Enum.Parse<FileType>(extension);

          var request = pipelineClientRequestFactory.Create(HttpMethod.Post, "urns/test-case-urn/cases/test-case-id/documents/test-document-id/versions/test-version-id/test-convert-to-pdf", currentCorrelationId);
          request.Headers.Add("Filetype", fileType.ToString());

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

      var conversionResult = orchestratorService.ReadToPdfStreamAsync(fileStream, fileType, documentId, currentCorrelationId).Result;

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

public class RedactionData
{
  public required string DocumentId { get; set; }
  public required List<RedactionDefinitionDto> Redactions { get; set; }
}
