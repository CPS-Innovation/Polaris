using PolarisGateway.Domain.CaseData;
using PolarisGateway.CaseDataImplementations.Ddei.Mappers;
using Ddei.Services;
using Ddei.Clients;
using Ddei.Factories.Contracts;
using PolarisGateway.Services;
using Ddei.Exceptions;
using Domain.Exceptions;
using Ddei.Services.Contract;
using Ddei.Domain.CaseData.Args;

namespace PolarisGateway.CaseDataImplementations.Ddei.Services
{
    public class DdeiService : ICaseDataService, IDocumentService, ICmsModernTokenService
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly ICaseDataArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentsMapper _caseDocumentsMapper;

        public DdeiService(
            IDdeiClient ddeiClient,
            ICaseDataArgFactory caseDataServiceArgFactory,
            ICaseDetailsMapper caseDetailsMapper,
            ICaseDocumentsMapper caseDocumentsMapper)
        {
            _ddeiClient = ddeiClient;
            _caseDataServiceArgFactory = caseDataServiceArgFactory;
            _caseDetailsMapper = caseDetailsMapper;
            _caseDocumentsMapper = caseDocumentsMapper;
        }

        public async Task<string> GetCmsModernToken(CmsCaseDataArg arg)
        {
            try
            {
                return await _ddeiClient.GetCmsModernToken(arg);
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCmsModernToken", exception);
            }
        }

        public async Task<IEnumerable<CaseDetails>> ListCases(CmsUrnArg arg)
        {
            try
            {
                var caseIdentifiers = await _ddeiClient.ListCaseIdsAsync(arg);

                var calls = caseIdentifiers.Select(async caseIdentifier =>
                     await _ddeiClient.GetCaseAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

                var cases = await Task.WhenAll(calls);

                return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in ListCases", exception);
            }
        }

        public async Task<CaseDetailsFull> GetCase(CmsCaseArg arg)
        {
            try
            {
                var @case = await _ddeiClient.GetCaseAsync(arg);
                return _caseDetailsMapper.MapCaseDetails(@case);
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCase", exception);
            }
        }

        public async Task<IEnumerable<DocumentDetails>> ListDocuments(CmsCaseArg arg)
        {
            try
            {
                var documents = await _ddeiClient.ListCaseDocumentsAsync(arg);

                return documents
                    .Select(document => _caseDocumentsMapper.MapDocumentDetails(document))
                    // todo: we get empty filenames coming back from DDEI
                    .Where(document => !string.IsNullOrWhiteSpace(document.FileName));
            }

            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in ListDocuments", exception);
            }
        }

        public async Task CheckoutDocument(CmsDocumentArg arg)
        {
            try
            {
                await _ddeiClient.CheckoutDocument(arg);
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task CancelCheckoutDocument(CmsDocumentArg arg)
        {
            try
            {
                await _ddeiClient.CancelCheckoutDocument(arg);
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task UploadPdf(CmsDocumentArg arg, Stream stream)
        {
            try
            {
                await _ddeiClient.UploadPdf(arg, stream);
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in UploadPdf", exception);
            }
        }
    }
}