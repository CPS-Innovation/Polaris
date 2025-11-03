// <copyright file="IMdsClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The CrownProsecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Clients.Interfaces
{
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Common.Dto.Response.HouseKeeping;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Mds client.
    /// </summary>
    public interface IMasterDataServiceClient
    {
        /// <summary>
        /// Asynchronously retrieves the summary of a case from CMS.
        /// </summary>
        /// <param name="request">The request containing the case ID and any required authentication parameters.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A task representing the asynchronous operation, which returns the <see cref="CaseSummary"/> of the case, or <c>null</c> if the case is not found or data is unavailable.</returns>
        /// <remarks>
        /// If the case summary is not found, the method returns <c>null</c>. The caller should handle the <c>null</c> return value accordingly.
        /// </remarks>
        Task<CaseSummaryResponse> GetCaseSummaryAsync(GetCaseSummaryRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Lists the communications for a case identifier.
        /// </summary>
        /// <param name="request">The request to list communications.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of communications for a case identifier.</returns>
        Task<IReadOnlyCollection<Communication>> ListCommunicationsHkAsync(ListCommunicationsHkRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Lists the used statements for a case identifier.
        /// </summary>
        /// <param name="request">The request to list used statements.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of used statements for a case identifier.</returns>
        Task<UsedStatementsResponse> GetUsedStatementsAsync(GetUsedStatementsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Lists the used exhibits for a case identifier.
        /// </summary>
        /// <param name="request">The request to list used exhibits.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of used exhibits for a case identifier.</returns>
        Task<UsedExhibitsResponse> GetUsedExhibitsAsync(GetUsedExhibitsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Lists the used MG forms for a case identifier.
        /// </summary>
        /// <param name="request">The request to list used MG forms.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of used MG forms for a case identifier.</returns>
        Task<UsedMgFormsResponse> GetUsedMgFormsAsync(GetUsedMgFormsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Lists the used other materials for a case identifier.
        /// </summary>
        /// <param name="request">The request to list used other materials.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of used other materials for a case identifier.</returns>
        Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(GetUsedOtherMaterialsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Lists the unused materials for a case identifier.
        /// </summary>
        /// <param name="request">The request to list unused materials.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of unused materials for a case identifier.</returns>
        Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(GetUnusedMaterialsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Gets exhibit producers for a given case Id.
        /// </summary>
        /// <param name="request">The request for exhibit producers.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of exhibit producers</returns>
        Task<ExhibitProducersResponse> GetExhibitProducersAsync(GetExhibitProducersRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Asynchronously retrieves the attachments for a given communication identifier.
        /// </summary>
        /// <param name="request">The request containing the parameters required to fetch the attachments.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A task representing the asynchronous operation, which returns the attachments response.</returns>
        Task<AttachmentsResponse> GetAttachmentsAsync(GetAttachmentsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Asynchronously retrieves the material document for the specified request.
        /// </summary>
        /// <param name="request">The request containing the document parameters.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A task representing the asynchronous operation, which returns a <see cref="FileStreamResult"/> if the document is found; otherwise, <c>null</c>.</returns>
        Task<FileStreamResult?> GetMaterialDocumentAsync(GetDocumentRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Gets case defendants.
        /// </summary>
        /// <param name="request">The request to list defendants.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A list of defendants for a case identifier.</returns>
        Task<DefendantsResponse?> GetCaseDefendantsAsync(ListCaseDefendantsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Gets witnesses belonging to the case.
        /// </summary>
        /// <param name="request">GetWitnessRequest.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>List of witnesses.</returns>
        Task<WitnessesResponse> GetCaseWitnessesAsync(GetCaseWitnessesRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Gets statements for a witness.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<WitnessStatementsResponse> GetWitnessStatementsAsync(GetWitnessStatementsRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Adds a case action plan for a given case Id.
        /// </summary>
        /// <param name="caseId">The case Id.</param>
        /// <param name="request">The request object.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="NoContentResult"/> indicating success without a specific content body.</returns>
        Task<NoContentResult> AddCaseActionPlanAsync(int caseId, AddActionPlanRequest request, CmsAuthValues cmsAuthValues);

        /// <summary>
        /// Asynchronously adds a new case witness.
        /// </summary>
        /// <param name="request">The request to add a witness.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="NoContentResult"/> indicating success without a specific content body.</returns>
        Task<NoContentResult> AddWitnessAsync(AddWitnessRequest request, CmsAuthValues cmsAuthValues);
    }
}
