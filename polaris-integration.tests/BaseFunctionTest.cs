using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace polaris_integration.tests
{
    public class BaseFunctionTest : IDisposable
    {
        protected static IConfigurationRoot _config;
        private static string _ddeiAuthUrl;
        protected static string _polarisGatewayUrl;
        protected static string _polarisGatewayCode;
        protected static string _polarisCoordinatorUrl;
        protected static string _polarisCoordinatorCode;

        private string _adToken;
        private string _cmsAuth;

        static BaseFunctionTest()
        {
            var dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            _config = new ConfigurationBuilder()
                .SetBasePath(dir)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _ddeiAuthUrl = _config["BaseUrls:DdeiAuthUrl"];
            _polarisGatewayUrl = _config["BaseUrls:PolarisGatewayUrl"];
            _polarisGatewayCode = _config["BaseUrls:PolarisGatewayCode"];
            _polarisCoordinatorUrl = _config["BaseUrls:PolarisCoordinatorUrl"];
            _polarisCoordinatorCode = _config["BaseUrls:PolarisCoordinatorCode"];

            //StartHost("polaris-gateway\\polaris-gateway", 7075);
            //StartHost("polaris-pipeline\\coordinator", 7072);
        }

        public BaseFunctionTest()
        {
            _adToken = GetBearerToken();
            _cmsAuth = GetCmsAuthCookieValues();
        }

        public void Dispose()
        {
            // StopHosts();
        }

        // Code to manually start azure functions, leave comented out for future reference
        // Assume that the subjects under test are running in Azure Functions
        #region Manage Azure Functions Hosts
        /*protected void StartHost(string service, int port)
        {
            try
            {
                const string funcExeRuntime = @"C:\Users\CPS\AppData\Roaming\npm\node_modules\azure-functions-core-tools\bin\func.exe";

                var args = $"start --port {port}";
                var binDir = @$"C:\dev\CPS\src\Polaris\{service}";

                ProcessStartInfo hostProcess = new ProcessStartInfo
                {
                    FileName = funcExeRuntime,
                    Arguments = args,
                    WorkingDirectory = binDir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                var host = Process.Start(hostProcess);
                _hosts.Add(host);

                WaitForFunctionToInitialise(host);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void WaitForFunctionToInitialise(Process host)
        {
            while (!host.StandardOutput.EndOfStream)
            {
                var line = host.StandardOutput.ReadLine();

                // Process fully started?
                if (line == "For detailed output, run func with --verbose flag.")
                    break;
            }
        }

        protected void StopHosts()
        {
            foreach (var host in _hosts)
            {
                host.CloseMainWindow();
                host.Dispose();
            }
        } */
        #endregion

        private string GetBearerToken()
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _config["AutomationUser:token_endpoint"]);
            var collection = new List<KeyValuePair<string, string>>
            {
                new("grant_type", _config["AutomationUser:grant_type"]),
                new("client_id", _config["AutomationUser:client_id"]),
                new("client_secret", _config["AutomationUser:client_secret"]),
                new("scope", _config["AutomationUser:scope"]),
                new("username", _config["AutomationUser:username"]),
                new("password", _config["AutomationUser:password"])
            };
            request.Content = new FormUrlEncodedContent(collection);
            var response = client.Send(request);
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;

            var authToken = JsonSerializer.Deserialize<AuthTokenDto>(json);
            return authToken.access_token;
        }

        private string GetCmsAuthCookieValues()
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_ddeiAuthUrl}api/login-full-cookie");
            var collection = new List<KeyValuePair<string, string>>
            {
                new("username", _config["Cms:Username"]),
                new("password", _config["Cms:Password"])
            };
            request.Content = new FormUrlEncodedContent(collection);
            var response = client.Send(request);
            response.EnsureSuccessStatusCode();
            var cookieValues = response.Content.ReadAsStringAsync().Result;

            var regex = new Regex(@".+({Cookies: \""[^\}]+\""\})");
            var cookie = regex.Match(cookieValues).Groups[1].Value;

            return cookie;
        }

        protected string MakeUrl(string format, Dictionary<string, string> keyValues)
        {
            foreach (string key in keyValues.Keys)
                format = format.Replace($"{{{key}}}", keyValues[key]);

            return format;
        }

        protected void AddAuthAndContextHeaders(HttpRequestMessage request, string correlationId)
        {
            var cmsAuthValues = Uri.EscapeDataString(_cmsAuth);

            var headers = request.Headers;
            headers.Add("Correlation-Id", correlationId);
            headers.Add("Authorization", $"Bearer {_adToken}");
            headers.Add("Cms-Auth-Values", cmsAuthValues);
            headers.Add("Cookie", $"Cms-Auth-Values={cmsAuthValues}");
        }

        protected IEnumerable<int> Fibonacci(int n)
        {
            int previous = 0, current = 1;
            for (var i = 0; i < n; i++)
            {
                yield return current;
                var temp = previous;
                previous = current;
                current = temp + current;
            }
        }
    }
}
