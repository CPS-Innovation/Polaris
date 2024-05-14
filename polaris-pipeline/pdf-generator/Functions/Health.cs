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
            await using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.HealthCheckResources.TestPdf.pdf");
            try
            {
                var conversionResult = await _pdfOrchestratorService.ReadToPdfStreamAsync(inputStream, FileType.PDF, "12345", Guid.NewGuid());
            
                if (conversionResult.HasFailureReason())
                {
                    return new ObjectResult(conversionResult.GetFailureReason())
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }

                return new OkResult();
            }
            catch (Exception exception)
            {
                return new ObjectResult(exception.ToFormattedString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
