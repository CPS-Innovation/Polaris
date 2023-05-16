using System.Net;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public static class FileSystem
{
    static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    public static void PrepareOutputFolder(Args args)
    {
        Directory.CreateDirectory(args.OutputFolderName);
    }

    public static async Task<List<string>> ReadInUrns(Args args)
    {
        if (args.UrnFileName.EndsWith(".txt", true, CultureInfo.CurrentCulture))
        {
            var urns = (await File.ReadAllLinesAsync(Path.Combine("input", args.UrnFileName)))
              .Where(urn => urn.Trim().Length > 0)
              .ToList();

            if (!File.Exists(args.CompletedUrnsFilePath))
            {
                return urns;
            }

            var completedUrns = (await File.ReadAllLinesAsync(args.CompletedUrnsFilePath))
              .Where(urn => urn.Trim().Length > 0)
              .ToList();

            return urns
              .Except(completedUrns)
              .ToList();
        }
        else
        {
            return args.UrnFileName.Split(",").ToList();
        }
    }

    public static void DeleteCaseDirectory(Args args, string urn, int caseId)
    {
        var directoryName = GetDirectoryName(urn, caseId);
        var directoryPath = Path.Combine(args.OutputFolderName, directoryName);
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }

    public static async Task WriteCaseJson(Args args, string urn, int caseId, object content)
    {
        var directoryPath = EnsureDirectoryExists(args, urn, caseId);
        var writePath = Path.Combine(directoryPath, "case.json");
        await File.WriteAllTextAsync(writePath, JsonConvert.SerializeObject(content, _jsonSerializerSettings));
    }

    private static string GetDirectoryName(string urn, int caseId)
    {
        return $"{urn}-{caseId}";
    }
    private static string EnsureDirectoryExists(Args args, string urn, int caseId)
    {
        var directoryName = GetDirectoryName(urn, caseId);
        var directoryPath = Path.Combine(args.OutputFolderName, directoryName);
        Directory.CreateDirectory(Path.Combine(args.OutputFolderName, directoryName));
        return directoryPath;
    }
}