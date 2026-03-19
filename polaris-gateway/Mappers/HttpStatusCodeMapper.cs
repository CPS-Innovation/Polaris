using Common.Constants;
using System.Net;

namespace PolarisGateway.Mappers
{
    public class HttpStatusCodeMapper : IHttpStatusCodeMapper
    {
        public int Map(PdfConversionStatus? status)
        {
            switch (status)
            {
                case PdfConversionStatus.AsposeWordsPasswordProtected:
                    return (int)HttpStatusCode.Forbidden;

                case PdfConversionStatus.AsposePdfPasswordProtected:
                    return (int)HttpStatusCode.Forbidden;

                default:
                    return (int)HttpStatusCode.UnsupportedMediaType;
            }
        }
    }
}
