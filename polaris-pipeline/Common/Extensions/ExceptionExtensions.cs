using System;
using System.Reflection;
using System.Text;

namespace Common.Extensions;

public static class ExceptionExtensions
{
    public static string ToStringFullResponse(this Exception exception)
    {
        var sb = new StringBuilder();
        try
        {
            var deployedFunctionName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            sb.AppendLine($"Function: {deployedFunctionName}");
        }
        catch (Exception localException)
        {
            sb.AppendLine("Local failure appending function hostname: " + localException);
        }

        try
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version;
            if (version != null)
            {
                var assemblyVersion = version
                    .ToString();

                sb.AppendLine($"Version: {assemblyVersion}");
            }
        }
        catch (Exception localException)
        {
            sb.AppendLine("Local failure appending assembly version number: " + localException);
        }

        try
        {
            var informationalVersion = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            sb.AppendLine($"Informational Version: {informationalVersion}");
        }
        catch (Exception localException)
        {
            sb.AppendLine("Local failure appending assembly informational version number: " + localException);
        }

        sb.Append(NestedMessage(exception));

        return sb.ToString();
    }

    private static string NestedMessage(Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        var innerExceptionMessage = NestedMessage(exception.InnerException);

        return $"{exception.Message};\n{innerExceptionMessage}";
    }
}