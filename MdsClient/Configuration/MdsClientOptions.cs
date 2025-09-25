// <copyright file="MdsClientOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MdsClient.Configuration
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The options for a client endpoint.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MdsClientOptions"/> class.
    /// </remarks>
    /// <param name="baseAddress">The service base address.</param>
    /// <param name="functionKey">The function key required to call an Azure Function.</param>
    /// <param name="relativePath">The relative paths supported by the service.</param>
    public class MdsClientOptions(
        Uri baseAddress,
        string? functionKey,
        IReadOnlyDictionary<string, string> relativePath)
    {
        /// <summary>
        /// The default section name in the appsettings.json file.
        /// </summary>
        public const string DefaultSectionName = $"DDEIClient";

        /// <summary>
        /// Initializes a new instance of the <see cref="MdsClientOptions"/> class.
        /// </summary>
        public MdsClientOptions()
            : this(
                  baseAddress: new Uri("#", UriKind.Relative),
                  functionKey: null,
                  relativePath: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
        {
        }

        /// <summary>
        /// Gets the service base address.
        /// </summary>
        public Uri BaseAddress { get; init; } = baseAddress;

        /// <summary>
        /// Gets the function key required to call an Azure Function.
        /// </summary>
        public string? FunctionKey { get; init; } = functionKey;

        /// <summary>
        /// Gets the relative paths supported by the service.
        /// </summary>
        public IReadOnlyDictionary<string, string> RelativePath { get; init; } = relativePath;
    }
}
