// <copyright file="GeneratePdfFromDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Durable.Activity;

using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using Common.Dto.Request;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class GeneratePdfFromDocument : BaseGeneratePdf
{
    public GeneratePdfFromDocument(
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsArgFactory mdsArgFactory,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IConfiguration configuration,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient)
        : base(mdsArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient, masterDataServiceClient)
    {
    }

    [Function(nameof(GeneratePdfFromDocument))]
    public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        return await base.Run(payload);
    }

    protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
    {
        var arg = this.MdsArgFactory.CreateDocumentVersionArgDto(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId,
            payload.MaterialId,
            payload.DocumentId);

        var result = await this.masterDataServiceClient.GetDocumentByDocumentIdAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        return result.Stream;
    }
}
