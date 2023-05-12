using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Common.Domain.Extensions
{
    public static class HealthReportExtensions
    {
        public static object ToResult(this HealthReport report)
        {
            object resultObject = new
            {
                status = report.Status.ToString(),
                entries = report.Entries.Select(e =>
                {
                    JObject detail;
                    string description;

                    try
                    {
                        detail = JObject.Parse(e.Value.Description);
                        description = "Dependant Service";
                    }
                    catch 
                    {
                        detail = null;
                        description = e.Value.Description;
                    }

                    return new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        detail,
                        description,
                        e.Value.Exception
                    };
                })
            };

            return resultObject;
        }
    }
}