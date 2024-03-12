using System;
using System.Net.Http.Headers;
using Azure.Search.Documents;
using System.Threading.Tasks;
using coordinator.Clients;
using coordinator.Clients.Contracts;
using Common.Configuration;
using Common.Factories;
using Common.Factories.Contracts;
using text_extractor.Services.CaseSearchService;
using text_extractor.Factories.Contracts;
using text_extractor.Services.CaseSearchService;
using text_extractor.Factories;
using text_extractor.Mappers;
using text_extractor.Mappers.Contracts;
using text_extractor.Constants;
using coordinator.Factories;
using Common.Wrappers;
using Common.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using text_extractor.Services.OcrService;
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
            var testOcrService = ActivatorUtilities.CreateInstance<TestOcrService>(serviceProvider);

            Console.WriteLine();
            Console.WriteLine("Choose an option below:");
            Console.WriteLine("[1]: Insert a document index only");
            Console.WriteLine("[2]: Insert a document index then delete it");
            Console.WriteLine("[3]: Delete document indexes only");
            Console.WriteLine("[4]: Generate OCR results only");

            var optionInput = Console.ReadLine();

            Enum.TryParse(optionInput, out OperationOption operationOption);

            string fileToExtract = null;

            if (operationOption != OperationOption.DeleteOnly)
            {
                Console.WriteLine();
                Console.WriteLine("File size to extract?");
                Console.WriteLine("[S]mall: 100 lines");
                Console.WriteLine("[M]edium: 10,000 lines");
                Console.WriteLine("[L]arge: > 100,000 lines");


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
                case OperationOption.OcrOnly:
                    await testOcrService.GetOcrResultsAsync(fileToExtract);
                    Console.WriteLine($"OCR completed for {fileToExtract}");
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

            configuration["PolarisPipelineTextExtractorFunctionAppKey"] = configuration.GetSection("Values")[coordinator.Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey];
            configuration["SearchClientEndpointUrl"] = configuration.GetSection("Values")[ConfigKeys.SearchClientEndpointUrl];
            configuration["SearchClientIndexName"] = configuration.GetSection("Values")[ConfigKeys.SearchClientIndexName];
            configuration["SearchClientAuthorizationKey"] = configuration.GetSection("Values")[ConfigKeys.SearchClientAuthorizationKey];
            configuration["ComputerVisionClientServiceKey"] = configuration.GetSection("Values")[ConfigKeys.ComputerVisionClientServiceKey];
            configuration["ComputerVisionClientServiceUrl"] = configuration.GetSection("Values")[ConfigKeys.ComputerVisionClientServiceUrl];

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ITextExtractorService, TextExtractorService>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IPipelineClientRequestFactory, PipelineClientRequestFactory>();
            services.AddTransient<ITextExtractorClientRequestFactory, TextExtractorClientRequestFactory>();
            services.AddSingleton<ITestOcrService, TestOcrService>();
            services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
            services.AddSingleton<IOcrService, OcrService>();
            services.AddHttpClient<ITextExtractorClient, TextExtractorClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("Values").GetValueFromConfig(coordinator.Constants.ConfigKeys.PipelineTextExtractorBaseUrl));
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });
            AddSearchClient(services, configuration);

            return services.BuildServiceProvider();
        }
        private static void AddSearchClient(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions<SearchClientOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("searchClient").Bind(settings);
            });

            services.AddTransient<ISearchIndexService, SearchIndexService>();
            services.AddTransient<IAzureSearchClientFactory, AzureSearchClientFactory>();
            services.AddTransient<IStreamlinedSearchResultFactory, StreamlinedSearchResultFactory>();
            services.AddTransient<IStreamlinedSearchLineMapper, StreamlinedSearchLineMapper>();
            services.AddTransient<IStreamlinedSearchWordMapper, StreamlinedSearchWordMapper>();
            services.AddTransient<ISearchLineFactory, SearchLineFactory>();
            services.AddTransient<ISearchIndexingBufferedSenderFactory, SearchIndexingBufferedSenderFactory>();
        }

    }
}