// <copyright file="IMdsApiClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Clients.Interfaces
{
    using Cps.MasterDataService.Infrastructure.ApiClient;

    /// <summary>
    /// Mds api client contracts.
    /// </summary>
    public interface IMasterDataServiceApiClientFactory
    {
        /// <summary>
        /// Creates Mds api client.
        /// </summary>
        /// <param name="cookieHeader">Cookie header.</param>
        /// <returns>Mds client.</returns>
        IMdsApiClient Create(string cookieHeader);
    }
}
