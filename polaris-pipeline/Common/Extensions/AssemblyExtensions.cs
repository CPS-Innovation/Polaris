using System;
using System.Globalization;
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
        
        var response = new AssemblyStatus
        {
            Name = assemblyName.Name,
            Version = assemblyName.Version?.ToString(),
            LastBuilt = GetBuildDate(currentAssembly)
        };
        return new JsonResult(response) {StatusCode = (int) HttpStatusCode.OK};
    }
    
    private static DateTime GetBuildDate(Assembly assembly)
    {
        const string buildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion == null) return default;
        
        var value = attribute.InformationalVersion;
        var index = value.IndexOf(buildVersionMetadataPrefix, StringComparison.Ordinal);
        if (index <= 0) return default;
        
        value = value[(index + buildVersionMetadataPrefix.Length)..];
        return DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) 
            ? result : default;
    }
}
