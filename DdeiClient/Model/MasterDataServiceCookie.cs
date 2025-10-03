// <copyright file="MdsCookie.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Model
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Mds cookie.
    /// </summary>
    public class MasterDataServiceCookie
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MasterDataServiceCookie"/> class.
        /// </summary>
        /// <param name="cookies">Auth cookie.</param>
        /// <param name="token">CMS auth token.</param>
        public MasterDataServiceCookie(string cookies, string token)
        {
            this.Cookies = cookies;
            this.Token = token;
            this.ExpiryTime = DateTime.UtcNow.AddHours(1).ToString("o", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets CMS cookie.
        /// </summary>
        public string Cookies { get; init; } = string.Empty;

        /// <summary>
        /// Gets CMS token.
        /// </summary>
        public string Token { get; init; } = string.Empty;

        /// <summary>
        /// Gets token expiry.
        /// </summary>
        public string ExpiryTime { get; init; } = string.Empty;
    }

}
