// <copyright file="CommsDocumentTypeIds.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Constants;

/// <summary>
/// Contains constants for Communication document Type IDs to be excluded from MGForms listings.
/// These IDs correspond to specific document types in the DocumentTypeMapper.
/// </summary>
public static class CommsDocumentTypeIds
{
    /// <summary>
    /// Communication Type ID for PE3 document type.
    /// </summary>
    public const string PE3 = "1053";

    /// <summary>
    /// Communication Type ID for PE4 document type.
    /// </summary>
    public const string PE4 = "1054";

    /// <summary>
    /// Communication Type ID for DREP document type.
    /// </summary>
    public const string DREP = "1055";

    /// <summary>
    /// Communication Type IDs that should be excluded from Used MG Forms.
    /// </summary>
    public static readonly string[] ExcludedFromUsedMgForms = { PE3, PE4, DREP };
}
