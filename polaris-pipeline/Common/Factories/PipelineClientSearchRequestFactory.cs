using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Common.Configuration;
using Common.Constants;
using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;
using Common.Factories.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;

namespace Common.Factories
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