using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Linq;

public class Health
{
    private readonly HealthCheckService _healthService;

    public Health(HealthCheckService healthService)
    {
        _healthService = healthService ?? throw new System.ArgumentNullException(nameof(healthService));
    }

    [FunctionName(nameof(Health))]
    public async Task<IActionResult> Healthcheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
        HttpRequest req)
    {
        var healthResult = await _healthService.CheckHealthAsync();
        var status = (healthResult.Status == HealthStatus.Healthy) ? 200 : 500;

        return new JsonResult(new
        {
            status = healthResult.Status.ToString(),
            entries = healthResult.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                e.Value.Description,
                e.Value.Exception
            })
        })
        {
            StatusCode = status
        };
    }
}
