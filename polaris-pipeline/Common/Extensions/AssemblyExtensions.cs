using System.Reflection;
using Common.Health.Status;

namespace Common.Extensions;

public static class AssemblyExtensions
{
    public static AssemblyStatus CurrentStatus(this Assembly currentAssembly)
    {
        if (currentAssembly == null)
            return null;
        
        var assemblyName = currentAssembly.GetName();
        var version = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        var response = new AssemblyStatus
        {
            Name = assemblyName.Name,
            Version = version
        };
        return response;
    }
}
