using System;
using System.Net.Http;

namespace coordinator.Clients.TextExtractor
{
    public interface IRequestFactory
    {
        HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId);
    }
}