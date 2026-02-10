// <copyright file="ServiceCollectionExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds options to be used with the <c>IOptions</c> pattern.
    /// See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options entity.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>Fluently returns the service collection.</returns>
    public static IServiceCollection AddServiceOptions<TOptions>(this IServiceCollection services, string sectionName)
    where TOptions : class
    {
        services
            .AddOptions<TOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                config
                    .GetSection(sectionName)
                    .Bind(options);
            })
            .ValidateDataAnnotations();

        return services;
    }
}
