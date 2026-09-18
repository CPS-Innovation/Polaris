using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Dto.Response
{
    public class RedactAndLogResponse
    {
        public bool Success { get; init; }
        public Guid CorrelationId { get; init; } = default!;
        public StepResult Redaction { get; init; } = default!;
        public StepResult Logging { get; init; } = default!;
    }

    public class StepResult
    {
        public string Status { get; init; } = default!;
        public string? Error { get; init; }
    }
}
