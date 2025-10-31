// <copyright file="ICaseMaterialService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using System.Collections.Generic;

/// <summary>
/// Interface for case materials service that provides methods to map case materials.
/// </summary>
public interface ICaseMaterialService
{
    /// <summary>
    /// Maps a collection of <see cref="Communication"/> objects to a list of <see cref="CaseMaterial"/> objects.
    /// </summary>
    /// <param name="communications">The collection of <see cref="Communication"/> objects to be mapped.</param>
    /// <returns>A list of <see cref="CaseMaterial"/> objects representing the mapped communications.</returns>
    List<CaseMaterial> MapCommunicationsToCaseMaterials(IEnumerable<Communication> communications);

    /// <summary>
    /// Maps a <see cref="UsedExhibitsResponse"/> object to a list of <see cref="CaseMaterial"/> objects.
    /// </summary>
    /// <param name="usedExhibits">
    /// The <see cref="UsedExhibitsResponse"/> containing a collection of used exhibits to be mapped.
    /// </param>
    /// <param name="exhibitProducers">contains collection of exhibit producers associated with the case.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>
    /// A list of <see cref="CaseMaterial"/> objects representing the mapped used exhibits.
    /// Returns an empty list if no exhibits are available.
    /// </returns>
    List<CaseMaterial> MapUsedExhibitsToCaseMaterials(UsedExhibitsResponse? usedExhibits, ExhibitProducersResponse? exhibitProducers, int caseId);

    /// <summary>
    /// Maps a <see cref="UsedStatementsResponse"/> object to a list of <see cref="CaseMaterial"/> objects.
    /// </summary>
    /// <param name="usedStatements">
    /// The <see cref="UsedStatementsResponse"/> containing a collection of used statements to be mapped.
    /// </param>
    /// <returns>
    /// A list of <see cref="CaseMaterial"/> objects representing the mapped used statements.
    /// Returns an empty list if no statements are available.
    /// </returns>
    List<CaseMaterial> MapUsedStatementsToCaseMaterials(UsedStatementsResponse? usedStatements);

    /// <summary>
    /// Maps a <see cref="UsedMgFormsResponse"/> object to a list of <see cref="CaseMaterial"/> objects.
    /// </summary>
    /// <param name="usedMgForms">
    /// The <see cref="UsedMgFormsResponse"/> containing a collection of used MG forms to be mapped.
    /// </param>
    /// <returns>
    /// A list of <see cref="CaseMaterial"/> objects representing the mapped used MG forms.
    /// Returns an empty list if no used MG forms are available.
    /// </returns>
    List<CaseMaterial> MapUsedMgFormsToCaseMaterials(UsedMgFormsResponse? usedMgForms);

    /// <summary>
    /// Maps a <see cref="UsedOtherMaterialsResponse"/> object to a list of <see cref="CaseMaterial"/> objects.
    /// </summary>
    /// <param name="usedOtherMaterials">
    /// The <see cref="UsedOtherMaterialsResponse"/> containing a collection of used other materials to be mapped.
    /// </param>
    /// <returns>
    /// A list of <see cref="CaseMaterial"/> objects representing the mapped used other materials.
    /// Returns an empty list if no used other materials are available.
    /// </returns>
    List<CaseMaterial> MapUsedOtherMaterialsToCaseMaterials(UsedOtherMaterialsResponse? usedOtherMaterials);

    /// <summary>
    /// Maps a <see cref="UnusedMaterialsResponse"/> object to a list of <see cref="CaseMaterial"/> objects.
    /// </summary>
    /// <param name="unusedMaterials">
    /// The <see cref="UnusedMaterialsResponse"/> containing collections of unused materials to be mapped.
    /// </param>
    /// <returns>
    /// A list of <see cref="CaseMaterial"/> objects representing the mapped unused materials.
    /// Returns an empty list if no materials are available.
    /// </returns>
    List<CaseMaterial> MapUnusedMaterialsToCaseMaterials(UnusedMaterialsResponse? unusedMaterials);

    /// <summary>
    /// Retrieves case materials asynchronously, including: communications, unused materials, used statements, used exhibits, used MG forms and used other materials.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">Authentication details for accessing the CMS.</param>
    /// <returns>
    /// A tuple containing case communications, unused materials, used statements, used exhibits, used MG forms, used other materials and exhibit producers.
    /// </returns>
    Task<(
        IReadOnlyCollection<Communication>,
        UnusedMaterialsResponse,
        UsedStatementsResponse,
        UsedExhibitsResponse,
        UsedMgFormsResponse,
        UsedOtherMaterialsResponse,
        ExhibitProducersResponse)>
        RetrieveCaseMaterialsAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Adds materials to the list of case materials, handling duplicates by removing the existing material and adding the new one.
    /// </summary>
    /// <typeparam name="T">The type of the material being added, which must implement <see cref="IMaterial"/>.</typeparam>
    /// <param name="allCaseMaterials">The list of all case materials to which the new materials will be added.</param>
    /// <param name="materials">The collection of materials to be added to the case materials list.</param>
    /// <param name="category">The category of the material being added (e.g., "Exhibit", "Statement").</param>
    /// <param name="type">The type of the material being added (e.g., "Other Exhibit", "Other Statement").</param>
    /// <param name="status">The status of the material being added (e.g., "Used", "Unused").</param>
    void AddCaseMaterials<T>(List<CaseMaterial> allCaseMaterials, IEnumerable<T> materials, string category, string type, string status)
    where T : IMaterial;

    /// <summary>
    /// Retrieves list of exhibit producers required as part of exhibit reclassification for a given case Id.
    /// </summary>
    /// <param name="caseId">The unique ID of the case.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<ExhibitProducersResponse> GetExhibitProducersAsync(int caseId, CmsAuthValues cmsAuthValues);
}
