using Common.Dto.Request;

namespace PolarisGateway.common.Mappers
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest);
    }
}
