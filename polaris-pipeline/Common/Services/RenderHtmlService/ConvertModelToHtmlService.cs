﻿using RazorLight;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.RenderHtmlService
{
    public class ConvertModelToHtmlService : IConvertModelToHtmlService
    {
        private RazorLightEngine _engine;

        public ConvertModelToHtmlService()
        {
            _engine = new RazorLightEngineBuilder()
                .SetOperatingAssembly(typeof(ConvertModelToHtmlService).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<Stream> ConvertAsync<T>(T data)
        {
            var name = $"{typeof(T).Name}".Replace("Dto", string.Empty);
            var resourceName = $"Common.Services.RenderHtmlService.{name}.cshtml";
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var model = reader.ReadToEnd();
                string html = await _engine.CompileRenderStringAsync(name, model, data);
                return new MemoryStream(Encoding.UTF8.GetBytes(html));
            }
        }
    }
}