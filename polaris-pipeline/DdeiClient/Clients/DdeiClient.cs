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

        public async Task<string> GetCmsModernToken(DdeiCmsCaseDataArgDto arg)
        {
            return await CallDdei<string>(
                        () => _ddeiClientRequestFactory.CreateCmsModernTokenRequest(arg),
                         arg.CorrelationId
            );
        }

        public async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiCmsUrnArgDto arg)
        {
            return await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(
                () => _ddeiClientRequestFactory.CreateListCasesRequest(arg),
                 arg.CorrelationId
            );
        }

        public async Task<DdeiCaseDetailsDto> GetCaseAsync(DdeiCmsCaseArgDto arg)
        {
            return await CallDdei<DdeiCaseDetailsDto>(
                () => _ddeiClientRequestFactory.CreateGetCaseRequest(arg),
                arg.CorrelationId
            );
        }

        public async Task<IEnumerable<DdeiDocumentDetailsDto>> ListCaseDocumentsAsync(DdeiCmsCaseArgDto arg)
        {
            try
            {
                return await CallDdei<IEnumerable<DdeiDocumentDetailsDto>>(
                    () => _ddeiClientRequestFactory.CreateListCaseDocumentsRequest(arg),
                     arg.CorrelationId
                );
            }
            catch (DdeiClientException httpException)
            {
                if (httpException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Enumerable.Empty<DdeiDocumentDetailsDto>();
                }
                throw;
            }
        }

        public async Task CheckoutDocument(DdeiCmsDocumentArgDto arg)
        {
            await CallDdei(
               () => _ddeiClientRequestFactory.CreateCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg)
        {
            await CallDdei(
               () => _ddeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg),
                arg.CorrelationId
           );
        }

        public async Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            await CallDdei(
               () => _ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream),
                arg.CorrelationId
            );
        }

        private async Task<T> CallDdei<T>(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            using var response = await CallDdei(requestFactory, correlationId);

            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> CallDdei(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            var request = requestFactory();
            var response = await _httpClient.SendAsync(request);
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
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