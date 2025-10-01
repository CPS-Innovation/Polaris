// <copyright file="CompleteReclassificationRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents request for a complete reclassificaton of a case material.
/// </summary>
public record CompleteReclassificationRequest(
    [property: JsonPropertyName("reclassification")] ReclassifyCaseMaterialRequest reclassification,
    [property: JsonPropertyName("actionPlan")] AddCaseActionPlanRequest? actionPlan,
    [property: JsonPropertyName("witness")] Witness? witness)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";

    /// <summary>
    /// Indicates if a new witness should be added based on request data (reclassification to statement and existing witness id not in request).
    /// </summary>
    /// <returns>True or false, based on request data.</returns>
    public bool AddWitness()
    {
        if (this.witness?.WitnessId <= 0 || ((this.witness?.WitnessId == null)
            && !string.IsNullOrWhiteSpace(this.witness?.Surname)
            && this.HasStatement()))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Flag to indicate if request has statement.
    /// </summary>
    /// <returns>True if request has statement object, otherwise false.</returns>
    public bool HasStatement()
    {
        return this.reclassification?.Statement != null;
    }

    /// <summary>
    /// Flag to indicate if request has exhibit.
    /// </summary>
    /// <returns>True if request has exhibit object, otherwise false.</returns>
    public bool HasExhibit()
    {
        return this.reclassification?.Exhibit != null;
    }

    /// <summary>
    /// Flag to indicate if request has action plan.
    /// </summary>
    /// <returns>True if request has action plan object, otherwise false.</returns>
    public bool HasActionPlan()
    {
        return this.actionPlan != null && this.HasStatement();
    }
}
