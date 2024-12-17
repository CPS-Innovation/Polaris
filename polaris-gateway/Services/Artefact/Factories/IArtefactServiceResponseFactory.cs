using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact.Factories
{
    public interface IArtefactServiceResponseFactory
    {
        ArtefactResult<T> CreateOkfResult<T>(T result, bool? isFromStorage);

        ArtefactResult<T> CreateInterimResult<T>(Guid continuationToken);

        ArtefactResult<T> CreateFailedResult<T>(PdfConversionStatus? pdfConversionStatus);

        ArtefactResult<U> ConvertNonOkResult<T, U>(ArtefactResult<T> result);
    }
}

