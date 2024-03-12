using System;
using System.Net.Http;

namespace coordinator.Clients.PdfRedactor

{
    public interface IRequestFactory
    {
        HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null);
    }
}