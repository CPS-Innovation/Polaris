using Common.Dto.Case.PreCharge;
using RazorLight;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RenderPcd
{
    public class ConvertPcdRequestToHtmlService : IConvertPcdRequestToHtmlService
    {
        private RazorLightEngine _engine;

        public ConvertPcdRequestToHtmlService()
        {
            _engine = new RazorLightEngineBuilder()
                .SetOperatingAssembly(typeof(ConvertPcdRequestToHtmlService).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<Stream> ConvertAsync(PcdRequestDto pcdRequest)
        {
            var resourceName = $"Common.Services.RenderHtmlService.PcdRequest.cshtml";
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var model = reader.ReadToEnd();
                var html = await _engine.CompileRenderStringAsync(nameof(ConvertPcdRequestToHtmlService), model, pcdRequest);
                return new MemoryStream(Encoding.UTF8.GetBytes(html));
            }
        }
    }
}