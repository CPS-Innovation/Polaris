using Common.Dto.Case.PreCharge;
using Common.Dto.Tracker;
using RazorEngineCore;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RenderPcd
{
    public class ConvertPcdRequestToHtmlService : IConvertPcdRequestToHtmlService
    {
        const string pcdRequestTemplateFilename = "PcdRequestTemplate.dll";

        public ConvertPcdRequestToHtmlService() 
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Common.Services.RenderHtmlService.PcdRequest.cshtml";
            string templateText = null;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                templateText = reader.ReadToEnd();

                IRazorEngine razorEngine = new RazorEngine();
                IRazorEngineCompiledTemplate<RazorEngineTemplateBase<PcdRequestDto>> saveTemplate
                    = razorEngine.Compile<RazorEngineTemplateBase<PcdRequestDto>>(templateText, builder =>
                    {
                        builder.AddAssemblyReferenceByName($"{nameof(System)}.{nameof(System.Collections)}");
                    });
                saveTemplate.SaveToFile(pcdRequestTemplateFilename);
            }
        }

        public async Task<Stream> ConvertAsync(PcdRequestDto pcdRequest)
        {
            IRazorEngineCompiledTemplate<RazorEngineTemplateBase<PcdRequestDto>> loadTemplate
                = RazorEngineCompiledTemplate<RazorEngineTemplateBase<PcdRequestDto>>.LoadFromFile(pcdRequestTemplateFilename);

            string html = await loadTemplate.RunAsync(instance => instance.Model = pcdRequest);
            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }
    }
}