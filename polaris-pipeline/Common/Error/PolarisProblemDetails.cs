using Microsoft.AspNetCore.Mvc;

namespace Common.Error
{
    public class PolarisProblemDetails : ProblemDetails
    {
        public PolarisProblemDetails InnerProblem { get; set; }
    }
}
