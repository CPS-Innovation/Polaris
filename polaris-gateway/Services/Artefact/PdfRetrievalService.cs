// <copyright file="PdfRetrievalService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.Artefact;

using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.Case.PreCharge;
using Common.Extensions;
using Common.Services.RenderHtmlService;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class PdfRetrievalService : IPdfRetrievalService
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IConvertModelToHtmlService convertModelToHtmlService;
    private readonly IPdfGeneratorClient pdfGeneratorClient;
    private readonly IMdsClient mdsClient;
    private readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly ICaseDetailsMapper caseDetailsMapper;

    public PdfRetrievalService(
        IMdsArgFactory mdsArgFactory,
        IConvertModelToHtmlService convertModelToHtmlService,
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        ICaseDetailsMapper caseDetailsMapper)
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.convertModelToHtmlService = convertModelToHtmlService.ExceptionIfNull();
        this.pdfGeneratorClient = pdfGeneratorClient.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        this.caseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
    }

    public async Task<DocumentRetrievalResult> GetPdfStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId, bool isLegacy = true)
    {
        var (stream, fileType, isKnownFileType) = DocumentNature.GetDocumentNatureType(materialId) switch
        {
            DocumentNature.Types.PreChargeDecisionRequest => await this.GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId),
            DocumentNature.Types.DefendantsAndCharges => await this.GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId),
            _ => await this.GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId)
        };

        if (!isKnownFileType)
        {
            return new DocumentRetrievalResult
            {
                Status = PdfConversionStatus.DocumentTypeUnsupported,
            };
        }

        var pdfResult = await this.pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, stream, fileType, isLegacy);

        return new DocumentRetrievalResult
        {
            PdfStream = pdfResult.Status == PdfConversionStatus.DocumentConverted ? pdfResult.PdfStream : null,
            Status = pdfResult.Status,
            FailedStatusCode = pdfResult.Status != PdfConversionStatus.DocumentConverted ? pdfResult.FailedStatusCode : null,
        };
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId)
    {
        var mdsDocumentIdAndVersionIdArgDto = this.mdsArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);
        var fileResult = await this.mdsClient.GetDocumentAsync(mdsDocumentIdAndVersionIdArgDto);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return (fileResult.Stream, fileType, isKnownFileType);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
    {
        var mdsPcdArgDto = this.mdsArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);

        var pcdRequest = await this.masterDataServiceClient.GetPcdRequestByPcdIdAsync(
            new GetPcdRequestByPcdIdCoreRequest(mdsPcdArgDto.CaseId, mdsPcdArgDto.PcdId, mdsPcdArgDto.CorrelationId),
            new CmsAuthValues(mdsPcdArgDto.CmsAuthValues, mdsPcdArgDto.CorrelationId));

        var stream = await this.convertModelToHtmlService.ConvertAsync(this.caseDetailsMapper.MapPcdRequest(pcdRequest));
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
    {
        var mdsCaseIdentifiersArgDto = this.mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);

        var defendantsAndCharges = await this.masterDataServiceClient.GetCaseDefendantsAsync(
            new ListCaseDefendantsRequest(mdsCaseIdentifiersArgDto.CaseId, mdsCaseIdentifiersArgDto.CorrelationId),
            new CmsAuthValues(mdsCaseIdentifiersArgDto.CmsAuthValues, mdsCaseIdentifiersArgDto.CorrelationId));

        var defendantsAndChargesMapped = this.caseDetailsMapper.MapDefendantsResponseToDefendantsAndChargesListDto(defendantsAndCharges, mdsCaseIdentifiersArgDto.CaseId);

        var stream = await this.convertModelToHtmlService.ConvertAsync(defendantsAndChargesMapped);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }
}
