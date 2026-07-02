using System;
using System.Collections.Generic;
using System.Text;

namespace PolarisGateway.Models
{
    public class RedactionFileSizeOptions
    {
        public const string ConfigKey = "RedactionFileSize";

        public double FileSizeLimitMb { get; set; }
    }
}
