using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.Factories;
using PolarisGateway.CaseDataImplementations.Ddei.Clients;
using PolarisGateway.Services;
using PolarisGateway.CaseDataImplementations.Ddei.Mappers;
using PolarisGateway.Domain.Exceptions;
using System.IO;

namespace PolarisGateway.CaseDataImplementations.Ddei.Services
{
    public class DdeiService : ICaseDataService, IDocumentService
    {
        private readonly IDdeiClient _tdeClient;
        private readonly ICaseDataArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentsMapper _caseDocumentsMapper;

        public DdeiService(
            IDdeiClient tdeClient,
            ICaseDataArgFactory caseDataServiceArgFactory,
            ICaseDetailsMapper caseDetailsMapper,
            ICaseDocumentsMapper caseDocumentsMapper)
        {
            _tdeClient = tdeClient;
            _caseDataServiceArgFactory = caseDataServiceArgFactory;
            _caseDetailsMapper = caseDetailsMapper;
            _caseDocumentsMapper = caseDocumentsMapper;
        }

        public async Task<IEnumerable<CaseDetails>> ListCases(UrnArg arg)
        {
            try
            {
                var caseIdentifiers = await _tdeClient.ListCaseIdsAsync(arg);

                var calls = caseIdentifiers.Select(async caseIdentifier =>
                     await _tdeClient.GetCaseAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

                var cases = await Task.WhenAll(calls);

                return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in ListCases", exception);
            }
        }

        public async Task<CaseDetailsFull> GetCase(CaseArg arg)
        {
            try
            {
                var @case = await _tdeClient.GetCaseAsync(arg);
                return _caseDetailsMapper.MapCaseDetails(@case);
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCase", exception);
            }
        }

        public async Task<IEnumerable<DocumentDetails>> ListDocuments(CaseArg arg)
        {
            try
            {
                var documents = await _tdeClient.ListCaseDocumentsAsync(arg);

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

        public async Task CheckoutDocument(DocumentArg arg)
        {
            try
            {
                await _tdeClient.CheckoutDocument(arg);
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task CancelCheckoutDocument(DocumentArg arg)
        {
            try
            {
                await _tdeClient.CancelCheckoutDocument(arg);
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task UploadPdf(DocumentArg arg, Stream stream, string fileName)
        {
            try
            {
                await _tdeClient.UploadPdf(arg, stream, fileName);
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in UploadPdf", exception);
            }
        }
    }
}