using Common.Constants;
using PolarisGateway.Services.Domain;

namespace PolarisGateway.Services;

public interface IArtefactServiceResponseFactory
{
    PdfResult CreateOkGetPdfResult(Stream pdfStream, bool isFromStorage);

    PdfResult CreateFailedGetPdfResult(PdfConversionStatus pdfConversionStatus);

    JsonArtefactResult CreateOcrResultsAvailableOcrResult(Stream stream, bool isFromStorage);

    JsonArtefactResult CreatePollWithTokenInitiateOcrResult(Guid operationId);

    JsonArtefactResult CreateFailedOnPdfConversionOcrResult(PdfConversionStatus? pdfConversionStatus);
}