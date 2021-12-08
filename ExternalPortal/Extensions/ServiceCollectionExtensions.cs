using System;
using System.Net.Http;
using System.Threading.Tasks;
using ExternalPortal.Configuration;
using ExternalPortal.Services;
using Microsoft.Extensions.Configuration;
using Ofgem.API.GGSS.Domain.Contracts.Services;
using Polly;
using Polly.Extensions.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseExternalServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            services.AddOptions();
            services.Configure<ServiceConfig>(o => configuration.GetSection("Services").Bind(o));

            var api = configuration.GetSection("Services:Api").Get<ApiConfig>();

            var documentsApiBaseUri = api.DocumentsApiBaseUri;

            services.AddHttpClient<ISaveDocumentService, SaveDocumentService>(client
                =>
            { client.BaseAddress = new Uri(documentsApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IDeleteDocumentService, DeleteDocumentService>(client
                    =>
            { client.BaseAddress = new Uri(documentsApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IGetDocumentService, GetDocumentService>(client
                   =>
            { client.BaseAddress = new Uri(documentsApiBaseUri); })
               .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IGetCompaniesHouseService, GetCompaniesHouseService>(client
                   =>
            { client.BaseAddress = new Uri(api.CompaniesHouseApiBaseUri); })
               .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<ICreateApplicationService, CreateApplicationService>(client
                    =>
                { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
            .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IGetApplicationService, GetApplicationService>(client
                        =>
                    { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IGetOrganisationService, GetOrganisationService>(client
                        =>
                    { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));
            
            services.AddHttpClient<IGetOrganisationDetailsService, GetOrganisationDetailsService>(client
                        =>
                    { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IUpdateApplicationService, UpdateApplicationService>(client
                        =>
                    { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IApplicationService, ApplicationService>(client
                =>
            { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddScoped<IResponsiblePersonService, ResponsiblePersonService>();

            services.AddHttpClient<IGetOrganisationsForUserService, GetOrganisationsForUserService>(client
                    =>
            { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<ISendEmailService, SendEmailService>(client
                =>
            { client.BaseAddress = new Uri(api.NotificationApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));

            services.AddHttpClient<IOrganisationService, OrganisationService>(client
                =>
              {
                  client.BaseAddress = new Uri(api.InternalApiBaseUri);
              }).AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));
            
            services.AddHttpClient<IAddUserService, AddUserService>(client
                => { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                        .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));
            
            services.AddHttpClient<IGetUserByProviderIdService, GetUserByProviderIdService>(client
                    => { client.BaseAddress = new Uri(api.InternalApiBaseUri); })
                .AddPolicyHandler(GetRetryPolicy(api.RetryCount, api.RetryIntervalSeconds));


            if (isDevelopment)
            {
                services.AddScoped<IGetCompaniesHouseService, FakeGetCompaniesHouseService>();
            }

            services.AddLogging();

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount, double retryInterval)
        {
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound);

            if (retryCount != default || retryInterval != default)
            {
                return policy
                    .WaitAndRetryAsync(retryCount, retryAttempt
                        => TimeSpan.FromSeconds(Math.Pow(retryInterval, retryAttempt)));
            }

            return Task.FromResult(policy) as IAsyncPolicy<HttpResponseMessage>;
        }
    }
}
