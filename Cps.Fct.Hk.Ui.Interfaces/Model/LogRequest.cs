// <copyright file="LogRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>
/// Details of a message to be logged to app insights.
/// </summary>
/// <param name="logLevel">The LogLevel represent the Information, Warning and Error logging types.</param>
/// <param name="message">The message to be logged.</param>
/// <param name="errorMessage">The exception to be logged if the log level is an error.</param>
public record LogRequest(
    [property: JsonPropertyName("logLevel")] LogLevel logLevel,
    [property: JsonPropertyName("message")] string message,
    [property: JsonPropertyName("errorMessage")] string? errorMessage);
