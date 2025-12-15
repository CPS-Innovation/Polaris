// <copyright file="CommunicationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Response.HouseKeeping.Pcd;
using Cps.Fct.Hk.Ui.Interfaces;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;

/// <summary>
/// Provides services for processing and retrieving communications related to a case.
/// </summary>
public class CommunicationService(
    ILogger<CommunicationService> logger,
    IMasterDataServiceClient apiClient,
    IDocumentTypeMapper documentTypeMapper,
    ICommunicationMapper communicationMapper)
    : ICommunicationService
{
    private readonly ILogger<CommunicationService> logger = logger;
    private readonly IMasterDataServiceClient apiClient = apiClient;
    private readonly IDocumentTypeMapper documentTypeMapper = documentTypeMapper;
    private readonly ICommunicationMapper communicationMapper = communicationMapper;

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Communication>> GetCommunicationsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching inbox communications for caseId [{caseIdString}] ...");

            var request = new ListCommunicationsHkRequest(caseId, Guid.NewGuid());
            IReadOnlyCollection<Communication> communications = await this.apiClient.ListCommunicationsHkAsync(request, cmsAuthValues).ConfigureAwait(false);

            // Map the document type to each communication
            var mappedCommunications = communications.Select(c =>
            {
                // Map the DocumentTypeId to a DocumentTypeInfo object
                DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapDocumentType(c.DocumentTypeId);

                return new Communication(
                    c.Id,
                    c.OriginalFileName,
                    c.Subject,
                    c.DocumentTypeId,
                    c.MaterialId,
                    c.Link,
                    c.Status,
                    documentTypeInfo.Category,
                    documentTypeInfo.DocumentType,
                    c.HasAttachments,
                    this.communicationMapper.MapCommunicationMethod(c.Method),
                    this.communicationMapper.MapDirection(c.Direction),
                    this.communicationMapper.MapCommunicationParty(c.Party),
                    c.Date);
            }).ToList();

            return mappedCommunications;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching communications for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UsedStatementsResponse> GetUsedStatementsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching used statements for caseId [{caseIdString}] ...");

            var request = new GetUsedStatementsRequest(caseId, Guid.NewGuid());
            UsedStatementsResponse usedStatements = await this.apiClient.GetUsedStatementsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return usedStatements;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used statements for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UsedExhibitsResponse> GetUsedExhibitsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching used exhibits for caseId [{caseIdString}] ...");

            var request = new GetUsedExhibitsRequest(caseId, Guid.NewGuid());
            UsedExhibitsResponse usedExhibits = await this.apiClient.GetUsedExhibitsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return usedExhibits;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used exhibits for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UsedMgFormsResponse> GetUsedMgFormsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching used MG forms for caseId [{caseIdString}] ...");

            var request = new GetUsedMgFormsRequest(caseId, Guid.NewGuid());
            UsedMgFormsResponse usedMgForms = await this.apiClient.GetUsedMgFormsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return usedMgForms;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used MG forms for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching used other materials for caseId [{caseIdString}] ...");

            var request = new GetUsedOtherMaterialsRequest(caseId, Guid.NewGuid());
            UsedOtherMaterialsResponse usedOtherMaterials = await this.apiClient.GetUsedOtherMaterialsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return usedOtherMaterials;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used other materials for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching unused materials for caseId [{caseIdString}] ...");

            var request = new GetUnusedMaterialsRequest(caseId, Guid.NewGuid());
            UnusedMaterialsResponse unusedMaterials = await this.apiClient.GetUnusedMaterialsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return unusedMaterials;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching unused materials for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AttachmentsResponse> GetAttachmentsAsync(int communicationId, string communicationSubject, CmsAuthValues cmsAuthValues)
    {
        string communicationIdString = communicationId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching attachments for communicationId [{communicationIdString}] ...");

            var request = new GetAttachmentsRequest(communicationId, Guid.NewGuid());
            AttachmentsResponse attachments = await this.apiClient.GetAttachmentsAsync(request, cmsAuthValues).ConfigureAwait(false);

            return attachments;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching attachments for communicationId [{communicationIdString}] with subject [{communicationSubject}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<Attachment>> RetrieveAllAttachmentsAsync(
        IReadOnlyCollection<Communication> communications,
        CmsAuthValues cmsAuthValues)
    {
        try
        {
            var allAttachments = new List<Attachment>();

            foreach (Communication communication in communications)
            {
                if (communication.HasAttachments)
                {
                    // Retrieve attachments for the current communication
                    AttachmentsResponse currentAttachments = await this.GetAttachmentsAsync(communication.Id, communication.Subject, cmsAuthValues).ConfigureAwait(false);

                    if (currentAttachments?.Attachments != null)
                    {
                        allAttachments.AddRange(currentAttachments.Attachments);
                    }

                    this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Communication Id [{communication.Id}] with attachments retrieved, subject [{communication.Subject}]");
                }
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Total attachments retrieved: {allAttachments.Count}");
            return allAttachments;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while trying to retrieve all attachments");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public List<Communication> MapAttachmentsToCommunications(List<Attachment> attachments)
    {
        try
        {
            return attachments.Select(att =>
            {
                // Map DocumentTypeId to a DocumentTypeInfo object
                DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapDocumentType(att.DocumentTypeId);

                return new Communication(
                    Id: 0, // Default or meaningful value
                    OriginalFileName: att.OriginalFileName ?? "Unknown File Name",
                    Subject: att.Name ?? "No Subject",
                    DocumentTypeId: att.DocumentTypeId,
                    MaterialId: att.MaterialId,
                    Link: att.Link ?? string.Empty,
                    Status: string.Empty,
                    Category: documentTypeInfo.Category,
                    Type: documentTypeInfo.DocumentType,
                    HasAttachments: false);
            }).ToList();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while mapping attachments to communications");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<object> GetCaseMaterialLinkAsync(int caseId, int materialId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            IActionResult GetNotFoundResult() =>
                new NotFoundObjectResult($"{LoggingConstants.HskUiLogPrefix} No case material document found for materialId [{materialId}]");

            // Fire off all tasks concurrently
            Task<IReadOnlyCollection<Communication>> communicationsTask = this.GetCommunicationsAsync(caseId, cmsAuthValues);
            Task<UnusedMaterialsResponse> unusedMaterialsTask = this.GetUnusedMaterialsAsync(caseId, cmsAuthValues);
            Task<UsedStatementsResponse> usedStatementsTask = this.GetUsedStatementsAsync(caseId, cmsAuthValues);
            Task<UsedExhibitsResponse> usedExhibitsTask = this.GetUsedExhibitsAsync(caseId, cmsAuthValues);
            Task<UsedMgFormsResponse> usedMgFormsTask = this.GetUsedMgFormsAsync(caseId, cmsAuthValues);
            Task<UsedOtherMaterialsResponse> usedOtherMaterialsTask = this.GetUsedOtherMaterialsAsync(caseId, cmsAuthValues);

            await Task
                .WhenAll(
                communicationsTask,
                unusedMaterialsTask,
                usedStatementsTask,
                usedExhibitsTask,
                usedMgFormsTask,
                usedOtherMaterialsTask)
                .ConfigureAwait(false);

            // Retrieve awaited results
            IReadOnlyCollection<Communication>? communications = await communicationsTask.ConfigureAwait(false);
            UnusedMaterialsResponse? unusedMaterials = await unusedMaterialsTask.ConfigureAwait(false);
            UsedStatementsResponse? usedStatements = await usedStatementsTask.ConfigureAwait(false);
            UsedExhibitsResponse? usedExhibits = await usedExhibitsTask.ConfigureAwait(false);
            UsedMgFormsResponse? usedMgForms = await usedMgFormsTask.ConfigureAwait(false);
            UsedOtherMaterialsResponse? usedOtherMaterials = await usedOtherMaterialsTask.ConfigureAwait(false);

            // Create a list of sources to search in
            var sources = new List<object?>
            {
                communications?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                usedStatements?.Statements?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                usedExhibits?.Exhibits?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                usedMgForms?.MgForms?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                usedOtherMaterials?.MgForms?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                unusedMaterials?.Exhibits?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                unusedMaterials?.MgForms?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                unusedMaterials?.OtherMaterials?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
                unusedMaterials?.Statements?.FirstOrDefault(c => c.MaterialId == materialId)?.Link,
            };

            // Return the first non-null link
            object? foundLink = sources.FirstOrDefault(link => link != null);
            if (foundLink != null)
            {
                return foundLink;
            }

            // If no matches, check communication attachments
            List<Attachment> allAttachments = await this.RetrieveAllAttachmentsAsync(
                communications ?? Array.Empty<Communication>(),
                cmsAuthValues).ConfigureAwait(false);

            if (allAttachments == null || allAttachments.Count == 0)
            {
                return GetNotFoundResult();
            }

            // Map Attachments to Communications and find the matching link
            List<Communication> attachmentCommunications = this.MapAttachmentsToCommunications(allAttachments);
            Communication? communicationAttachment = attachmentCommunications?.FirstOrDefault(c => c.MaterialId == materialId);

            return communicationAttachment?.Link != null
                ? communicationAttachment.Link
                : GetNotFoundResult();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while trying to get the link for materialId [{materialId}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ExhibitProducersResponse> GetExhibitProducersAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Getting exhibit producers for caseId [{caseIdString}]");

            var request = new GetExhibitProducersRequest(caseId, Guid.NewGuid());

            ExhibitProducersResponse? producers = await this.apiClient.GetExhibitProducersAsync(request, cmsAuthValues).ConfigureAwait(false);

            return producers;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching producers for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<RenameMaterialResponse> RenameMaterialAsync(int caseId, int materialId, string subject, CmsAuthValues cmsAuthValues, Guid correspondenceId = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to rename material with materialId [{materialId}]");

            var request = new RenameMaterialRequest(correspondenceId == default ? Guid.NewGuid() : correspondenceId, materialId, subject);
            RenameMaterialResponse renameMaterialResponse = await this.apiClient.RenameMaterialAsync(request, cmsAuthValues).ConfigureAwait(false);
            this.logger.LogInformation(LoggingConstants.RenameMaterialOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, materialId);

            return renameMaterialResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.RenameMaterialOperationFailed, LoggingConstants.HskUiLogPrefix, caseId, materialId);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<PcdRequestCore>> GetPcdRequestCore(int caseId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching unused materials for caseId [{caseIdString}] ...");
            Guid correlationId = cmsAuthValues.CorrelationId == Guid.Empty ? Guid.NewGuid() : cmsAuthValues.CorrelationId;

            var request = new GetPcdRequestsCoreRequest(caseId, correlationId);
            IReadOnlyCollection<PcdRequestCore> unusedMaterials = await this.apiClient.GetPcdRequestCoreAsync(request, cmsAuthValues).ConfigureAwait(false);

            return unusedMaterials;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching unused materials for caseId [{caseIdString}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PcdRequestDto> GetPcdRequestByPcdIdAsync(int caseId, int pcdId, CmsAuthValues cmsAuthValues)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Fetching PCD Request overview for caseId [{caseIdString}] and PCD ID [{pcdId}].");
            Guid correlationId = cmsAuthValues.CorrelationId == Guid.Empty ? Guid.NewGuid() : cmsAuthValues.CorrelationId;
            var request = new GetPcdRequestByPcdIdCoreRequest(caseId, pcdId, correlationId);
            PcdRequestDto unusedMaterials = await this.apiClient.GetPcdRequestByPcdIdAsync(request, cmsAuthValues).ConfigureAwait(false);

            return unusedMaterials;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching PCD Request overview for caseId [{caseIdString}] and PCD id [{pcdId}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<DiscardMaterialResponse> DiscardMaterialAsync(int caseId, int materialId, string discardReason, string discardReasonDescription, CmsAuthValues cmsAuthValues, Guid correspondenceId = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to discard material with materialId [{materialId}]");

            var request = new DiscardMaterialRequest(correspondenceId == default ? Guid.NewGuid() : correspondenceId, materialId, discardReason, discardReasonDescription);
            DiscardMaterialResponse discardMaterialResponse = await this.apiClient.DiscardMaterialAsync(request, cmsAuthValues).ConfigureAwait(false);
            this.logger.LogInformation(LoggingConstants.DiscardMaterialOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, materialId);

            return discardMaterialResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.DiscardMaterialOperationFailed, LoggingConstants.HskUiLogPrefix, caseId, materialId);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<UpdateExhibitResponse> UpdateExhibitAsync(int caseId, UpdateExhibitRequest exhibit, CmsAuthValues cmsAuthValues, Guid correspondenceId = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to update exhibit with materialId [{exhibit.MaterialId}]");

            var request = new UpdateExhibitRequest(
                correspondenceId == default ? Guid.NewGuid() : correspondenceId,
                caseId,
                exhibit.DocumentType,
                exhibit.Item,
                exhibit.MaterialId,
                exhibit.Reference,
                exhibit.Subject,
                exhibit.Used,
                exhibit.NewProducer,
                exhibit.ExistingProducerOrWitnessId);

            UpdateExhibitResponse response = await this.apiClient.UpdateExhibitAsync(request, cmsAuthValues).ConfigureAwait(false);
            this.logger.LogInformation(LoggingConstants.UpdateExhibitOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, exhibit.MaterialId);

            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.UpdateExhibitOperationFailed, LoggingConstants.HskUiLogPrefix, caseId, exhibit.MaterialId);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<UpdateStatementResponse> UpdateStatementAsync(int caseId, UpdateStatementRequest statement, CmsAuthValues cmsAuthValues, Guid correspondenceId = default)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Attempting to update statement with materialId [{statement.MaterialId}]");

            var request = new UpdateStatementRequest(
                correspondenceId == default ? Guid.NewGuid() : correspondenceId,
                caseId,
                statement.MaterialId,
                statement.WitnessId,
                statement.StatementDate,
                statement.StatementNumber,
                statement.Used);

            UpdateStatementResponse response = await this.apiClient.UpdateStatementAsync(request, cmsAuthValues).ConfigureAwait(false);
            this.logger.LogInformation(LoggingConstants.UpdateStatementOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, statement.MaterialId);

            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, LoggingConstants.UpdateStatementOperationFailed, LoggingConstants.HskUiLogPrefix, caseId, statement.MaterialId);
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiClient.PreChargeDecisionAnalysisOutcome> FirstInitialReviewGetCaseHistoryAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            return await this.apiClient.FirstInitialReviewGetCaseHistoryAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiClient.PreChargeDecisionAnalysisOutcome> GetInitialReviewByHistoryIdAsync(int caseId, int historyId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            return await this.apiClient.GetInitialReviewByHistoryIdAsync(caseId, historyId, cmsAuthValues);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<ApiClient.HistoryEvent>> GetHistoryEventsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            return await this.apiClient.GetHistoryEventsAsync(caseId, cmsAuthValues);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiClient.OffenceChangeResponse> GetOffenceChargeByHistoryIdAsync(int caseId, int historyId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            return await this.apiClient.GetOffenceChargeByHistoryIdAsync(caseId, historyId, cmsAuthValues);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiClient.PreChargeDecisionOutcome> GetPreChargeDecisionCaseHistoryEventDetailsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            return await this.apiClient.GetPreChargeDecisionCaseHistoryEventDetailsAsync(caseId, cmsAuthValues);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ApiClient.PreChargeDecisionOutcome> GetPreChargeDecisionByHistoryId(int caseId, int historyId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            return await this.apiClient.GetPreChargeDecisionByHistoryId(caseId, historyId, cmsAuthValues);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
