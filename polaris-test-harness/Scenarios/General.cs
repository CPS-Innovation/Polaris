

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

        while (true)
        {
            await Task.Delay(2000);
            var response = await GetTrackerEntity(args, urn, id, correlationId);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dto = Json.Deserialize<TrackerDto>(content);
                if (dto.Status == TrackerStatus.Completed || dto.Status == TrackerStatus.Failed)
                {
                    Console.WriteLine(content);
                    break;
                }
                else
                {
                    Console.WriteLine(dto.Status);
                }
            }

            // if (response.StatusCode != HttpStatusCode.NotFound)
            // {
            //     break;
            // }
        }

        // FileSystem.DeleteCaseDirectory(args, urn, id);
        // await FileSystem.WriteCaseJson(args, urn, id, content);

        //Console.WriteLine($"Processed case {index + 1} of {ids.Count()} for urn {urn}, {id}, status {(int)statusCode}");

        //await ProcessDocuments(args, urn, id);

        //await FileSystem.WriteCompletedCase(args, urn);
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

