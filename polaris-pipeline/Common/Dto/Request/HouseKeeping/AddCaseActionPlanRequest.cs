// <copyright file="AddCaseActionPlanRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System;
using System.Text.Json.Serialization;

#pragma warning disable IDE1006 // Naming Styles

/// <summary>
/// Represents a request to add a case action plan.
/// </summary>
public record AddCaseActionPlanRequest(
    [property: JsonPropertyName("urn")] string urn,
    [property: JsonPropertyName("fullDefendantName")] string fullDefendantName,
    [property: JsonPropertyName("allDefendants")] bool allDefendants,
    [property: JsonPropertyName("date")] DateOnly date,
    [property: JsonPropertyName("dateExpected")] DateOnly? dateExpected,
    [property: JsonPropertyName("dateTimeCreated")] DateTime? dateTimeCreated,
    [property: JsonPropertyName("type")] string type,
    [property: JsonPropertyName("actionPointText")] string? actionPointText,
    [property: JsonPropertyName("status")] string? status,
    [property: JsonPropertyName("statusDescription")] string? statusDescription,
    [property: JsonPropertyName("dG6Justification")] string? dG6Justification,
    [property: JsonPropertyName("createdByOrganisation")] string createdByOrganisation,
    [property: JsonPropertyName("expectedDateUpdated")] bool expectedDateUpdated,
    [property: JsonPropertyName("partyType")] string? partyType,
    [property: JsonPropertyName("policeChangeReason")] string? policeChangeReason,
    [property: JsonPropertyName("statusUpdated")] bool statusUpdated,
    [property: JsonPropertyName("syncedWithPolice")] string? syncedWithPolice,
    [property: JsonPropertyName("cpsChangeReason")] string? cpsChangeReason,
    [property: JsonPropertyName("duplicateOriginalMaterial")] string? duplicateOriginalMaterial,
    [property: JsonPropertyName("material")] string? material,
    [property: JsonPropertyName("chaserTaskDate")] DateOnly? chaserTaskDate,
    [property: JsonPropertyName("defendantId")] int? defendantId,
    [property: JsonPropertyName("steps")] Step[] steps)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";
}

/// <summary>
/// Represents a single step in an action plan.
/// </summary>
public record Step(
    [property: JsonPropertyName("code")] string code,
    [property: JsonPropertyName("description")] string description,
    [property: JsonPropertyName("text")] string text,
    [property: JsonPropertyName("hidden")] bool hidden,
    [property: JsonPropertyName("hiddenDraft")] bool hiddenDraft);

#pragma warning restore IDE1006 // Naming Styles
