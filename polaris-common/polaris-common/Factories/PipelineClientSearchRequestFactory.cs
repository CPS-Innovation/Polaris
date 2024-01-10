using System.Text;
using Microsoft.Extensions.Configuration;
using polaris_common.Configuration;
using polaris_common.Constants;
using polaris_common.Domain.SearchIndex;
using polaris_common.Dto.Request.Search;
using polaris_common.Factories.Contracts;
using polaris_common.Wrappers.Contracts;

namespace polaris_common.Factories
{
    public class PipelineClientSearchRequestFactory : IPipelineClientSearchRequestFactory
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public PipelineClientSearchRequestFactory(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public HttpRequestMessage Create(
          long cmsCaseId,
            string searchTerm, Guid correlationId,
            IEnumerable<SearchFilterDocument> documents)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.Search}?code={_configuration[PipelineSettings.PipelineTextExtractorFunctionAppKey]}", correlationId);
            var searchDto = new SearchRequestDto
            {
                CaseId = cmsCaseId,
                SearchTerm = searchTerm,
                Documents = documents.ToList().Select(doc => new SearchRequestDocumentDto
                {
                    CmsDocumentId = doc.CmsDocumentId,
                    CmsVersionId = doc.CmsVersionId,
                }).ToList()
            };

            var json = _jsonConvertWrapper.SerializeObject(searchDto);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }
    }
}