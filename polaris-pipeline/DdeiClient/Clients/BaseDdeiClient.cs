using Azure;
using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Exceptions;
using Common.Extensions;
using Common.Telemetry;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Domain.Response;
using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.Document;
using Ddei.Domain.Response.PreCharge;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using DdeiClient.Domain.Response.Document;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace DdeiClient.Clients;

public abstract class BaseDdeiClient : IDdeiClient
{
    protected readonly HttpClient HttpClient;
    protected readonly IDdeiArgFactory CaseDataServiceArgFactory;
    protected readonly ICaseDetailsMapper CaseDetailsMapper;
    protected readonly ICaseDocumentMapper<DdeiDocumentResponse> CaseDocumentMapper;
    protected readonly ICaseDocumentNoteMapper CaseDocumentNoteMapper;
    protected readonly ICaseDocumentNoteResultMapper CaseDocumentNoteResultMapper;
    protected readonly ICaseExhibitProducerMapper CaseExhibitProducerMapper;
    protected readonly ICaseWitnessMapper CaseWitnessMapper;
    protected readonly ICaseIdentifiersMapper CaseIdentifiersMapper;
    protected readonly ICmsMaterialTypeMapper CmsMaterialTypeMapper;
    protected readonly ICaseWitnessStatementMapper CaseWitnessStatementMapper;
    protected readonly IJsonConvertWrapper JsonConvertWrapper;
    protected readonly IDdeiClientRequestFactory DdeiClientRequestFactory;
    protected readonly ILogger<BaseDdeiClient> Logger;
    private readonly ITelemetryClient _telemetryClient;

    protected BaseDdeiClient(
        HttpClient httpClient,
        IDdeiClientRequestFactory ddeiClientRequestFactory,
        IDdeiArgFactory caseDataServiceArgFactory,
        ICaseDetailsMapper caseDetailsMapper,
        ICaseDocumentMapper<DdeiDocumentResponse> caseDocumentMapper,
        ICaseDocumentNoteMapper caseDocumentNoteMapper,
        ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper,
        ICaseExhibitProducerMapper caseExhibitProducerMapper,
        ICaseWitnessMapper caseWitnessMapper,
        ICaseIdentifiersMapper caseIdentifiersMapper,
        ICmsMaterialTypeMapper cmsMaterialTypeMapper,
        ICaseWitnessStatementMapper caseWitnessStatementMapper,
        IJsonConvertWrapper jsonConvertWrapper,
        ILogger<BaseDdeiClient> logger,
        ITelemetryClient telemetryClient)
    {
        HttpClient = httpClient.ExceptionIfNull();
        CaseDataServiceArgFactory = caseDataServiceArgFactory.ExceptionIfNull();
        CaseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
        CaseDocumentMapper = caseDocumentMapper.ExceptionIfNull();
        CaseDocumentNoteMapper = caseDocumentNoteMapper.ExceptionIfNull();
        CaseDocumentNoteResultMapper = caseDocumentNoteResultMapper.ExceptionIfNull();
        CaseExhibitProducerMapper = caseExhibitProducerMapper.ExceptionIfNull();
        CaseWitnessMapper = caseWitnessMapper.ExceptionIfNull();
        CaseIdentifiersMapper = caseIdentifiersMapper.ExceptionIfNull();
        CmsMaterialTypeMapper = cmsMaterialTypeMapper.ExceptionIfNull();
        CaseWitnessStatementMapper = caseWitnessStatementMapper.ExceptionIfNull();
        JsonConvertWrapper = jsonConvertWrapper.ExceptionIfNull();
        DdeiClientRequestFactory = ddeiClientRequestFactory.ExceptionIfNull();
        Logger = logger.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
    }

    public virtual async Task VerifyCmsAuthAsync(DdeiBaseArgDto arg) =>
        // Will throw in the same way as any other call if auth is not correct.
        await CallDdei(DdeiClientRequestFactory.CreateVerifyCmsAuthRequest(arg));

