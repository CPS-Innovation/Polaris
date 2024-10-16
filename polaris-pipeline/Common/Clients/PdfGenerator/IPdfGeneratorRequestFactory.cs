using System;
using System.Net.Http;

namespace Common.Clients.PdfGenerator

{
    public interface IPdfGeneratorRequestFactory
    {
        HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null);
    }
}

