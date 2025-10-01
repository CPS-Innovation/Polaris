// <copyright file="AddWitnessRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using System;
using System.Text.Json.Serialization;

public record AddWitnessRequest(
Guid id,
[property: JsonPropertyName("caseId")] int caseId,
[property: JsonPropertyName("firstName")] string FirstName,
[property: JsonPropertyName("surname")] string Surname)
    : BaseRequest(CorrespondenceId: id)
{
    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; init; } = "application/json";
}
