// <copyright file="IMdsApiClientFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MdsClient
{
    using Common.Dto.Request;
    using Common.Dto.Request.HouseKeeping;
    using Cps.MasterDataService.Infrastructure.ApiClient;

    /// <summary>
    /// Mds api client contracts.
    /// </summary>
    public interface IMdsApiClientFactory
    {
        /// <summary>
        /// Creates Mds api client.
        /// </summary>
        /// <param name="cookieHeader">Cookie header.</param>
        /// <returns>Mds client.</returns>
        IMdsApiClient Create(string cookieHeader);

        /// <summary>
        /// Asynchronously retrieves the summary of a case from CMS.
        /// </summary>
        /// <param name="request">The request containing the case ID and any required authentication parameters.</param>
        /// <param name="cmsAuthValues">CMS authentication values including cookies and tokens.</param>
        /// <returns>A task representing the asynchronous operation, which returns the <see cref="CaseSummary"/> of the case, or <c>null</c> if the case is not found or data is unavailable.</returns>
        /// <remarks>
        /// If the case summary is not found, the method returns <c>null</c>. The caller should handle the <c>null</c> return value accordingly.
        /// </remarks>
        Task<Common.Dto.Response.HouseKeeping.CaseSummaryResponse?> GetCaseSummaryAsync(GetCaseSummaryRequest request, CmsAuthValues cmsAuthValues);
    }
}
