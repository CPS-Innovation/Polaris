using Common.Constants;
using System.Net;

namespace PolarisGateway.Mappers
{
    public class HttpStatusCodeMapper : IHttpStatusCodeMapper
    {
        public int Map(PdfConversionStatus? status)
        {
            return status switch
            {
                PdfConversionStatus.AsposeWordsPasswordProtected
                or PdfConversionStatus.AsposePdfPasswordProtected
                or PdfConversionStatus.AsposeSlidesPasswordProtected
                    => (int)HttpStatusCode.Forbidden,

                _ => (int)HttpStatusCode.UnsupportedMediaType
            };
        }
    }
}
