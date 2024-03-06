using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest);
    }
}
