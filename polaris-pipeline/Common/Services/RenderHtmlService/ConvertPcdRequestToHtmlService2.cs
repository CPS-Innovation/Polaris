using Common.Dto.Case.PreCharge;
using RazorLight;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RenderPcd
{
    public class ConvertPcdRequestToHtmlService2 : IConvertPcdRequestToHtmlService
    {
        private RazorLightEngine _engine;

        public ConvertPcdRequestToHtmlService2()
        {
            _engine = new RazorLightEngineBuilder()
                .SetOperatingAssembly(typeof(ConvertPcdRequestToHtmlService2).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<Stream> ConvertAsync(PcdRequestDto pcdRequest)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"Common.Services.RenderHtmlService.PcdRequest.cshtml";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string html = await _engine.CompileRenderStringAsync("some-key", reader.ReadToEnd(), pcdRequest);
                    return new MemoryStream(Encoding.UTF8.GetBytes(html));
                }
            }
            catch (Exception)
            {
                var templates = (typeof(ConvertPcdRequestToHtmlService2).Assembly).GetManifestResourceNames();
                throw;
            }
        }
    }
}