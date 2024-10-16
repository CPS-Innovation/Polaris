using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact.Factories
{
    public interface IArtefactServiceResponseFactory
    {
        ArtefactResult<T> CreateOkfResult<T>(T result, bool? isFromStorage);

        ArtefactResult<T> CreateInterimResult<T>(T result);

        ArtefactResult<T> CreateFailedResult<T>(PdfConversionStatus? pdfConversionStatus);
    }
}

