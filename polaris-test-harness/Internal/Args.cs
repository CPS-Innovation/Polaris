using System.Net;
using CommandLine;


public class Args
{
    [Option]
    public string UrnFileName { get; set; }
    [Option]
    public string DDEIHostName { get; set; }
    [Option]
    public string DDEIFunctionKey { get; set; }
    [Option]
    public string CoordinatorHostName { get; set; }
    [Option]
    public string CoordinatorFunctionKey { get; set; }
    [Option]
    public string CoordinatorDurableFunctionKey { get; set; }
    [Option]
    public string CmsUsername { get; set; }
    [Option]
    public string CmsPassword { get; set; }
    [Option(Default = false)]
    public bool RunInParallel { get; set; }
    [Option(Default = 4)]
    public int RunInParallelInFlightLimit { get; set; }

    [Option(Default = 10)]
    public int SequentialRunningMidCallDelay { get; set; }

    public string OutputFolderName
    {
        get
        {
            return $"output/{DateTime.UtcNow.ToString("s")}-{UrnFileName.Replace(".", "-")}";
        }
    }

    public List<HttpStatusCode> DieOnReceivedStatusCodes
    {
        get
        {
            return new List<HttpStatusCode>{
              HttpStatusCode.BadGateway
            };
        }
    }

    public string CompletedUrnsFilePath
    {
        get
        {
            return Path.Combine(OutputFolderName, "completed-urns.txt");
        }
    }

    //public bool ShouldRetrieveDocs { get; }

    public Uri GetCoordinatorRequestUri(string path)
    {
        return new Uri($"{CoordinatorHostName}/{path}?code={CoordinatorFunctionKey}");
    }

    public Uri GetCoordinatorLowLevelRequestUri(string path)
    {
        return new Uri($"{CoordinatorHostName}/{path}?code={CoordinatorDurableFunctionKey}");
    }

    public Uri GetDDEIRequestUri(string path)
    {
        return new Uri($"{DDEIHostName}/{path}?code={DDEIFunctionKey}");
    }
}