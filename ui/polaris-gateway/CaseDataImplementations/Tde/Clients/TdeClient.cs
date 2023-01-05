using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RumpoleGateway.Domain.CaseData.Args;
using RumpoleGateway.CaseDataImplementations.Tde.Domain;
using RumpoleGateway.CaseDataImplementations.Tde.Factories;
using RumpoleGateway.Wrappers;
using RumpoleGateway.CaseDataImplementations.Tde.Domain.Exceptions;
using System.IO;

namespace RumpoleGateway.CaseDataImplementations.Tde.Clients
{
    public class TdeClient : ITdeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITdeClientRequestFactory _tdeClientRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public TdeClient(HttpClient httpClient, ITdeClientRequestFactory tdeClientRequestFactory, IJsonConvertWrapper jsonConvertWrapper)
        {
            _httpClient = httpClient;
            _tdeClientRequestFactory = tdeClientRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<IEnumerable<CaseIdentifiers>> ListCaseIdsAsync(UrnArg arg)
        {
            return await CallTde<IEnumerable<CaseIdentifiers>>(
                () => _tdeClientRequestFactory.CreateListCasesRequest(arg),
                 arg.CorrelationId
            );
        }

        public async Task<CaseDetails> GetCaseAsync(CaseArg arg)
        {
            return await CallTde<CaseDetails>(
                () => _tdeClientRequestFactory.CreateGetCaseRequest(arg),
                arg.CorrelationId
            );
        }

        public async Task<IEnumerable<DocumentDetails>> ListCaseDocumentsAsync(CaseArg arg)
        {
            try
            {
                return await CallTde<IEnumerable<DocumentDetails>>(
                    () => _tdeClientRequestFactory.CreateListCaseDocumentsRequest(arg),
                     arg.CorrelationId
                );
            }
            catch (TdeClientException httpException)
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
            await CallTde(
               () => _tdeClientRequestFactory.CreateCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task CancelCheckoutDocument(DocumentArg arg)
        {
            await CallTde(
               () => _tdeClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task UploadPdf(DocumentArg arg, Stream stream, string filename)
        {
            await CallTde(
               () => _tdeClientRequestFactory.CreateUploadPdfRequest(arg, stream, filename),
                arg.CorrelationId
            );
        }

        private async Task<T> CallTde<T>(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            using var response = await CallTdeInternal(requestFactory, correlationId);

            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content, correlationId);
        }


        private async Task<HttpResponseMessage> CallTde(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            return await CallTdeInternal(requestFactory, correlationId);
        }

        private async Task<HttpResponseMessage> CallTdeInternal(Func<HttpRequestMessage> requestFactory, Guid correlationId)
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
                throw new TdeClientException(response.StatusCode, exception);
            }
        }
    }
}