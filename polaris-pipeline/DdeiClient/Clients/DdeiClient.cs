using Ddei.Factories.Contracts;
using Ddei.Domain;
using Ddei.Exceptions;
using Ddei.Domain.CaseData.Args;
using Common.Wrappers.Contracts;

namespace Ddei.Clients
{
    public class DdeiClient : IDdeiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDdeiClientRequestFactory _ddeiClientRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public DdeiClient(HttpClient httpClient, IDdeiClientRequestFactory ddeiClientRequestFactory, IJsonConvertWrapper jsonConvertWrapper)
        {
            _httpClient = httpClient;
            _ddeiClientRequestFactory = ddeiClientRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<string> GetCmsModernToken(CmsCaseDataArg arg)
        {
            return await CallDdei<string>(
                        () => _ddeiClientRequestFactory.CreateCmsModernTokenRequest(arg),
                         arg.CorrelationId
            );
        }

        public async Task<IEnumerable<CaseIdentifiers>> ListCaseIdsAsync(CmsUrnArg arg)
        {
            return await CallDdei<IEnumerable<CaseIdentifiers>>(
                () => _ddeiClientRequestFactory.CreateListCasesRequest(arg),
                 arg.CorrelationId
            );
        }

        public async Task<CaseDetails> GetCaseAsync(CmsCaseArg arg)
        {
            return await CallDdei<CaseDetails>(
                () => _ddeiClientRequestFactory.CreateGetCaseRequest(arg),
                arg.CorrelationId
            );
        }

        public async Task<IEnumerable<DocumentDetails>> ListCaseDocumentsAsync(CmsCaseArg arg)
        {
            try
            {
                return await CallDdei<IEnumerable<DocumentDetails>>(
                    () => _ddeiClientRequestFactory.CreateListCaseDocumentsRequest(arg),
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

        public async Task CheckoutDocument(CmsDocumentArg arg)
        {
            await CallDdei(
               () => _ddeiClientRequestFactory.CreateCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task CancelCheckoutDocument(CmsDocumentArg arg)
        {
            await CallDdei(
               () => _ddeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task UploadPdf(CmsDocumentArg arg, Stream stream, string filename)
        {
            await CallDdei(
               () => _ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream, filename),
                arg.CorrelationId
            );
        }

        private async Task<T> CallDdei<T>(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            using var response = await CallDdeiInternal(requestFactory, correlationId);

            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content);
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
                var content = await response.Content.ReadAsStringAsync();
                if(!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(content);
                }
                return response;
            }
            catch (HttpRequestException exception)
            {
                throw new DdeiClientException(response.StatusCode, exception);
            }
        }
    }
}