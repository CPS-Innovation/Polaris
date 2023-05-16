

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
            catch (Exception)
            {
                await Task.Delay(3 * 1000);
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
            RequestUri = args.GetDDEIRequestUri($"urns/{urn}/cases")
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
        var request = new HttpRequestMessage
        {
            RequestUri = args.GetCoordinatorRequestUri($"urns/{urn}/cases/{id}")
        };

        var (statusCode, content) = await Api.MakeJsonCall<dynamic>(request);

        FileSystem.DeleteCaseDirectory(args, urn, id);
        await FileSystem.WriteCaseJson(args, urn, id, content);

        //Console.WriteLine($"Processed case {index + 1} of {ids.Count()} for urn {urn}, {id}, status {(int)statusCode}");

        //await ProcessDocuments(args, urn, id);

        //await FileSystem.WriteCompletedCase(args, urn);
    }

    static async Task DeleteCaseOrchestration(Args args, int caseId, string cmsAuthValues, Guid correlationId)
    {
        var url = $"runtime/webhooks/durabletask/instances/{caseId}?code={args.CoordinatorFunctionKey}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("Correlation-Id", correlationId.ToString());
        request.Headers.Add("Cms-Auth-Values", cmsAuthValues);

        await Api.MakeCall(request);
    }

    static async Task DeleteCaseTrackerEntity(Args args, int caseId, string cmsAuthValues, Guid correlationId)
    {
        var url = $"runtime/webhooks/durabletask/instances/@trackerentity@{caseId}?code={args.CoordinatorFunctionKey}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("Correlation-Id", correlationId.ToString());
        request.Headers.Add("Cms-Auth-Values", cmsAuthValues);

        await Api.MakeCall(request);
    }

    static async Task<TrackerDto> GetTrackerEntity(Args args, string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
    {
        var url = $"urns/{caseUrn}/cases/{caseId}?code={args.CoordinatorFunctionKey}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Correlation-Id", correlationId.ToString());
        request.Headers.Add("Cms-Auth-Values", cmsAuthValues);

        var result = await Api.MakeJsonCall<TrackerDto>(request);

        return result.Item2;
    }
}

