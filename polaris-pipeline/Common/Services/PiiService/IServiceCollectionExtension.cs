
using Common.Services.PiiService.AllowedWords;
using Common.Services.PiiService.Chunking;
using Common.Services.PiiService.Mappers;
using Common.Services.PiiService.TextAnalytics;
using Common.Services.PiiService.TextSanitization;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Services.PiiService
{
    public static class IServiceCollectionExtension
    {
        public static void AddPiiService(this IServiceCollection services)
        {
            services.AddSingleton<IPiiChunkingService, PiiChunkingService>();
            services.AddSingleton<IPiiService, PiiService>();
            services.AddSingleton<ITextAnalyticsClientFactory, TextAnalyticsClientFactory>();
            services.AddSingleton<ITextAnalysisClient, TextAnalysisClient>();
            services.AddSingleton<IPiiEntityMapper, PiiEntityMapper>();
            services.AddSingleton<IPiiAllowedListService, PiiAllowedListService>();
            services.AddSingleton<IPiiAllowedList, PiiAllowedList>();
            services.AddSingleton<ITextSanitizationService, TextSanitizationService>();
        }
    }
}