    public virtual async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCaseIdOnlyArgDto arg)
    {
        var response = await CallDdei<DdeiCaseIdentifiersDto>(DdeiClientRequestFactory.CreateUrnLookupRequest(arg));

        return CaseIdentifiersMapper.MapCaseIdentifiers(response);
    }

    public virtual async Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArgDto arg)
    {
        var caseIdentifiers = await ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseInternalAsync(CaseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => CaseDetailsMapper.MapCaseDetails(@case));
    }

    public virtual async Task<CaseDto> GetCaseAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseInternalAsync(arg);
        return CaseDetailsMapper.MapCaseDetails(@case);
    }

    public virtual async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallDdeiLog<IEnumerable<DdeiPcdRequestCoreDto>>(DdeiClientRequestFactory.CreateGetPcdRequestsRequest(arg));
        var request = DdeiClientRequestFactory.CreateGetPcdRequestsRequest(arg);
        return CaseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
    }

    public virtual async Task<PcdRequestDto> GetPcdRequestAsync(DdeiPcdArgDto arg)
    {
        var pcdRequest = await CallDdei<DdeiPcdRequestDto>(DdeiClientRequestFactory.CreateGetPcdRequest(arg));
        return CaseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
    }

    public virtual async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var response = await CallDdei(DdeiClientRequestFactory.CreateGetDefendantAndChargesRequest(arg));
        var content = await response.Content.ReadAsStringAsync();
        var defendantAndCharges = JsonConvertWrapper.DeserializeObject<IEnumerable<DdeiCaseDefendantDto>>(content);
        var etag = response.Headers.ETag?.Tag;

        return CaseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
    }

    public virtual async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiLog<List<DdeiDocumentResponse>>(
            DdeiClientRequestFactory.CreateListCaseDocumentsRequest(arg)
        );

        return ddeiResults
            .Select(ddeiResult => CaseDocumentMapper.Map(ddeiResult));
    }

    public virtual async Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdei(DdeiClientRequestFactory.CreateGetDocumentRequest(arg));
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(),
            FileName = fileName
        };
    }

    public virtual async Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId)
    {
        var response = await CallDdei(
            DdeiClientRequestFactory.CreateDocumentFromFileStoreRequest(new DdeiFileStoreArgDto
            {
                Path = path,
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            })
        );

        return await response.Content.ReadAsStreamAsync();
    }

    public virtual async Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdei(
            DdeiClientRequestFactory.CreateCheckoutDocumentRequest(arg),
            HttpStatusCode.Conflict);

        return response.StatusCode == HttpStatusCode.Conflict ?
            new CheckoutDocumentDto
            {
                IsSuccess = false,
                LockingUserName = await response.Content.ReadAsStringAsync()
            } :
            new CheckoutDocumentDto
            {
                IsSuccess = true
            };
    }

    public virtual async Task CancelCheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        await CallDdei(DdeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg));
    }

    public virtual async Task<HttpResponseMessage> UploadPdfAsync(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream)
    {
        return await CallDdei(DdeiClientRequestFactory.CreateUploadPdfRequest(arg, stream),
        [
            HttpStatusCode.Gone,
            HttpStatusCode.RequestEntityTooLarge
        ]);
    }

    public virtual async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(DdeiDocumentArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiDocumentNoteResponse>>(DdeiClientRequestFactory.CreateGetDocumentNotesRequest(arg));

        return ddeiResults.Select(ddeiResult => CaseDocumentNoteMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<DocumentNoteResult> AddDocumentNoteAsync(DdeiAddDocumentNoteArgDto arg)
    {
        var response = await CallDdei<DdeiDocumentNoteAddedResponse>(DdeiClientRequestFactory.CreateAddDocumentNoteRequest(arg));

        return CaseDocumentNoteResultMapper.Map(response);
    }

    public virtual async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdei<RenameMaterialResponse>(DdeiClientRequestFactory.CreateRenameDocumentRequest(arg));

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }

    public virtual async Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg)
    {
        var response = await CallDdei<DdeiDocumentReclassifiedResponse>(DdeiClientRequestFactory.CreateReclassifyDocumentRequest(arg));

        return new DocumentReclassifiedResultDto
        {
            DocumentId = response.Id,
            DocumentTypeId = response.DocumentTypeId,
            ReclassificationType = response.ReclassificationType,
            OriginalDocumentTypeId = response.OriginalDocumentTypeId,
            DocumentRenamed = response.DocumentRenamed,
            DocumentRenamedOperationName = response.DocumentRenamedOperationName
        };
    }

    public virtual async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiDocumentExhibitProducerResponse>>(DdeiClientRequestFactory.CreateGetExhibitProducersRequest(arg));

        return ddeiResults.Select(ddeiResult => CaseExhibitProducerMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<IEnumerable<CaseWitnessDto>> GetWitnessesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiCaseWitnessResponse>>(DdeiClientRequestFactory.CreateCaseWitnessesRequest(arg));

        return ddeiResults.Select(ddeiResult => CaseWitnessMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiMaterialTypeListResponse>>(DdeiClientRequestFactory.CreateGetMaterialTypeListRequest(arg));

        return ddeiResults.Select(ddeiResult => CmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiWitnessStatementsArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiCaseWitnessStatementsResponse>>(DdeiClientRequestFactory.CreateGetWitnessStatementsRequest(arg));

        return ddeiResults.Select(ddeiResult => CaseWitnessStatementMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<bool> ToggleIsUnusedDocumentAsync(DdeiToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto) =>
        (await CallDdei(DdeiClientRequestFactory.CreateToggleIsUnusedDocumentRequest(toggleIsUnusedDocumentDto)))
        .IsSuccessStatusCode;

    protected virtual async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiUrnArgDto arg) =>
        await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(DdeiClientRequestFactory.CreateListCasesRequest(arg));

    protected virtual async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCaseIdentifiersArgDto arg) =>
        await CallDdei<DdeiCaseDetailsDto>(DdeiClientRequestFactory.CreateGetCaseRequest(arg));

    protected virtual async Task<T> CallDdei<T>(HttpRequestMessage request)
    {
        using var response = await CallDdei(request);
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvertWrapper.DeserializeObject<T>(content);
    }

    protected virtual async Task<T> CallDdeiLog<T>(HttpRequestMessage request)
    {
        using var response = await CallDdeiLog(request);
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvertWrapper.DeserializeObject<T>(content);
    }

    protected virtual async Task<HttpResponseMessage> CallDdei(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var response = await HttpClient.SendAsync(request);
        try
        {
            if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(content);
        }
        catch (HttpRequestException exception)
        {
            throw new DdeiClientException(response.StatusCode, exception);
        }
    }

    protected virtual async Task<HttpResponseMessage> CallDdeiLog(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            _telemetryClient.TrackEvent(new TestEvent { ErrorMessage = "HTTP client call debug log: About to call await HttpClient.SendAsync" });
            response = await HttpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackEvent(new TestEvent { ErrorMessage = $"HTTP client call debug log: An exception was thrown in CallDdeiLog - MESSAGE {ex.Message} - INNER EXCEPTION {ex.InnerException} - SOURCE {ex.Source} - TOSTRING {ex.ToString()}" });
        }

        var sb = new StringBuilder();
        foreach (var header in response.RequestMessage.Headers)
        {
            sb.Append($"Header key: {header.Key} - Header value: {header.Value}");
        }
        _telemetryClient.TrackEvent(new TestEvent { ErrorMessage = $"HTTP client call debug log: HEADERS: {sb.ToString()}" });
        _telemetryClient.TrackEvent(new TestEvent { ErrorMessage = $"HTTP client call debug log: STATUS CODE: {response.StatusCode}" });
        _telemetryClient.TrackEvent(new TestEvent { ErrorMessage = $"HTTP client call debug log: REASON PHRASE: {response.ReasonPhrase}" });
        _telemetryClient.TrackEvent(new TestEvent { ErrorMessage = $"HTTP client call debug log: RESPONSE CONTENT: {response.Content.ToString()}" });

        return response;
    }
}

public class TestEvent : BaseTelemetryEvent
{
    public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
    {
        return (
    new Dictionary<string, string>
    {
                    { nameof(ErrorMessage), ErrorMessage },
    },
    new Dictionary<string, double?>
    {
                    { "This is unknown", 0 },
    }
);
    }
}