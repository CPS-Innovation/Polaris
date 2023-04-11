using Common.Dto.Response;
using Common.Mappers;
using Common.Mappers.Contracts;
using Ddei.Factories.Contracts;
using Ddei.Factories;
using Ddei.Mappers;
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
        private const string FunctionKey = "x-functions-key";

        public static void AddDdeiClient(this IServiceCollection services, IConfigurationRoot configuration)
        {                
            services.AddTransient<ICaseDataArgFactory, CaseDataArgFactory>();
            services.AddHttpClient<IDdeiClient, DdeiClient>((service, client) =>
            {
                client.BaseAddress = new Uri(configuration["DdeiBaseUrl"]);
                client.DefaultRequestHeaders.Add(FunctionKey, configuration["DdeiAccessKey"]);
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