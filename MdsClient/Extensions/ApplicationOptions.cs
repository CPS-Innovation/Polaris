// <copyright file="ApplicationOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MdsClient.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Application options.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplicationOptions"/> class.
/// </remarks>
/// <param name="appName">The display name of the application.</param>
/// <param name="appDescription">The description of the application.</param>
[method: JsonConstructor]

/// <summary>
/// Application options.
/// </summary>
public class ApplicationOptions(
    string appName,
    string appDescription)
{
    /// <summary>
    /// The default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "Application";

    /// <summary>
    /// The default <see cref="JsonSerializerOptions"/> for the application.
    /// </summary>
    public static readonly JsonSerializerOptions ApplicationSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
            {
                new JsonStringEnumConverter(),
            },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationOptions"/> class.
    /// </summary>
    public ApplicationOptions()
        : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    /// Gets the app name of the application.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string AppName { get; init; } = appName;

    /// <summary>
    /// Gets the description of the application.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AppDescription { get; init; } = appDescription;

}
