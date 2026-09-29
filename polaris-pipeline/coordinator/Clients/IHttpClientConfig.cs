// <copyright file="IHttpClientConfig.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients;

public interface IHttpClientConfig
{
    string BaseUrl { get; }

    int TimeoutSeconds { get; }

    string AccessKey { get; }
}
