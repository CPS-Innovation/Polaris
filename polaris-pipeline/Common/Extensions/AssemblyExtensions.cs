using System.Net;
using System.Reflection;
using Common.Health.Status;
using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions;

public static class AssemblyExtensions
{
    public static JsonResult CurrentStatus(this Assembly currentAssembly)
    {
        if (currentAssembly == null)
            return new JsonResult(new {status = "Assembly version could not be retrieved"}) {StatusCode = (int) HttpStatusCode.BadRequest};
        
        var assemblyName = currentAssembly.GetName();
        var version = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        var response = new AssemblyStatus
        {
            Name = assemblyName.Name,
            Version = version
        };
        return new JsonResult(response) {StatusCode = (int) HttpStatusCode.OK};
    }
}
