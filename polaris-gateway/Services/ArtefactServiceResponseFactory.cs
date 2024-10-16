using Common.Constants;

using PolarisGateway.Services.Domain;

namespace PolarisGateway.Services;

public class ArtefactServiceResponseFactory : IArtefactServiceResponseFactory
{

    public PdfResult CreateOkGetPdfResult(Stream pdfStream, bool isFromStorage)
    {
        return new PdfResult
        {
            Status = PdfResult.ResultStatus.PdfAvailable,
            Stream = pdfStream,
            IsFromStorage = isFromStorage,
        };
    }
    public PdfResult CreateFailedGetPdfResult(PdfConversionStatus pdfConversionStatus)
    {
        return new PdfResult
        {
            Status = PdfResult.ResultStatus.Failed,
            PdfConversionStatus = pdfConversionStatus,
        };
    }

    public JsonArtefactResult CreateOcrResultsAvailableOcrResult(Stream stream, bool isFromStorage)
    {
        return new JsonArtefactResult
        {
            Status = JsonArtefactResult.ResultStatus.ArtefactAvailable,
            Stream = stream,
            IsFromStorage = isFromStorage,
        };
    }

    public JsonArtefactResult CreatePollWithTokenInitiateOcrResult(Guid operationId)
    {
        return new JsonArtefactResult
        {
            Status = JsonArtefactResult.ResultStatus.PollWithToken,
            Token = operationId,
        };
    }

    public JsonArtefactResult CreateFailedOnPdfConversionOcrResult(PdfConversionStatus? pdfConversionStatus)
    {
        return new JsonArtefactResult
        {
            Status = JsonArtefactResult.ResultStatus.FailedOnPdfConversion,
            FailedPdfConversionStatus = pdfConversionStatus,
        };
    }
}