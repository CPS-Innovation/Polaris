using Common.Domain.Document;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Factories;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.PdfService;
using PolarisGateway.Domain.Exceptions;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("app-settings.json", true);

var configuration = builder.Build();

var services = new ServiceCollection();
ConfigureServices(services, configuration);
services
    .AddLogging(l =>
    {
        l.AddConsole();
        l.SetMinimumLevel(LogLevel.Information);
    })
    .AddSingleton<Executor, Executor>()
    .BuildServiceProvider()
    .GetService<Executor>()
    ?.Execute();
return;

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    SetAsposeLicence();
    services.AddSingleton<IPdfService, WordsPdfService>();
    services.AddSingleton<IPdfService, CellsPdfService>();
    services.AddSingleton<IPdfService, SlidesPdfService>();
    services.AddSingleton<IPdfService, ImagingPdfService>();
    services.AddSingleton<IPdfService, DiagramPdfService>();
    services.AddSingleton<IPdfService, HtmlPdfService>();
    services.AddSingleton<IPdfService, EmailPdfService>();
    services.AddSingleton<IPdfService, PdfRendererService>();
    services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
    {
        var pdfServices = provider.GetServices<IPdfService>();
        var servicesList = pdfServices.ToList();
        var wordsPdfService = servicesList.First(s => s.GetType() == typeof(WordsPdfService));
        var cellsPdfService = servicesList.First(s => s.GetType() == typeof(CellsPdfService));
        var slidesPdfService = servicesList.First(s => s.GetType() == typeof(SlidesPdfService));
        var imagingPdfService = servicesList.First(s => s.GetType() == typeof(ImagingPdfService));
        var diagramPdfService = servicesList.First(s => s.GetType() == typeof(DiagramPdfService));
        var htmlPdfService = servicesList.First(s => s.GetType() == typeof(HtmlPdfService));
        var emailPdfService = servicesList.First(s => s.GetType() == typeof(EmailPdfService));
        var pdfRendererService = servicesList.First(s => s.GetType() == typeof(PdfRendererService));
        var loggingService = provider.GetService<ILogger<PdfOrchestratorService>>();

        return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService,
            diagramPdfService, htmlPdfService, emailPdfService, pdfRendererService, loggingService, configuration);
    });
    services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
    services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();
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
        throw new AsposeLicenseException(exception.Message);
    }
}

internal class Executor
{
    private readonly IPdfOrchestratorService _pdfOrchestratorService;
    
    public Executor(IPdfOrchestratorService pdfOrchestratorService)
    {
        _pdfOrchestratorService = pdfOrchestratorService;
    }

    public void Execute()
    {
        var toConvertPath = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")), "to-convert");
        var toStorePath = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")), "converted");
        
        SinglePassThrough(toConvertPath, toStorePath);
        //return MultiplePassThrough(toConvertPath, toStorePath, 100);
    }

    private void SinglePassThrough(string toConvertPath, string toStorePath)
    {
        foreach (var file in Directory.GetFiles(toConvertPath))
        {
            var fs = File.Open(file, FileMode.Open);
            var fileName = Guid.NewGuid();
            var currentFileType = file.Contains(".hte") ? FileType.HTE : FileType.CSV;
            
            var conversionResult = _pdfOrchestratorService.ReadToPdfStream(fs, currentFileType, "12345678", fileName);

            using var fsOut = File.Create(Path.Combine(toStorePath, string.Concat(fileName.ToString(), ".pdf")));
            conversionResult.Seek(0, SeekOrigin.Begin);
            conversionResult.CopyTo(fsOut);
        }
    }

    /*private async Task MultiplePassThrough(string toConvertPath, string toStorePath, int numberIterations)
    {
        var tasks = new List<Task<int>>();
        for (var i = 0; i <= numberIterations; i++)
        {
            tasks.Add(ConvertFilesAsync(toConvertPath, toStorePath, i));
        }
                
        foreach (var task in await Task.WhenAll(tasks))
        {
            Console.WriteLine("Processing iteration {0} completed", task.ToString());
        }
        
        Console.WriteLine("--------------------------");
    }

    private async Task<int> ConvertFilesAsync(string toConvertPath, string toStorePath, int iterationId)
    {
        Console.WriteLine("Processing iteration {0}", iterationId.ToString());
        foreach (var file in Directory.GetFiles(toConvertPath))
        {
            await using var fs = File.Open(file, FileMode.Open);
            var originalBytes = fs.Length;
            Console.WriteLine("Original Bytes: {0}", originalBytes);
            var fileName = Guid.NewGuid();
            var conversionResult = _pdfOrchestratorService.ReadToPdfStream(fs, FileType.HTE, "12345678", fileName);

            await using var fsOut = File.Create(Path.Combine(toStorePath, string.Concat(fileName.ToString(), ".pdf")));
            conversionResult.Seek(0, SeekOrigin.Begin);
            conversionResult.CopyTo(fsOut);
            var convertedBytes = fsOut.Length;
            Console.WriteLine("Converted Bytes: {0}", convertedBytes);
        }
        Console.WriteLine("--------------------------");

        return iterationId;
    }*/
}