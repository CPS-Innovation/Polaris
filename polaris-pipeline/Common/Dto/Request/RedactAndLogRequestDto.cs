using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Dto.Request
{
    public class RedactAndLogRequestDto
    {

        public RedactPdfRequestDto RedactionPayload { get; set; }
        public CreateRedactionLogsRequest LogPayload { get; set; }
    }
}
