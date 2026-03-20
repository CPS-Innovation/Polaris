using Common.Constants;

namespace PolarisGateway.Mappers
{
    public interface IHttpStatusCodeMapper
    {
        int Map(PdfConversionStatus? status);
    }
}
