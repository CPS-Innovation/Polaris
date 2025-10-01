// <copyright file="CaseSummary.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a summary information related to a case, containing details such as the urn,
/// lead defendant first names and surname.
/// </summary>
/// <param name="CaseId">The case id of the case.</param>
/// <param name="Urn">The URN of the case.</param>
/// <param name="LeadDefendantFirstNames">The lead defendant first name of the case.</param>
/// <param name="LeadDefendantSurname">The lead defendant surname of the case.</param>
/// <param name="NumberOfDefendants">The number of defendants in the case.</param>
/// <param name="UnitName">The case unit name.</param>
public record CaseSummary(
    [property: JsonPropertyName("id")] int CaseId,
    [property: JsonPropertyName("urn")] string Urn,
    [property: JsonPropertyName("leadDefendantFirstNames")] string LeadDefendantFirstNames,
    [property: JsonPropertyName("leadDefendantSurname")] string LeadDefendantSurname,
    [property: JsonPropertyName("numberOfDefendants")] int NumberOfDefendants,
    [property: JsonPropertyName("unitName")] string UnitName);
