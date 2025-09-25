// <copyright file="MdsCookie.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MdsClient.Model
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Mds cookie.
    /// </summary>
    public class MdsCookie
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MdsCookie"/> class.
        /// </summary>
        /// <param name="cookies">Auth cookie.</param>
        /// <param name="token">CMS auth token.</param>
        public MdsCookie(string cookies, string token)
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
