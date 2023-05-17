using System.Text.RegularExpressions;
using System.Net;

public static class Api
{
    static HttpStatusCode[] _expectedStatusCodes = new HttpStatusCode[] {
      HttpStatusCode.OK,
      HttpStatusCode.Accepted,
      HttpStatusCode.Locked,
      HttpStatusCode.NotFound,
      HttpStatusCode.Conflict
    };

    static string _cmsAuthValues { get; set; }

    static HttpClient httpClient = new HttpClient(new HttpClientHandler
    {
        MaxConnectionsPerServer = 32,
        UseCookies = false,
    });

    public static async Task AttachCookiesToClient(Args args)
    {
        using var postContent = new FormUrlEncodedContent(new[] {
          new KeyValuePair<string, string>("username", args.CmsUsername),
          new KeyValuePair<string, string>("password", args.CmsPassword)
        });
        var response = await Api.MakeCall(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = args.GetDDEIRequestUri("api/login-full-cookie"),
            Content = postContent
        });

        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        _cmsAuthValues = ExtractCmsAuthValues(cookies);
    }

    public static async Task<HttpResponseMessage> MakeCall(HttpRequestMessage req)
    {
        if (!string.IsNullOrEmpty(_cmsAuthValues))
        {
            req.Headers.Add("Cms-Auth-Values", _cmsAuthValues);
        }

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
        if (!string.IsNullOrEmpty(_cmsAuthValues))
        {
            req.Headers.Add("Cms-Auth-Values", _cmsAuthValues);
        }


        HttpResponseMessage response = null;
        string content = string.Empty;
        try
        {
            response = await httpClient.SendAsync(req);
            content = await response.Content.ReadAsStringAsync();
            if (!_expectedStatusCodes.Contains(response.StatusCode))
            {
                throw new Exception("Expecting only 200s or 404s");
            }
            Report(req, response, content);
            return (response.StatusCode, Json.Deserialize<T>(content));
        }
        catch (Exception exception)
        {
            ReportException(exception, req, response, content);
            throw;
        }
    }

    private static void Report(HttpRequestMessage req, HttpResponseMessage response, string content)
    {
        var timestamp = DateTime.Now.ToString("o");
        var rawLog = $"{timestamp} {Pad(req.Method.ToString(), 6)} {Pad(((int)response.StatusCode).ToString(), 3)} {Pad(content.Length.ToString(), 9)} {req.RequestUri}";
        var log = Regex.Replace(rawLog, "code=.*", "code=redacted");
        File.AppendAllLines("log.log", new[] { log });
        Console.WriteLine(log);
        //Console.WriteLine(content);
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

    private static string ExtractCmsAuthValues(IEnumerable<string> cookies)
    {
        var cookieString = string.Join(";", cookies);
        Console.WriteLine(cookieString);
        var match = Regex.Match(cookieString, "Cms-Auth-Values=([^;]+);");
        return WebUtility.UrlDecode(match.Groups[1].Value);
    }
}