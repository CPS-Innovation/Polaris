using Common.Constants;

namespace PolarisGateway.Services.Artefact.Domain;

public class ArtefactResult<T>
{
    public ResultStatus Status { get; set; }
    public T Result { get; set; }
    public PdfConversionStatus? PdfConversionStatus { get; set; }
    public bool? IsFromStorage { get; set; }
}