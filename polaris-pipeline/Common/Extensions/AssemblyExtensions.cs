using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Common.Domain.Health;
using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions;

public static class AssemblyExtensions
{
    public static JsonResult CurrentStatus(this Assembly currentAssembly)
    {
        if (currentAssembly == null)
            return new JsonResult(new { status = "Assembly version could not be retrieved" }) { StatusCode = (int)HttpStatusCode.BadRequest };

        var assemblyName = currentAssembly.GetName();
        var infoVerAttr = (AssemblyInformationalVersionAttribute)currentAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
            .FirstOrDefault();

        var response = new AssemblyStatus
        {
            Name = assemblyName.Name,
            BuildVersion = assemblyName.Version?.ToString(),
            SourceVersion = GetShortHash(infoVerAttr),
            LastBuilt = GetBuildDate(currentAssembly)
        };
        return new JsonResult(response) { StatusCode = (int)HttpStatusCode.OK };
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        var creationDate = new FileInfo(assembly.Location).CreationTimeUtc;
        var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        return TimeZoneInfo.ConvertTime(creationDate, TimeZoneInfo.Utc, britishZone);
    }

    private static string GetShortHash(AssemblyInformationalVersionAttribute infoVerAttr)
    {
        var version = "1.0.0+LOCALBUILD"; // Dummy version for local dev
        if (infoVerAttr == null || infoVerAttr.InformationalVersion.Length <= 6) return version;

        // Hash is embedded in the version after a '+' symbol, e.g. 1.0.0+a34a913742f8845d3da5309b7b17242222d41a21
        version = infoVerAttr.InformationalVersion;
        var longHash = version[(version.IndexOf('+') + 1)..];
        return longHash[..8];
    }
}
