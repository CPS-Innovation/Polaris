
using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact.Factories;

public class ArtefactServiceResponseFactory : IArtefactServiceResponseFactory
{

    public ArtefactResult<T> CreateOkfResult<T>(T result, bool? isFromStorage)
    {
        return new ArtefactResult<T>
        {
            Status = ResultStatus.ArtefactAvailable,
            Result = result,
            IsFromStorage = isFromStorage,
        };
    }

    public ArtefactResult<T> CreateInterimResult<T>(T result)
    {
        return new ArtefactResult<T>
        {
            Status = ResultStatus.PollWithToken,
            Result = result,
        };
    }

    public ArtefactResult<T> CreateFailedResult<T>(PdfConversionStatus? pdfConversionStatus)
    {
        return new ArtefactResult<T>
        {
            Status = ResultStatus.Failed,
            PdfConversionStatus = pdfConversionStatus,
        };
    }
}