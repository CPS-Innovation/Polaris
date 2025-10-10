// <copyright file="CaseMaterialService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using System.Linq;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Cps.Fct.Hk.Ui.Services.Constants;
using Common.Dto.Request.HouseKeeping;
using System.Globalization;

/// <summary>
/// Provides services for processing and retrieving case materials related to a case.
/// </summary>
public class CaseMaterialService(
    ILogger<CaseMaterialService> logger,
    ICommunicationService communicationService,
    IDocumentTypeMapper documentTypeMapper)
    : ICaseMaterialService
{
    private readonly ILogger<CaseMaterialService> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly IDocumentTypeMapper documentTypeMapper = documentTypeMapper;

    /// <inheritdoc />
    public List<CaseMaterial> MapCommunicationsToCaseMaterials(IEnumerable<Communication>? communications)
    {
        var caseMaterials = new List<CaseMaterial>();

        if (communications == null)
        {
            return caseMaterials;
        }

        caseMaterials.AddRange(communications.Select(c =>
        {
            bool canReclassfy = this.IsCaseMaterialReclassifiable(c.DocumentTypeId, c.Direction);

            return new CaseMaterial(
                 c.Id,
                 c.OriginalFileName,
                 c.Subject,
                 c.DocumentTypeId,
                 c.MaterialId,
                 c.Link,
                 c.Category,
                 c.Type,
                 c.HasAttachments,
                 c.Status,
                 "None",
                 c.Method,
                 c.Direction,
                 c.Party,
                 c.Date,
                 isReclassifiable: canReclassfy);
        }));
        return caseMaterials;
    }

    /// <inheritdoc />
    public List<CaseMaterial> MapUsedExhibitsToCaseMaterials(UsedExhibitsResponse? usedExhibits, ExhibitProducersResponse? exhibitProducers, int caseId)
    {
        var caseMaterials = new List<CaseMaterial>();

        if (usedExhibits?.Exhibits == null)
        {
            return caseMaterials;
        }

        caseMaterials.AddRange(usedExhibits.Exhibits.Select(e =>
        {
            DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapDocumentType(e.DocumentType ?? 0);

            int? existingProducerOrWitnessId = this.MapExistingProducerOrWitnessId(e.Producer, exhibitProducers, caseId);

            bool canReclassfy = this.IsCaseMaterialReclassifiable(e.DocumentType);

            return new CaseMaterial(
                e.Id,
                e.OriginalFileName,
                e.Title,
                e.DocumentType ?? 0,
                e.Id,
                e.Link,
                documentTypeInfo.Category,
                documentTypeInfo.DocumentType,
                false,
                e.Status,
                "Used",
                date: e.ReceivedDate,
                reference: e.Reference,
                producer: e.Producer,
                item: e.Title,
                existingProducerOrWitnessId: existingProducerOrWitnessId,
                isReclassifiable: canReclassfy);
        }));

        return caseMaterials;
    }

    /// <inheritdoc />
    public List<CaseMaterial> MapUsedStatementsToCaseMaterials(UsedStatementsResponse? usedStatements)
    {
        var caseMaterials = new List<CaseMaterial>();

        if (usedStatements?.Statements == null)
        {
            return caseMaterials;
        }

        caseMaterials.AddRange(usedStatements.Statements.Select(s =>
        {
            DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapDocumentType(s.DocumentType ?? 0);

            // Use s.Title as fallback when s.OriginalFileName is null or blank
            string subject = string.IsNullOrWhiteSpace(s.OriginalFileName) ? s.Title : s.PresentationTitle;

            bool canReclassfy = this.IsCaseMaterialReclassifiable(s.DocumentType);

            return new CaseMaterial(
                s.Id,
                s.OriginalFileName,
                subject,
                s.DocumentType ?? 0,
                s.Id,
                s.Link,
                documentTypeInfo.Category,
                documentTypeInfo.DocumentType,
                false,
                s.Status,
                "Used",
                date: s.ReceivedDate,
                recordedDate: s.StatementTakenDate,
                witnessId: s.WitnessId,
                title: s.Title, // This is statement number.
                isReclassifiable: canReclassfy);
        }));

        return caseMaterials;
    }

    /// <inheritdoc />
    public List<CaseMaterial> MapUsedMgFormsToCaseMaterials(UsedMgFormsResponse? usedMgForms)
    {
        var caseMaterials = new List<CaseMaterial>();

        if (usedMgForms?.MgForms == null)
        {
            return caseMaterials;
        }

        caseMaterials.AddRange(usedMgForms.MgForms.Select(e =>
        {
            DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapMaterialType(e.MaterialType ?? "0");
            _ = int.TryParse(e.MaterialType, out int documentTypeId);

            bool canReclassfy = this.IsCaseMaterialReclassifiable(documentTypeId);

            return new CaseMaterial(
                e.Id,
                e.OriginalFileName,
                e.Title,
                documentTypeId,
                e.Id,
                e.Link,
                documentTypeInfo.Category,
                documentTypeInfo.DocumentType,
                false,
                e.Status,
                "Used",
                date: e.Date,
                isReclassifiable: canReclassfy);
        }));

        return caseMaterials;
    }

    /// <inheritdoc />
    public List<CaseMaterial> MapUsedOtherMaterialsToCaseMaterials(UsedOtherMaterialsResponse? usedOtherMaterials)
    {
        var caseMaterials = new List<CaseMaterial>();

        if (usedOtherMaterials?.MgForms == null)
        {
            return caseMaterials;
        }

        caseMaterials.AddRange(usedOtherMaterials.MgForms.Select(e =>
        {
            DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapMaterialType(e.MaterialType ?? "0");

            _ = int.TryParse(e.MaterialType, out int documentTypeId);

            bool canReclassfy = this.IsCaseMaterialReclassifiable(documentTypeId);

            return new CaseMaterial(
                e.Id,
                e.OriginalFileName,
                e.Title,
                e.DocumentType ?? documentTypeId,
                e.Id,
                e.Link,
                documentTypeInfo.Category,
                documentTypeInfo.DocumentType,
                false,
                e.Status,
                "Used",
                date: e.Date,
                isReclassifiable: canReclassfy);
        }));

        return caseMaterials;
    }

    /// <inheritdoc />
    public List<CaseMaterial> MapUnusedMaterialsToCaseMaterials(UnusedMaterialsResponse? unusedMaterials)
    {
        var caseMaterials = new List<CaseMaterial>();

        if (unusedMaterials == null)
        {
            return caseMaterials;
        }

        // Map unused exhibits
        if (unusedMaterials.Exhibits != null)
        {
            caseMaterials.AddRange(unusedMaterials.Exhibits.Select(e =>
            {
                DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapMaterialType(e.MaterialType ?? "0");

                bool canReclassfy = this.IsCaseMaterialReclassifiable(e.DocumentType);

                return new CaseMaterial(
                    e.Id,
                    e.OriginalFileName,
                    e.Title,
                    e.DocumentType ?? 0,
                    e.Id,
                    e.Link,
                    documentTypeInfo.Category,
                    documentTypeInfo.DocumentType,
                    false,
                    e.Status,
                    "Unused",
                    date: e.ReceivedDate,
                    isReclassifiable: canReclassfy);
            }));
        }

        // Map unused mgForms
        if (unusedMaterials.MgForms != null)
        {
            caseMaterials.AddRange(unusedMaterials.MgForms.Select(m =>
            {
                DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapMaterialType(m.MaterialType ?? "0");

                _ = int.TryParse(m.MaterialType, out int documentTypeId);

                bool canReclassfy = this.IsCaseMaterialReclassifiable(documentTypeId);

                return new CaseMaterial(
                    m.Id,
                    m.OriginalFileName,
                    m.Title,
                    documentTypeId,
                    m.Id,
                    m.Link,
                    documentTypeInfo.Category,
                    documentTypeInfo.DocumentType,
                    false,
                    m.Status,
                    "Unused",
                    date: m.Date,
                    isReclassifiable: canReclassfy);
            }));
        }

        // Map unused otherMaterials
        if (unusedMaterials.OtherMaterials != null)
        {
            caseMaterials.AddRange(unusedMaterials.OtherMaterials.Select(om =>
            {
                DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapMaterialType(om.MaterialType ?? "0");

                _ = int.TryParse(om.MaterialType, out int documentTypeId);

                bool canReclassfy = this.IsCaseMaterialReclassifiable(documentTypeId);

                return new CaseMaterial(
                    om.Id,
                    om.OriginalFileName,
                    om.Title,
                    documentTypeId,
                    om.Id,
                    om.Link,
                    documentTypeInfo.Category,
                    documentTypeInfo.DocumentType,
                    false,
                    om.Status,
                    "Unused",
                    date: om.Date,
                    isReclassifiable: canReclassfy);
            }));
        }

        // Map unused statements
        if (unusedMaterials.Statements != null)
        {
            caseMaterials.AddRange(unusedMaterials.Statements.Select(s =>
            {
                // Statements do not have a MaterialType property, using document type mapper to map to category.
                DocumentTypeInfo documentTypeInfo = this.documentTypeMapper.MapDocumentType(s.DocumentType ?? 0);

                bool canReclassfy = this.IsCaseMaterialReclassifiable(s.DocumentType);

                return new CaseMaterial(
                    s.Id,
                    s.OriginalFileName,
                    s.PresentationTitle,
                    s.DocumentType ?? 0,
                    s.Id,
                    s.Link,
                    documentTypeInfo.Category,
                    documentTypeInfo.DocumentType,
                    false,
                    s.Status,
                    "Unused",
                    date: s.ReceivedDate,
                    recordedDate: s.StatementTakenDate,
                    isReclassifiable: canReclassfy);
            }));
        }

        return caseMaterials;
    }

    /// <inheritdoc />
    public async Task<(
        IReadOnlyCollection<Communication>,
        UnusedMaterialsResponse,
        UsedStatementsResponse,
        UsedExhibitsResponse,
        UsedMgFormsResponse,
        UsedOtherMaterialsResponse,
        ExhibitProducersResponse)> RetrieveCaseMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        var communicationsTask = Task.Run(() => this.GetCommunicationsAsync(caseId, cmsAuthValues));
        var unusedMaterialsTask = Task.Run(() => this.GetUnusedMaterialsAsync(caseId, cmsAuthValues));
        var usedStatementsTask = Task.Run(() => this.GetUsedStatementsAsync(caseId, cmsAuthValues));
        var usedExhibitsTask = Task.Run(() => this.GetUsedExhibitsAsync(caseId, cmsAuthValues));
        var usedMgFormsTask = Task.Run(() => this.GetUsedMgFormsAsync(caseId, cmsAuthValues));
        var usedOtherMaterialsTask = Task.Run(() => this.GetUsedOtherMaterialsAsync(caseId, cmsAuthValues));
        var exhibitProducerTask = Task.Run(() => this.GetExhibitProducersAsync(caseId, cmsAuthValues));

        await Task
            .WhenAll(
            communicationsTask,
            unusedMaterialsTask,
            usedStatementsTask,
            usedExhibitsTask,
            usedMgFormsTask,
            usedOtherMaterialsTask,
            exhibitProducerTask)
            .ConfigureAwait(false);

        return (
            await communicationsTask.ConfigureAwait(false),
            await unusedMaterialsTask.ConfigureAwait(false),
            await usedStatementsTask.ConfigureAwait(false),
            await usedExhibitsTask.ConfigureAwait(false),
            await usedMgFormsTask.ConfigureAwait(false),
            await usedOtherMaterialsTask.ConfigureAwait(false),
            await exhibitProducerTask.ConfigureAwait(false));
    }

    /// <summary>
    /// Asynchronously retrieves communications based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Communication"/> objects.</returns>
    public async Task<IReadOnlyCollection<Communication>> GetCommunicationsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [{caseId}]...");
            return await this.communicationService.GetCommunicationsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching communications for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching communications for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Asynchronously retrieves unused materials based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="UnusedMaterialsResponse"/>.</returns>
    public async Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving unused materials for caseId [{caseId}]...");
            return await this.communicationService.GetUnusedMaterialsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching unused materials for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching unused materials for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Asynchronously retrieves used statements based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="UsedStatementsResponse"/>.</returns>
    public async Task<UsedStatementsResponse> GetUsedStatementsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving used statements for caseId [{caseId}]...");
            return await this.communicationService.GetUsedStatementsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used statements for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching used statements for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Asynchronously retrieves used exhibits based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="UsedExhibitsResponse"/>.</returns>
    public async Task<UsedExhibitsResponse> GetUsedExhibitsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving used exhibits for caseId [{caseId}]...");
            return await this.communicationService.GetUsedExhibitsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used exhibits for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching used exhibits for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Asynchronously retrieves used mg forms based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="UsedMgFormsResponse"/>.</returns>
    public async Task<UsedMgFormsResponse> GetUsedMgFormsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving used MG forms for caseId [{caseId}]...");
            return await this.communicationService.GetUsedMgFormsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used MG forms for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching used MG forms for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Asynchronously retrieves used other materials based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="UsedOtherMaterialsResponse"/>.</returns>
    public async Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving used other materials for caseId [{caseId}]...");
            return await this.communicationService.GetUsedOtherMaterialsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching used other materials for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching used other materials for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Adds materials to the list of case materials, handling duplicates by removing the existing material and adding the new one.
    /// </summary>
    /// <typeparam name="T">The type of the material being added, which must implement <see cref="IMaterial"/>.</typeparam>
    /// <param name="allCaseMaterials">The list of all case materials to which the new materials will be added.</param>
    /// <param name="materials">The collection of materials to be added to the case materials list.</param>
    /// <param name="category">The category of the material being added (e.g., "Exhibit", "Statement").</param>
    /// <param name="type">The type of the material being added (e.g., "Used", "Unused").</param>
    /// <param name="status">The status of the material being added (e.g., "Used", "Unused").</param>
    public void AddCaseMaterials<T>(List<CaseMaterial> allCaseMaterials, IEnumerable<T> materials, string category, string type, string status)
        where T : IMaterial
    {
        foreach (T material in materials)
        {
            bool isDuplicate = allCaseMaterials.Any(cm => cm.MaterialId == material.MaterialId);

            if (isDuplicate)
            {
                CaseMaterial? duplicateMaterial = allCaseMaterials!.FirstOrDefault(cm => cm.MaterialId == material.MaterialId);

                if (duplicateMaterial != null)
                {
                    allCaseMaterials!.Remove(duplicateMaterial);
                }
            }
        }
    }

    /// <summary>
    /// Map existing producer or witnessId for exhibit.
    /// </summary>
    /// <param name="producer">The name of the exhibit producer.</param>
    /// <param name="exhibitProducers">Collection of exhibit producers associated with the case.</param>
    /// <param name="caseId">The case Id that the exhibit producers belong to.</param>
    /// <returns>ExistingProducerOrWitnessId, otherwise null if not found.</returns>
    public int? MapExistingProducerOrWitnessId(string? producer, ExhibitProducersResponse? exhibitProducers, int caseId)
    {
        try
        {
            int? existingProducerOrWitnessId = null;

            if (string.IsNullOrWhiteSpace(producer) || exhibitProducers?.ExhibitProducers?.Count == 0)
            {
                return existingProducerOrWitnessId;
            }

            ExhibitProducer? exhibitProducer = exhibitProducers?.ExhibitProducers?
                .FirstOrDefault(x => x.Name.Equals(producer, StringComparison.InvariantCultureIgnoreCase));

            existingProducerOrWitnessId ??= exhibitProducer?.Id;

            return existingProducerOrWitnessId;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered mapping existing exhibit producer or witnessId for caseId [{caseId}]: {ex.Message}");
            throw new Exception($"An error was encountered mapping existing exhibit producer or witnessId for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Asynchronously retrieves exhibit producers based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>t
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ExhibitProducersResponse"/>.</returns>
    public async Task<ExhibitProducersResponse> GetExhibitProducersAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving exhibit producers for caseId [{caseId}]...");
            return await this.communicationService.GetExhibitProducersAsync(caseId, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} An error was encountered fetching exhibit producers for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"An error was encountered fetching exhibit producers for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Checks whether a case material can be reclassfied or not based on it's document type.
    /// </summary>
    /// <param name="documentTypeId">The case material's document Id.</param>
    /// <param name="direction">The communication direction.</param>
    /// <returns>True if the case material can be reclassfied, otherwise false.</returns>
    private bool IsCaseMaterialReclassifiable(int? documentTypeId, string? direction = null)
    {
        if (documentTypeId == null || documentTypeId == 0)
        {
            return false;
        }

        if (direction != null && direction.Equals("Outgoing", StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        return this.documentTypeMapper.GetDocumentTypesWithClassificationGroup().Any(d => d.Id == documentTypeId);
    }
}
