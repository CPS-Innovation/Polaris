﻿using System.IO;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Ddei;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using coordinator.Durable.Payloads;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Ddei.Factories;
namespace coordinator.Durable.Activity
{
    public class GeneratePdfFromDocument : BaseGeneratePdf
    {
        public GeneratePdfFromDocument(
            IPdfGeneratorClient pdfGeneratorCLient,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IPolarisBlobStorageService polarisBlobStorageService)
            : base(ddeiClient, ddeiArgFactory, polarisBlobStorageService, pdfGeneratorCLient) { }

        [FunctionName(nameof(GeneratePdfFromDocument))]
        public new async Task<(bool, PdfConversionStatus)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            return await base.Run(context);
        }

        protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
             => await DdeiClient.GetDocumentFromFileStoreAsync(
                    payload.Path,
                    payload.CmsAuthValues,
                    payload.CorrelationId);
    }
}