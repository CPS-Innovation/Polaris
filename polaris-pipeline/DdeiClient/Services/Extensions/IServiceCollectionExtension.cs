using Common.Dto.Response;
using Common.Mappers;
using Common.Mappers.Contracts;
using Ddei.Factories.Contracts;
using Ddei.Factories;
using Ddei.Mappers;
using Ddei.Options;
using DdeiClient.Mappers.Contract;
using DdeiClient.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Common.Services.DocumentToggle;

namespace Ddei.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddDdeiClient(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions<DdeiOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("ddei").Bind(settings);
            });
            var ddeiBaseUrl = new Uri(configuration.GetSection("ddei").GetValue<string>("BaseUrl"));
            services.AddTransient<ICaseDataArgFactory, CaseDataArgFactory>();
            services.AddTransient<IDdeiClient, DdeiClient>();
            services.AddHttpClient<IDdeiClient, DdeiClient>((client) =>
            {
                client.BaseAddress = ddeiBaseUrl;
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            services.AddTransient<IDdeiClientRequestFactory, DdeiClientRequestFactory>();
            services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, DdeiCaseDocumentMapper>();
            services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
            services.AddTransient<IDocumentToggleService, DocumentToggleService>();

            services.AddSingleton<ITransitionDocumentMapper, TransitionDocumentMapper>();
            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()
            ));

        }
    }
}