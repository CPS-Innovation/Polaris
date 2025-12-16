// <copyright file="ICommunicationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Cps.Fct.Hk.Ui.Interfaces.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Dto.Request.HouseKeeping;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;
using Common.Enums;

/// <summary>
/// Interface for communication service that provides methods to retrieve and log communications.
/// </summary>
public interface ICommunicationService
{
    /// <summary>
    /// Retrieves a collection of communications for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which communications are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve communications.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only collection of <see cref="Communication"/> objects.</returns>
    Task<IReadOnlyCollection<Communication>> GetCommunicationsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves a collection of used statements for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which used statements are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the used statements.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UsedStatementsResponse"/> object, which includes details of the used statements.</returns>
    Task<UsedStatementsResponse> GetUsedStatementsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves a collection of used exhibits for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which used exhibits are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the used exhibits.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UsedExhibitsResponse"/> object, which includes details of the used exhibits.</returns>
    Task<UsedExhibitsResponse> GetUsedExhibitsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves a collection of used MG forms for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which used MG forms are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the used MG forms.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UsedMgFormsResponse"/> object, which includes details of the used MG forms.</returns>
    Task<UsedMgFormsResponse> GetUsedMgFormsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves a collection of used other materials for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which used other materials are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the used other materials.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UsedOtherMaterialsResponse"/> object, which includes details of the used other materials.</returns>
    Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves a collection of unused materials for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which unused materials are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the unused materials.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UnusedMaterialsResponse"/> object, which includes details of the unused materials.</returns>
    Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves a collection of attachments for a specific communication.
    /// </summary>
    /// <param name="communicationId">The unique identifier of the communication for which attachments are being retrieved.</param>
    /// <param name="communicationSubject">The subject of the communication for which attachments are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the unused materials.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="AttachmentsResponse"/> object, which includes attachments of the communication.</returns>
    Task<AttachmentsResponse> GetAttachmentsAsync(int communicationId, string communicationSubject, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves all attachments for a collection of communications.
    /// </summary>
    /// <param name="communications">The collection of communications to obtain attachments from.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the unused materials.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Attachment"/> objects.</returns>
    Task<List<Attachment>> RetrieveAllAttachmentsAsync(IReadOnlyCollection<Communication> communications, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Maps a list of attachments to a list of <see cref="Communication"/> objects.
    /// </summary>
    /// <param name="attachments">The list of attachments to map.</param>
    /// <returns>A list of <see cref="Communication"/> objects, where each object represents a mapped attachment.</returns>
    List<Communication> MapAttachmentsToCommunications(List<Attachment> attachments);

    /// <summary>
    /// Asynchronously retrieves the link for a specific material based on the material ID.
    /// </summary>
    /// <param name="caseId">The ID of the case from which communications are retrieved.</param>
    /// <param name="materialId">The ID of the material to find the corresponding case material document link.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <returns>An object that contains the communication link if found, or a result indicating that no communication was found.</returns>
    Task<object> GetCaseMaterialLinkAsync(int caseId, int materialId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Retrieves list of exhibit producers required as part of exhibit reclassification for a given case Id.
    /// </summary>
    /// <param name="caseId">The unique ID of the case.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<ExhibitProducersResponse> GetExhibitProducersAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously rename a material name with given material ID.
    /// </summary>
    /// <param name="caseId">The ID of the case from which communications are retrieved.</param>
    /// <param name="materialId">The ID of material to be renamed.</param>
    /// <param name="subject">New material name to be changed with.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="correspondenceId">correspondenceId.</param>
    /// <returns>An object that contains the renamed material id.</returns>
    Task<RenameMaterialResponse> RenameMaterialAsync(int caseId, int materialId, string subject, CmsAuthValues cmsAuthValues, Guid correspondenceId = default);

    /// <summary>
    /// Get PCD requests core info by case Id.
    /// </summary>
    /// <param name="caseId">Get PCD requests by case id.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <returns>Return collection of PCD requests.</returns>
    Task<IReadOnlyCollection<PcdRequestCore>> GetPcdRequestCore(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get PCD Request overview by case Id Pcd id.
    /// </summary>
    /// <param name="caseId">Get PCD requests by case id.</param>
    /// <param name="pcdId">Parameter to get PCD requests by PCD id.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <returns>Return single PCD request information by PCD id.</returns>
    Task<PcdRequestDto> GetPcdRequestByPcdIdAsync(int caseId, int pcdId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Asynchronously discard a material name with given material ID.
    /// </summary>
    /// <param name="caseId">The ID of the case from which communications are retrieved.</param>
    /// <param name="materialId">The ID of material to be renamed.</param>
    /// <param name="discardReason">Discard reason code for the material.</param>
    /// <param name="discardReasonDescription">Discard reason description for the material.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="correspondenceId">correspondenceId.</param>
    /// <returns>An object that contains the discarded material id.</returns>
    Task<DiscardMaterialResponse> DiscardMaterialAsync(int caseId, int materialId, string discardReason, string discardReasonDescription, CmsAuthValues cmsAuthValues, Guid correspondenceId = default);

    /// <summary>
    /// Asynchronously sets material read or unread status..
    /// </summary>
    /// <param name="materialId">The ID of material to be renamed.</param>
    /// <param name="action">Material read or unread status.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="correspondenceId">correspondenceId.</param>
    /// <returns>An object that contains the renamed material id.</returns>
    Task<SetMaterialReadStatusResponse> SetMaterialReadStatusAsync(int materialId, MaterialReadStatusType action, CmsAuthValues cmsAuthValues, Guid correspondenceId = default);

    /// <summary>
    /// Updates exhibit given an exhibit request.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="exhibit">The update exhibit request model.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="correspondenceId">correspondenceId.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<UpdateExhibitResponse> UpdateExhibitAsync(int caseId, UpdateExhibitRequest exhibit, CmsAuthValues cmsAuthValues, Guid correspondenceId = default);

    /// <summary>
    /// Update statement given a statement request.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="statement">The update statement request.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="correspondenceId">correspondenceId.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<UpdateStatementResponse> UpdateStatementAsync(int caseId, UpdateStatementRequest statement, CmsAuthValues cmsAuthValues, Guid correspondenceId = default);

    /// <summary>
    /// Get First Initial Review Get Case History.
    /// </summary>
    /// <param name="caseId">The caseId.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ApiClient.PreChargeDecisionAnalysisOutcome> FirstInitialReviewGetCaseHistoryAsync(int caseId, CmsAuthValues cmsAuthValues);


    /// <summary>
    /// Get Initial Review ByHistoryId.
    /// </summary>
    /// <param name="caseId">Case id.</param>
    /// <param name="historyId">History Id.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ApiClient.PreChargeDecisionAnalysisOutcome> GetInitialReviewByHistoryIdAsync(int caseId, int historyId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// GetHistoryEvents.
    /// </summary>
    /// <param name="caseId">The caseId.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ICollection<ApiClient.HistoryEvent>> GetHistoryEventsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get Offence Charge.
    /// </summary>
    /// <param name="caseId">The caseId.</param>
    /// <param name="historyId">History Id.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ApiClient.OffenceChangeResponse> GetOffenceChargeByHistoryIdAsync(int caseId, int historyId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get Pre Charge Decision Case History Event Details.
    /// </summary>
    /// <param name="caseId">Case Id.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ApiClient.PreChargeDecisionOutcome> GetPreChargeDecisionCaseHistoryEventDetailsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get Pre Charge Decision By History Id.
    /// </summary>
    /// <param name="caseId">Case Id.</param>
    /// <param name="historyId">History Id.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ApiClient.PreChargeDecisionOutcome> GetPreChargeDecisionByHistoryId(int caseId, int historyId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Get Pcd review.
    /// </summary>
    /// <param name="caseId">The case id.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string containing the lock release status.</returns>
    Task<ApiClient.PcdReviewData> GetPcdReview(int caseId, CmsAuthValues cmsAuthValues);
}
