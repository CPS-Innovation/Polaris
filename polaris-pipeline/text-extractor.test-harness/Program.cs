using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common.Clients;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Services.CaseSearchService;
using Common.Services.Extensions;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TextExtractor.TestHarness.Constants;
using TextExtractor.TestHarness.Services;

namespace TextExtractor.TestHarness
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider(args);

            var textExtractorService = ActivatorUtilities.CreateInstance<TextExtractorService>(serviceProvider);
            var searchIndexService = ActivatorUtilities.CreateInstance<SearchIndexService>(serviceProvider);

            Console.WriteLine();
            Console.WriteLine("Choose an option below:");
            Console.WriteLine("[1]: Insert a document index only");
            Console.WriteLine("[2]: Insert a document index then delete it");
            Console.WriteLine("[3]: Delete document indexes only");

            var optionInput = Console.ReadLine();

            Enum.TryParse(optionInput, out OperationOption operationOption);

            string fileToExtract = null;

            if (operationOption != OperationOption.DeleteOnly)
            {
                Console.WriteLine();
                Console.WriteLine("File size to extract? [S]mall/[M]edium/[L]arge");

                string fileInput = Console.ReadLine();

                fileToExtract = fileInput.ToUpperInvariant() switch
                {
                    "S" => SourceFile.Small,
                    "M" => SourceFile.Medium,
                    "L" => SourceFile.Large,
                    _ => throw new Exception("File input value not recognised.")
                };
            }

            switch (operationOption)
            {
                case OperationOption.InsertOnly:
                    await textExtractorService.ExtractAndStoreDocument(fileToExtract);
                    Console.WriteLine($"{fileToExtract} has been inserted.");
                    break;
                case OperationOption.InsertDelete:
                    await textExtractorService.ExtractAndStoreDocument(fileToExtract);
                    Console.WriteLine($"{fileToExtract} has been inserted.");
                    await searchIndexService.RemoveCaseIndexEntriesAsync(TestProperties.CmsCaseId);
                    Console.WriteLine($"{fileToExtract} has been deleted.");
                    break;
                case OperationOption.DeleteOnly:
                    await searchIndexService.RemoveCaseIndexEntriesAsync(TestProperties.CmsCaseId);
                    Console.WriteLine($"Document indexes for case {TestProperties.CmsCaseId} have been deleted.");
                    break;
                default:
                    throw new Exception("Option input was not recognised.");
            }

            Console.WriteLine("Operation completed.");
        }

        static ServiceProvider BuildServiceProvider(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args)
                            .Build();

            configuration["PolarisPipelineTextExtractorFunctionAppKey"] = configuration.GetSection("Values")[PipelineSettings.PipelineTextExtractorFunctionAppKey];
            configuration["SearchClientEndpointUrl"] = configuration.GetSection("Values")[ConfigKeys.SharedKeys.SearchClientEndpointUrl];
            configuration["SearchClientIndexName"] = configuration.GetSection("Values")[ConfigKeys.SharedKeys.SearchClientIndexName];
            configuration["SearchClientAuthorizationKey"] = configuration.GetSection("Values")[ConfigKeys.SharedKeys.SearchClientAuthorizationKey];

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITextExtractorService, TextExtractorService>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            services.AddTransient<IPipelineClientSearchRequestFactory, PipelineClientSearchRequestFactory>();
            services.AddHttpClient<ITextExtractorClient, TextExtractorClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("Values").GetValueFromConfig(PipelineSettings.PipelineTextExtractorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            services.AddSearchClient(configuration);

            return services.BuildServiceProvider();
        }
    }
}