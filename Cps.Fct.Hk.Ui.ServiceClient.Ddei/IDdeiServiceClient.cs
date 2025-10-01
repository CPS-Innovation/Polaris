// <copyright file="IDdeiServiceClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei;

using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Interfaces.Model.PCD;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model.Requests;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model.Requests.PcdRequests;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The interface for the DDEI API service client.
/// </summary>
public interface IDdeiServiceClient
{
    /// <summary>
    /// Asynchronously retrieves the CMS modern token based on the provided request.
    /// </summary>
    /// <param name="request">The request containing necessary parameters to obtain the CMS modern token.</param>
    /// <returns>A task representing the asynchronous operation, which returns the CMS modern token as a <see cref="string"/> if successful; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    Task<string?> GetCmsModernTokenAsync(GetCmsModernTokenRequest request);

    /// <summary>
    /// Asynchronously retrieves the summary of a case from CMS.
    /// </summary>
    /// <param name="request">The request containing the case ID and any required authentication parameters.</param>
    /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
    /// <returns>A task representing the asynchronous operation, which returns the <see cref="CaseSummary"/> of the case, or <c>null</c> if the case is not found or data is unavailable.</returns>
    /// <remarks>
    /// If the case summary is not found, the method returns <c>null</c>. The caller should handle the <c>null</c> return value accordingly.
    /// </remarks>
    Task<CaseSummary?> GetCaseSummaryAsync(GetCaseSummaryRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the communications for a case identifier.
    /// </summary>
    /// <param name="request">The request to list communications.</param>
    /// <returns>A list of communications for a case identifier.</returns>
    Task<IReadOnlyCollection<Communication>> ListCommunicationsHkAsync(ListCommunicationsHkRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the used statements for a case identifier.
    /// </summary>
    /// <param name="request">The request to list used statements.</param>
    /// <returns>A list of used statements for a case identifier.</returns>
    Task<UsedStatementsResponse> GetUsedStatementsAsync(GetUsedStatementsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the used exhibits for a case identifier.
    /// </summary>
    /// <param name="request">The request to list used exhibits.</param>
    /// <returns>A list of used exhibits for a case identifier.</returns>
    Task<UsedExhibitsResponse> GetUsedExhibitsAsync(GetUsedExhibitsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the used MG forms for a case identifier.
    /// </summary>
    /// <param name="request">The request to list used MG forms.</param>
    /// <returns>A list of used MG forms for a case identifier.</returns>
    Task<UsedMgFormsResponse> GetUsedMgFormsAsync(GetUsedMgFormsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the used other materials for a case identifier.
    /// </summary>
    /// <param name="request">The request to list used other materials.</param>
    /// <returns>A list of used other materials for a case identifier.</returns>
    Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(GetUsedOtherMaterialsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the unused materials for a case identifier.
    /// </summary>
    /// <param name="request">The request to list unused materials.</param>
    /// <returns>A list of unused materials for a case identifier.</returns>
    Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(GetUnusedMaterialsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously retrieves the material document for the specified request.
    /// </summary>
    /// <param name="request">The request containing the document parameters.</param>
    /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
    /// <returns>A task representing the asynchronous operation, which returns a <see cref="FileStreamResult"/> if the document is found; otherwise, <c>null</c>.</returns>
    Task<FileStreamResult?> GetMaterialDocumentAsync(GetDocumentRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously retrieves the attachments for a given communication identifier.
    /// </summary>
    /// <param name="request">The request containing the parameters required to fetch the attachments.</param>
    /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
    /// <returns>A task representing the asynchronous operation, which returns the attachments response.</returns>
    Task<AttachmentsResponse> GetAttachmentsAsync(GetAttachmentsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Reclassifies a communication for a material identifier.
    /// </summary>
    /// <param name="request">The request to list communications.</param>
    /// <returns></returns>
    Task<ReclassificationResponse> ReclassifyCommunicationAsync(ReclassifyCommunicationRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously renames a material name using material id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cmsAuthValues"></param>
    /// <returns></returns>
    Task<RenameMaterialResponse> RenameMaterialAsync(RenameMaterialRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get PCD Requests core/basic info by case id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cmsAuthValues"></param>
    /// <returns></returns>
    Task<List<PcdRequestCore>> GetPcdRequestCoreAsync(GetPcdRequestsCoreRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get PCD Requests overview by case id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cmsAuthValues"></param>
    /// <returns></returns>
    Task<List<PcdRequestDto>> GetPcdRequestOverviewAsync(GetPcdRequestsOverviewRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get PCD request overview by case id and PCD id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cmsAuthValues"></param>
    /// <returns></returns>
    Task<PcdRequestDto> GetPcdRequestByPcdIdAsync(GetPcdRequestByPcdIdCoreRequest request , CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Sets Material read or unread status using material id..
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cmsAuthValues"></param>
    /// <returns></returns>
    Task<SetMaterialReadStatusResponse> SetMaterialReadStatusAsync(SetMaterialReadStatusRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously discards a material using material id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cmsAuthValues"></param>
    /// <returns></returns>
    Task<DiscardMaterialResponse> DiscardMaterialAsync(DiscardMaterialRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Authenticates a user against the DDEI API.
    /// </summary>
    /// <param name="request">The authentication request.</param>
    /// <returns>The authentication information required to be passed to a client as cookie or header fields.</returns>
    Task<AuthenticationContext> AuthenticateAsync(AuthenticationRequest request);

    /// <summary>
    /// Gets witnesses belonging to the case.
    /// </summary>
    /// <returns></returns>
    Task<WitnessesResponse> GetWitnessesForCaseAsync(GetWitnessesForCaseRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Gets statements for a witness.
    /// </summary>
    Task<StatementsForWitnessResponse> GetStatementsForWitnessAsync(GetStatementsForWitnessRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Lists the defendants for a case identifier.
    /// </summary>
    /// <param name="request">The request to list defendants.</param>
    /// <returns>A list of defendants for a case identifier.</returns>
    Task<DefendantsResponse?> ListCaseDefendantsAsync(ListCaseDefendantsRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Gets exhibit producers for a given case Id.
    /// </summary>
    /// <returns>A list of exhibit producers</returns>
    Task<ExhibitProducersResponse> GetExhibitProducersAsync(GetExhibitProducersRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Adds a case action plan for a given case Id.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="NoContentResult"/> indicating success without a specific content body.</returns>
    Task<NoContentResult> AddCaseActionPlanAsync(int caseId, AddActionPlanRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously adds a new case witness.
    /// </summary>
    /// <param name="request">The request to add a witness.</param>
    /// <param name="cmsAuthValues"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="NoContentResult"/> indicating success without a specific content body.</returns>
    Task<NoContentResult> AddWitnessAsync(AddWitnessRequest request, CmsAuthValues cmsAuthValues);
     
    /// <summary>
    /// Asynchronously check case lock status.
    /// </summary>
    /// <param name="caseId">The ID of the case to be unlocked.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<CaseLockedStatusResult> CheckCaseLockAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously updates statemenet.
    /// </summary>
    /// <param name="request">The statement request.</param>
    /// <param name="cmsAuthValues"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<UpdateStatementResponse> UpdateStatementAsync(Model.UpdateStatementRequest request, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously updates exhibit.
    /// </summary>
    /// <param name="request">The exhibit request.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<UpdateExhibitResponse> UpdateExhibitAsync(Model.UpdateExhibitRequest request, CmsAuthValues cmsAuthValues);
}
