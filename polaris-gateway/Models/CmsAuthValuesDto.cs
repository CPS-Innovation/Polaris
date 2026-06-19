using System;
using System.Collections.Generic;
using System.Text;

namespace PolarisGateway.Models
{
    public class CmsAuthValuesDto
    {
        public string Token { get; set; } = string.Empty;

        public Guid SessionCorrelationId { get; set; }

        public DateTime SessionCreatedTime { get; set; }

        public string CmsVersionId { get; set; } = string.Empty;

        public string Cookies { get; set; } = string.Empty;

        public string UserIpAddress { get; set; } = string.Empty;
    }
}
