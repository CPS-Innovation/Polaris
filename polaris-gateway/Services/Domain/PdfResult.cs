using Common.Constants;

namespace PolarisGateway.Services.Domain;

public class PdfResult
{
    public ResultStatus Status { get; set; }
    public Stream Stream { get; set; }
    public PdfConversionStatus? PdfConversionStatus { get; set; }
    public bool? IsFromStorage { get; set; }

    public enum ResultStatus
    {
        PdfAvailable,
        Failed
    }
}