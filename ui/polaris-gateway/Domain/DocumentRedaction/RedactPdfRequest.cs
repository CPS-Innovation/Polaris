﻿using System.Collections.Generic;

namespace RumpoleGateway.Domain.DocumentRedaction
{
    public class RedactPdfRequest
    {
        public int CaseId { get; set; }

        public int DocumentId { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinition> RedactionDefinitions { get; set; }
    }
}
