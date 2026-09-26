using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Dto.Response
{
    public class RedactAndLogResponse
    {
        public bool Success { get; init; }
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public StepResult Redaction { get; init; }
        public StepResult Logging { get; init; }
    }

    public class StepResult
    {
        public string Status { get; init; }
        public string? Error { get; init; }
    }
}
