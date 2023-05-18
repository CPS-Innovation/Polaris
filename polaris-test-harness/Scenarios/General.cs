

using System.Net;
using System.Net.Http;
public static class General
{
    public static async Task EntryAsync(Args args)
    {
        var done = false;
        while (!done)
        {
            try
            {
                done = await RetryLoop(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
                //await Task.Delay(3 * 1000);
            }
        }
    }

    public static async Task<bool> RetryLoop(Args args)
    {
        FileSystem.PrepareOutputFolder(args);
        var urns = await FileSystem.ReadInUrns(args);

        await Api.AttachCookiesToClient(args);
        await ProcessListHelper.ProcessList(args, urns, (urn, index) => ProcessUrn(args, urn, index, urns.Count()));

        return true;
    }

    static async Task ProcessUrn(Args args, string urn, int index, int totalCount)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = args.GetDDEIRequestUri($"api/urns/{urn}/cases")
        };

        var (statusCode, content) = await Api.MakeJsonCall<IEnumerable<CaseIdentifiers>>(request);

        var ids = content.Select(item => item.Id);

        await ProcessListHelper.ProcessList(args, ids, async (id, index) =>
        {
            await ProcessCase(args, ids, urn, id, index);
        });
    }

    static async Task ProcessCase(Args args, IEnumerable<int> ids, string urn, int id, int index)
    {
        var correlationId = Guid.NewGuid().ToString();

        await DeleteCaseOrchestration(args, id, correlationId);
        await DeleteCaseTrackerEntity(args, id, correlationId);
        await RefreshTracker(args, urn, id, correlationId);

        var loop = true;
        while (loop)
        {
            //var statusResponse = await GetOrchestratorStatus(args, id, correlationId);
            //  Console.WriteLine(statusResponse);

            await Task.Delay(5000);
            var response = await GetTrackerEntity(args, urn, id, correlationId);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dto = Json.Deserialize<TrackerDto>(content);
                if (dto.Status == TrackerStatus.Completed || dto.Status == TrackerStatus.Failed)
                {
                    Console.WriteLine(content);
                    loop = false;
                }
                else
                {
                    Console.WriteLine(dto.Status);
                }
            }
        }

    }

    static async Task<string> GetOrchestratorStatus(Args args, int caseId, string correlationId)
    {
        var uri = new Uri($"{args.CoordinatorHostName}/runtime/webhooks/durabletask/instances/{caseId}?code={args.CoordinatorDurableFunctionKey}&showHistory=true&showHistoryOutput=true&showInput=true&returnInternalServerErrorOnFailure=true");

        var request = new HttpRequestMessage
        {
            RequestUri = uri,
        };
        request.Headers.Add("Correlation-Id", correlationId);

        var response = await Api.MakeCall(request);
        return await response.Content.ReadAsStringAsync();
    }

    static async Task DeleteCaseOrchestration(Args args, int caseId, string correlationId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = args.GetCoordinatorLowLevelRequestUri($"runtime/webhooks/durabletask/instances/{caseId}"),
        };
        request.Headers.Add("Correlation-Id", correlationId);

        await Api.MakeCall(request);
    }

    static async Task DeleteCaseTrackerEntity(Args args, int caseId, string correlationId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = args.GetCoordinatorLowLevelRequestUri($"runtime/webhooks/durabletask/instances/@trackerentity@{caseId}"),
        };
        request.Headers.Add("Correlation-Id", correlationId);

        await Api.MakeCall(request);
    }

    static async Task RefreshTracker(Args args, string caseUrn, int caseId, string correlationId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = args.GetCoordinatorRequestUri($"api/urns/{caseUrn}/cases/{caseId}"),
        };
        request.Headers.Add("Correlation-Id", correlationId);

        await Api.MakeCall(request);
    }

    static async Task<HttpResponseMessage> GetTrackerEntity(Args args, string caseUrn, int caseId, string correlationId)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = args.GetCoordinatorRequestUri($"api/urns/{caseUrn}/cases/{caseId}/tracker"),
        };
        request.Headers.Add("Correlation-Id", correlationId);

        return await Api.MakeCall(request);
    }
}

