using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.Case.PreCharge;
using Common.Extensions;
using Common.Services.RenderHtmlService;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public class PdfRetrievalService : IPdfRetrievalService
{
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IConvertModelToHtmlService _convertModelToHtmlService;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IMdsClient _mdsClient;
    private readonly IMasterDataServiceClient _masterDataServiceClient;

    public PdfRetrievalService(
        IMdsArgFactory mdsArgFactory,
        IConvertModelToHtmlService convertModelToHtmlService,
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient)
    {
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _convertModelToHtmlService = convertModelToHtmlService.ExceptionIfNull();
        _pdfGeneratorClient = pdfGeneratorClient.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    public async Task<DocumentRetrievalResult> GetPdfStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId, bool isLegacy = true)
    {
        var (stream, fileType, isKnownFileType) = DocumentNature.GetDocumentNatureType(materialId) switch
        {
            DocumentNature.Types.PreChargeDecisionRequest => await GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId),
            DocumentNature.Types.DefendantsAndCharges => await GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId),
            _ => await GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId)
        };

        if (!isKnownFileType)
        {
            return new DocumentRetrievalResult
            {
                Status = PdfConversionStatus.DocumentTypeUnsupported,
            };
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, materialId, documentId, stream, fileType, isLegacy);

        return new DocumentRetrievalResult
        {
            PdfStream = pdfResult.Status == PdfConversionStatus.DocumentConverted ? pdfResult.PdfStream : null,
            Status = pdfResult.Status,
            FailedStatusCode = pdfResult.Status != PdfConversionStatus.DocumentConverted ? pdfResult.FailedStatusCode : null,
        };
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId)
    {
        var mdsDocumentIdAndVersionIdArgDto = _mdsArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);
        var fileResult = await _mdsClient.GetDocumentAsync(mdsDocumentIdAndVersionIdArgDto);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return (fileResult.Stream, fileType, isKnownFileType);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
    {
        var mdsPcdArgDto = _mdsArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);
        //var pcdRequest = await _mdsClient.GetPcdRequestAsync(mdsPcdArgDto); ; // calls /cases/{arg.CaseId}/pcd-request/{arg.PcdId}      returns case.precharge.PcdRequestDto

        var pcdRequest = await _masterDataServiceClient.GetPcdRequestByPcdIdAsync(
            new GetPcdRequestByPcdIdCoreRequest(mdsPcdArgDto.CaseId, mdsPcdArgDto.PcdId, mdsPcdArgDto.CorrelationId),
            new CmsAuthValues(mdsPcdArgDto.CmsAuthValues, mdsPcdArgDto.CorrelationId));

        var stream = await _convertModelToHtmlService.ConvertAsync(MapPcdRequest(pcdRequest));
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
    {
        var mdsCaseIdentifiersArgDto = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);
        var defendantsAndCharges = await _mdsClient.GetDefendantAndChargesAsync(mdsCaseIdentifiersArgDto);
        var stream = await _convertModelToHtmlService.ConvertAsync(defendantsAndCharges);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private Common.Dto.Response.Case.PreCharge.PcdRequestDto MapPcdRequest(Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto request)
    {
        return new Common.Dto.Response.Case.PreCharge.PcdRequestDto
        {
            Id = request.Id,
            DecisionRequested = request.DecisionRequested,
            DecisionRequiredBy = request.DecisionRequiredBy,

            Comments = request.Comments == null
                ? null
                : new PcdCommentsDto
                {
                    Text = request.Comments.Text,
                    TextWithCmsMarkup = request.Comments.TextWithCmsMarkup,
                },

            CaseOutline = request.CaseOutline?.Select(co => new PcdCaseOutlineLineDto
            {
                Heading = co.Heading,
                Text = co.Text,
                TextWithCmsMarkup = co.TextWithCmsMarkup,
            }).ToList(),

            Suspects = request.Suspects?.Select(sus => new PcdRequestSuspectDto
            {
                Surname = sus.Surname,
                FirstNames = sus.FirstNames,
                Dob = sus.Dob,
                BailConditions = sus.BailConditions,
                BailDate = sus.BailDate,
                RemandStatus = sus.RemandStatus,

                ProposedCharges = sus.ProposedCharges?.Select(charge => new PcdProposedChargeDto
                {
                    Charge = charge.Charge,
                    EarlyDate = charge.EarlyDate,
                    LateDate = charge.LateDate,
                    Location = charge.Location,
                    Category = charge.Category,
                }).ToList(),
            }).ToList(),
        };
    }
}
