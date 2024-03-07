namespace PolarisGateway.Clients
{
    public interface ICoordinatorRequestFactory
    {
        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues, HttpContent content);
    }
}

