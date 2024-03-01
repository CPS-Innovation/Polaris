using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Common.Configuration;
using coordinator.Constants;
using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;
using Common.Wrappers.Contracts;
using Common.Factories.Contracts;
using Microsoft.Extensions.Configuration;

namespace coordinator.Factories
{
    public class TextExtractorClientRequestFactory : ITextExtractorClientRequestFactory
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TextExtractorClientRequestFactory(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public HttpRequestMessage Create(
            string caseUrn,
            long cmsCaseId,
            string searchTerm, Guid correlationId,
            IEnumerable<SearchFilterDocument> documents)
        {
            var request = _pipelineClientRequestFactory.Create(HttpMethod.Post, $"{RestApi.GetSearchPath(caseUrn, cmsCaseId)}?code={_configuration[ConfigKeys.PipelineTextExtractorFunctionAppKey]}", correlationId);
            var searchDto = new SearchRequestDto
            {
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