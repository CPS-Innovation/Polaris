// <copyright file="Witness.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Text.Json.Serialization;

/// <summary>
/// Details of a Witness associated with a case.
/// </summary>
/// <param name="CaseId">The unique case ID.</param>
/// <param name="WitnessId">The unique witness ID.</param>
/// <param name="FirstName">First name of witness.</param>
/// <param name="Surname">Surname of witness.</param>
public record Witness(
    [property: JsonPropertyName("caseId")] int CaseId,
    [property: JsonPropertyName("witnessId")] int? WitnessId,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("surname")] string Surname);
