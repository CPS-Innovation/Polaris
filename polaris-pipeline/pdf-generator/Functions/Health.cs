using System;
using System.Net;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using pdf_generator.Extensions;
using pdf_generator.Services.PdfService;

namespace pdf_generator.Functions
{
    public class Health
    {
        private readonly IPdfOrchestratorService _pdfOrchestratorService;

        public Health(IPdfOrchestratorService pdfOrchestratorService)
        {
            _pdfOrchestratorService = pdfOrchestratorService;
        }
        
        [Function("Health")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Health)] HttpRequest req)
        {
            const string targetMessage = "The namespace declaration attribute has an incorrect 'namespaceURI': 'http://www.w3.org/2000/xmlns/'";
            await using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.HealthCheckResources.TestPdf.pdf");
            var conversionResult = await _pdfOrchestratorService.ReadToPdfStreamAsync(inputStream, FileType.PDF, "12345", Guid.NewGuid());
        
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
}
