namespace Microsoft.Extensions.DependencyInjection
{
    public static  class ApplicationInsigtsExtensions
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-coreNow
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOfgemCloudApplicationInsightsTelemetry(this IServiceCollection services)
        {
            ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
            = new ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
            // Disables adaptive sampling.
            aiOptions.EnableAdaptiveSampling = false;

            // Disables QuickPulse (Live Metrics stream).
            aiOptions.EnableQuickPulseMetricStream = false;

            services.AddApplicationInsightsTelemetry(aiOptions);

            return services;
        }
    }
}
