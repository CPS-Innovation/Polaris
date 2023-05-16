using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public static class Api
{
    static HttpStatusCode[] _expectedStatusCodes = new HttpStatusCode[] {
      HttpStatusCode.OK,
      HttpStatusCode.NotFound,
      HttpStatusCode.Conflict
    };

    static HttpClient httpClient = new HttpClient(new HttpClientHandler
    {
        MaxConnectionsPerServer = 32
    });

    static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    public static async Task AttachCookiesToClient(Args args)
    {
        using var postContent = new FormUrlEncodedContent(new[] {
          new KeyValuePair<string, string>("username", args.CmsUsername),
          new KeyValuePair<string, string>("password", args.CmsPassword)
        });
        var response = await Api.MakeCall(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = args.GetDDEIRequestUri("login-full-cookie"),
            Content = postContent
        });
    }

    public static async Task<HttpResponseMessage> MakeCall(HttpRequestMessage req)
    {
        HttpResponseMessage response = null;
        string content = string.Empty;
        try
        {
            response = await httpClient.SendAsync(req);
            if (!_expectedStatusCodes.Contains(response.StatusCode))
            {
                throw new Exception("Expecting only 200s or 404s");
            }

            content = await response.Content.ReadAsStringAsync();
            Report(req, response, content);
            return response;
        }
        catch (Exception exception)
        {
            ReportException(exception, req, response, content);
            throw;
        }
    }

    public static async Task<(HttpStatusCode, T)> MakeJsonCall<T>(HttpRequestMessage req)
    {
        HttpResponseMessage response = null;
        string content = string.Empty;
        try
        {
            response = await httpClient.SendAsync(req);
            content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Report(req, response, content);
            return (response.StatusCode, JsonConvert.DeserializeObject<T>(content, _jsonSerializerSettings)!);
        }
        catch (Exception exception)
        {
            ReportException(exception, req, response, content);
            throw;
        }
    }

    private static void Report(HttpRequestMessage req, HttpResponseMessage response, string content)
    {
        var rawLog = $"{Pad(req.Method.ToString(), 6)} {Pad(((int)response.StatusCode).ToString(), 3)} {Pad(content.Length.ToString(), 9)} {req.RequestUri}";
        var log = Regex.Replace(rawLog, "code=.*", "code=redacted");
        File.AppendAllLines("log.log", new[] { log });
        Console.WriteLine(log);
    }

    private static void ReportException(Exception exception, HttpRequestMessage req, HttpResponseMessage response, string content)
    {
        Report(req, response, content);
        var lines = new[] {
            $"Failed: ***********************************************************************",
            //req.ToString(),
            //response == null ? null : response.ToString(),
            content.ToString(),
            //exception.ToString()
        };
        File.AppendAllLines("log.log", lines);
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }

        throw exception;
    }

    private static string Pad(string s, int length)
    {
        return s.PadRight(length, ' ');
    }
}