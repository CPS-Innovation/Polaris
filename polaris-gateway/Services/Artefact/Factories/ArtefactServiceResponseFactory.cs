using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;
using System;

namespace PolarisGateway.Services.Artefact.Factories;

public class ArtefactServiceResponseFactory : IArtefactServiceResponseFactory
{

    public ArtefactResult<T> CreateOkfResult<T>(T result, bool? isFromStorage)
    {
        return new ArtefactResult<T>
        {
            Status = ResultStatus.ArtefactAvailable,
            Artefact = result,
            IsFromStorage = isFromStorage,
        };
    }

    public ArtefactResult<T> CreateInterimResult<T>(Guid continuationToken)
    {
        return new ArtefactResult<T>
        {
            Status = ResultStatus.PollWithToken,
            ContinuationToken = continuationToken,
        };
    }

    public ArtefactResult<T> CreateFailedResult<T>(PdfConversionStatus? pdfConversionStatus)
    {
        return new ArtefactResult<T>
        {
            Status = ResultStatus.Failed,
            FailedStatus = pdfConversionStatus,
        };
    }

    public ArtefactResult<U> ConvertNonOkResult<T, U>(ArtefactResult<T> result)
    {
        return new ArtefactResult<U>
        {
            Status = result.Status,
            ContinuationToken = result.ContinuationToken,
            FailedStatus = result.FailedStatus,
            IsFromStorage = result.IsFromStorage,
        };
    }
}