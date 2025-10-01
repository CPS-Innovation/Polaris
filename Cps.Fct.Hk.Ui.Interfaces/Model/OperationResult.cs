// <copyright file="OperationResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Represents operation result model.
/// </summary>
/// <param name="Success">The operation has succeeded or failed.</param>
/// <param name="OperationName">The name of the opertion.</param>
/// <param name="ErrorMessage">The error mesage produced by the operation.</param>
/// <param name="ResultData">Results returned by the operation.</param>
public record OperationResult(
[property: JsonPropertyName("success")] bool Success,
[property: JsonPropertyName("operationName")] string OperationName,
[property: JsonPropertyName("errorMessage")] string? ErrorMessage,
[property: JsonPropertyName("resultData")] object? ResultData);
