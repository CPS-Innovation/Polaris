// <copyright file="MdsClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Clients;

using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Extensions;
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
using DdeiClient.Domain.Response;
using DdeiClient.Domain.Response.Document;
using DdeiClient.Factories;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;

public class MdsClient : BaseCmsClient, IMdsClient
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IMdsClientRequestFactory mdsClientRequestFactory;
    private readonly ICaseDetailsMapper caseDetailsMapper;
    private readonly ICaseDocumentMapper<MdsDocumentResponse> caseDocumentMapper;
    private readonly ICaseDocumentNoteMapper caseDocumentNoteMapper;
    private readonly ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper;
    private readonly ICaseExhibitProducerMapper caseExhibitProducerMapper;
    private readonly ICaseIdentifiersMapper caseIdentifiersMapper;
    private readonly ICmsMaterialTypeMapper cmsMaterialTypeMapper;
    private readonly ICaseWitnessStatementMapper caseWitnessStatementMapper;
    private readonly IJsonConvertWrapper jsonConvertWrapper;
    private readonly IMdsClientFactory mdsClientFactory;
    public MdsClient(
        IHttpClientFactory httpClientFactory,
        IMdsClientRequestFactory mdsClientRequestFactory,
        ICaseDetailsMapper caseDetailsMapper,
        ICaseDocumentMapper<MdsDocumentResponse> caseDocumentMapper,
        ICaseDocumentNoteMapper caseDocumentNoteMapper,
        ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper,
        ICaseExhibitProducerMapper caseExhibitProducerMapper,
        ICaseIdentifiersMapper caseIdentifiersMapper,
        ICmsMaterialTypeMapper cmsMaterialTypeMapper,
        ICaseWitnessStatementMapper caseWitnessStatementMapper,
        IMdsClientFactory mdsClientFactory,
        IJsonConvertWrapper jsonConvertWrapper)
        : base(jsonConvertWrapper)
    {
        this.httpClientFactory = httpClientFactory.ExceptionIfNull();
        this.mdsClientRequestFactory = mdsClientRequestFactory.ExceptionIfNull();
        this.caseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
        this.caseDocumentMapper = caseDocumentMapper.ExceptionIfNull();
        this.caseDocumentNoteMapper = caseDocumentNoteMapper.ExceptionIfNull();
        this.caseDocumentNoteResultMapper = caseDocumentNoteResultMapper.ExceptionIfNull();
        this.caseExhibitProducerMapper = caseExhibitProducerMapper.ExceptionIfNull();
        this.caseIdentifiersMapper = caseIdentifiersMapper.ExceptionIfNull();
        this.cmsMaterialTypeMapper = cmsMaterialTypeMapper.ExceptionIfNull();
        this.caseWitnessStatementMapper = caseWitnessStatementMapper.ExceptionIfNull();
        this.mdsClientFactory = mdsClientFactory.ExceptionIfNull();
        this.jsonConvertWrapper = jsonConvertWrapper.ExceptionIfNull();
    }

    public async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(MdsCaseIdOnlyArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsCaseIdentifiersDto = await this.CallHttpClientAsync<MdsCaseIdentifiersDto>(this.mdsClientRequestFactory.CreateUrnLookupRequest(arg), arg.CmsAuthValues, cancellationToken);

        return this.caseIdentifiersMapper.MapCaseIdentifiers(mdsCaseIdentifiersDto);
    }

    public async Task<CaseSummaryDto> GetCaseSummaryAsync(MdsCaseIdOnlyArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResult = await this.CallHttpClientAsync<MdsCaseSummaryDto>(this.mdsClientRequestFactory.CreateGetCaseSummary(arg), arg.CmsAuthValues, cancellationToken);
        return this.caseDetailsMapper.Map(mdsResult);
    }

    // calls /cases/{arg.CaseId}/pcd-requests/overview
    public async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var pcdRequests = await this.CallHttpClientAsync<IEnumerable<MdsPcdRequestCoreDto>>(this.mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues, cancellationToken);
        return this.caseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
    }

    // calls /cases/{arg.CaseId}/pcd-requests/overview
    public async Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var pcdRequests = await this.CallHttpClientAsync<IEnumerable<MdsPcdRequestDto>>(this.mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues, cancellationToken);
        return this.caseDetailsMapper.MapPreChargeDecisionRequests(pcdRequests);
    }

    // calls /cases/{arg.CaseId}/pcd-request/{arg.PcdId}
    public async Task<PcdRequestDto> GetPcdRequestAsync(MdsPcdArgDto arg, CancellationToken cancellationToken = default)
    {
        var pcdRequest = await this.CallHttpClientAsync<MdsPcdRequestDto>(this.mdsClientRequestFactory.CreateGetPcdRequest(arg), arg.CmsAuthValues, cancellationToken);
        return this.caseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
    }

    // calls /cases/{caseId}/defendants
    public async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await this.CallHttpClientAsync(this.mdsClientRequestFactory.CreateGetDefendantAndChargesRequest(arg), arg.CmsAuthValues, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var defendantAndCharges = this.jsonConvertWrapper.DeserializeObject<IEnumerable<MdsCaseDefendantDto>>(content);
        var etag = response.Headers.ETag?.Tag;

        return this.caseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
    }

    public async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await this.CallHttpClientAsync<List<MdsDocumentResponse>>(this.mdsClientRequestFactory.CreateListCaseDocumentsRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.Select(mdsResult => this.caseDocumentMapper.Map(mdsResult));
    }

    public async Task<FileResult> GetDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await this.CallHttpClientAsync(this.mdsClientRequestFactory.CreateGetDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(cancellationToken),
            FileName = fileName,
        };
    }

    public async Task<CheckoutDocumentDto> CheckoutDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default)
    {
        await this.CallHttpClientAsync(
            this.mdsClientRequestFactory.CreateCheckoutDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);

        return new CheckoutDocumentDto { IsSuccess = true };
    }

    public async Task CancelCheckoutDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default)
    {
        await this.CallHttpClientAsync(this.mdsClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);
    }

    public async Task<HttpResponseMessage> UploadPdfAsync(MdsMaterialIdAndDocumentIdArgDto arg, Stream stream, CancellationToken cancellationToken = default)
    {
        return await this.CallHttpClientAsync(
            this.mdsClientRequestFactory.CreateUploadPdfRequest(arg, stream),
            arg.CmsAuthValues,
            cancellationToken,
            [
                HttpStatusCode.Gone,
                HttpStatusCode.RequestEntityTooLarge,
            ]);
    }

    public async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(MdsDocumentArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await this.CallHttpClientAsync<List<DocumentNoteResponse>>(this.mdsClientRequestFactory.CreateGetDocumentNotesRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.Select(mdsResult => this.caseDocumentNoteMapper.Map(mdsResult)).ToArray();
    }

    public async Task<DocumentNoteResult> AddDocumentNoteAsync(MdsAddDocumentNoteArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await this.CallHttpClientAsync<MdsDocumentNoteAddedResponse>(this.mdsClientRequestFactory.CreateAddDocumentNoteRequest(arg), arg.CmsAuthValues, cancellationToken);

        return this.caseDocumentNoteResultMapper.Map(response);
    }

    public async Task<DocumentRenamedResultDto> RenameDocumentAsync(MdsRenameDocumentArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await this.CallHttpClientAsync<RenameMaterialResponse>(this.mdsClientRequestFactory.CreateRenameDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }

    public async Task<DocumentRenamedResultDto> RenameExhibitAsync(MdsRenameDocumentArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await this.CallHttpClientAsync<RenameMaterialDescriptionResponse>(this.mdsClientRequestFactory.CreateRenameExhibitRequest(arg), arg.CmsAuthValues, cancellationToken);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunicationDescription.Id };
    }

    public async Task<MdsCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(MdsReclassifyCommunicationArgDto arg, CancellationToken cancellationToken = default)
    {
        return await this.CallHttpClientAsync<MdsCommunicationReclassifiedResponse>(this.mdsClientRequestFactory.CreateReclassifyCommunicationRequest(arg), arg.CmsAuthValues, cancellationToken);
    }

    public async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await this.CallHttpClientAsync<MdsDocumentExhibitProducerResponse>(this.mdsClientRequestFactory.CreateGetExhibitProducersRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.ExhibitProducers.Select(this.caseExhibitProducerMapper.Map);
    }

    public async Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await this.CallHttpClientAsync<List<MdsCaseWitnessResponse>>(this.mdsClientRequestFactory.CreateCaseWitnessesRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults;
    }

    public async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(CmsBaseArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await this.CallHttpClientAsync<List<MdsMaterialTypeListResponse>>(this.mdsClientRequestFactory.CreateGetMaterialTypeListRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.Select(mdsResult => this.cmsMaterialTypeMapper.Map(mdsResult)).ToArray();
    }

    public async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(MdsWitnessStatementsArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await this.CallHttpClientAsync<StatementsForWitnessResponse>(this.mdsClientRequestFactory.CreateGetWitnessStatementsRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.StatementsForWitness.Select(this.caseWitnessStatementMapper.Map).ToArray();
    }

    public async Task<bool> ToggleIsUnusedDocumentAsync(MdsToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto, CancellationToken cancellationToken = default) =>
        (await this.CallHttpClientAsync(this.mdsClientRequestFactory.CreateToggleIsUnusedDocumentRequest(toggleIsUnusedDocumentDto), toggleIsUnusedDocumentDto.CmsAuthValues, cancellationToken))
        .IsSuccessStatusCode;

    public async Task<IEnumerable<MdsCaseIdentifiersDto>> ListCaseIdsAsync(MdsUrnArgDto arg, CancellationToken cancellationToken = default) =>
        await this.CallHttpClientAsync<IEnumerable<MdsCaseIdentifiersDto>>(this.mdsClientRequestFactory.CreateListCasesRequest(arg), arg.CmsAuthValues, cancellationToken);

    private async Task<MdsCaseDetailsDto> GetCaseInternalAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default) =>
        await this.CallHttpClientAsync<MdsCaseDetailsDto>(this.mdsClientRequestFactory.CreateGetCaseRequest(arg), arg.CmsAuthValues, cancellationToken);

    protected override HttpClient GetHttpClient(string cmsAuthValues)
    {
        var mdsClientName = this.mdsClientFactory.Create(cmsAuthValues);
        return this.httpClientFactory.CreateClient(mdsClientName);
    }
}
