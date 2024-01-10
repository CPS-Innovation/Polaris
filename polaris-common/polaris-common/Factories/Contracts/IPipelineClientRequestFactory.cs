namespace polaris_common.Factories.Contracts
{
    public interface IPipelineClientRequestFactory
    {
        HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null);
    }
}

