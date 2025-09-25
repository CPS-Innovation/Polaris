using Common.Configuration;
using Common.Domain.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Services.PdfServices;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using pdf_generator.Models;

namespace pdf_generator.Functions;

public class Health
{
    private readonly IPdfService _pdfService;

    public Health([FromKeyedServices(FileType.PDF)] IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [Function("Health")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Health)] HttpRequest req)
    {
        const string targetMessage = "The namespace declaration attribute has an incorrect 'namespaceURI': 'http://www.w3.org/2000/xmlns/'";
        await using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.HealthCheckResources.TestPdf.pdf") ?? Stream.Null;
        var conversionResult = await _pdfService.ReadToPdfStreamAsync(new ReadToPdfDto()
        {
            Stream = inputStream,
            DocumentId = "12345",
            CorrelationId = Guid.NewGuid()
        });

        if (conversionResult.HasFailureReason() && conversionResult.Feedback.Contains(targetMessage))
        {
            return new ObjectResult(conversionResult.GetFailureReason())
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        return new OkResult();
    }
}