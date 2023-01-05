using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.CaseDataImplementations.Ddei.Domain;
using PolarisGateway.CaseDataImplementations.Ddei.Factories;
using PolarisGateway.Wrappers;
using PolarisGateway.CaseDataImplementations.Ddei.Domain.Exceptions;
using System.IO;

namespace PolarisGateway.CaseDataImplementations.Ddei.Clients
{
    public class DdeiClient : IDdeiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDdeiClientRequestFactory _tdeClientRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public DdeiClient(HttpClient httpClient, IDdeiClientRequestFactory tdeClientRequestFactory, IJsonConvertWrapper jsonConvertWrapper)
        {
            _httpClient = httpClient;
            _tdeClientRequestFactory = tdeClientRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<IEnumerable<CaseIdentifiers>> ListCaseIdsAsync(UrnArg arg)
        {
            return await CallDdei<IEnumerable<CaseIdentifiers>>(
                () => _tdeClientRequestFactory.CreateListCasesRequest(arg),
                 arg.CorrelationId
            );
        }

        public async Task<CaseDetails> GetCaseAsync(CaseArg arg)
        {
            return await CallDdei<CaseDetails>(
                () => _tdeClientRequestFactory.CreateGetCaseRequest(arg),
                arg.CorrelationId
            );
        }

        public async Task<IEnumerable<DocumentDetails>> ListCaseDocumentsAsync(CaseArg arg)
        {
            try
            {
                return await CallDdei<IEnumerable<DocumentDetails>>(
                    () => _tdeClientRequestFactory.CreateListCaseDocumentsRequest(arg),
                     arg.CorrelationId
                );
            }
            catch (DdeiClientException httpException)
            {
                if (httpException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Enumerable.Empty<DocumentDetails>();
                }
                throw;
            }

        }

        public async Task CheckoutDocument(DocumentArg arg)
        {
            await CallDdei(
               () => _tdeClientRequestFactory.CreateCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task CancelCheckoutDocument(DocumentArg arg)
        {
            await CallDdei(
               () => _tdeClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task UploadPdf(DocumentArg arg, Stream stream, string filename)
        {
            await CallDdei(
               () => _tdeClientRequestFactory.CreateUploadPdfRequest(arg, stream, filename),
                arg.CorrelationId
            );
        }

        private async Task<T> CallDdei<T>(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            using var response = await CallDdeiInternal(requestFactory, correlationId);

            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content, correlationId);
        }


        private async Task<HttpResponseMessage> CallDdei(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            return await CallDdeiInternal(requestFactory, correlationId);
        }

        private async Task<HttpResponseMessage> CallDdeiInternal(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            var request = requestFactory();
            var response = await _httpClient.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException exception)
            {
                throw new DdeiClientException(response.StatusCode, exception);
            }
        }
    }
}