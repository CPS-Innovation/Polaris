namespace PolarisGateway.Clients.Coordinator
{
    public interface IRequestFactory
    {
        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues, HttpContent content);
    }
}

