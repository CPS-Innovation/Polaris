using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolarisGateway.Models
{
    public class OcrPollResponse
    {
        [OpenApiProperty(Description = "URL to poll for OCR completion result")]
        public string NextUrl { get; set; } = default!;
    }
}
