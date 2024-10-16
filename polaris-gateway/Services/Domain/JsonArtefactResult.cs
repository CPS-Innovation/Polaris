using Common.Constants;

namespace PolarisGateway.Services.Domain;

public class JsonArtefactResult
{
    public ResultStatus Status { get; set; }
    public Stream Stream { get; set; }
    public Guid Token { get; set; }
    public PdfConversionStatus? FailedPdfConversionStatus { get; set; }
    public bool? IsFromStorage { get; set; }
    public enum ResultStatus
    {
        ArtefactAvailable,
        PollWithToken,
        FailedOnPdfConversion,
    }
}