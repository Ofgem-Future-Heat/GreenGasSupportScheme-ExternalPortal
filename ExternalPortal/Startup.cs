using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using ExternalPortal.Configuration;
using ExternalPortal.Models;
using ExternalPortal.Services;
using GovUkDesignSystem.ModelBinders;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ExternalPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            });
            
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C");
            
            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.ResponseMode = OpenIdConnectResponseMode.FormPost;
                options.Events = new OpenIdConnectEvents
                {
                    OnMessageReceived = async context => {
                        if (context.HttpContext.Request.Form.ContainsKey("id_token"))
                        {
                            var token = new JsonWebToken(context.Request.Form["id_token"]);

                            var userExists = await CheckUserExists(context, token);

                            if (!userExists)
                            {
                                await PersistUser(context, token);
                            }
                        }
                    },
                    OnRemoteFailure = context =>
                    {
                        if (context.HttpContext.Request.Form.ContainsKey("id_token"))
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/dashboard");
                            return Task.FromResult(0);
                        }
                        context.HandleResponse();
                        context.Response.Redirect("/");
                        return Task.FromResult(0);
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/");
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services
                .AddRazorPages()
                .AddMicrosoftIdentityUI();

            services.AddOfgemCloudApplicationInsightsTelemetry();

            services.AddApplicationInsightsKubernetesEnricher();

           if(Environment.EnvironmentName.ToLower() == "dr")
            {
                Configuration.GetSection("Redis:Server").Value = Configuration.GetSection("Redis:DisasterRecoveryServer").Value;
                Configuration.GetSection("Redis:PrimaryKey").Value = Configuration.GetSection("Redis:DisasterRecoveryPrimaryKey").Value;
            }

            services.UseAzureRedisData(Configuration);

            services.AddScoped<IRedisCacheService, RedisCacheService>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".ggss.ext.session";
            });

            services.AddControllersWithViews(options =>
                options.ModelMetadataDetailsProviders.Add(new GovUkDataBindingErrorTextProvider()));
           
            services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitialiser>();

            services.AddOfgemCloudApplicationInsightsTelemetry();

            services.UseExternalServices(Configuration, Environment.IsDevelopment());

            services.AddScoped<UserProfile>(sp => UserProfileBuilder.CreateForCurrentUser(sp));

            services.AddTransient<IAppVersionService, AppVersionService>();

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddRouting(option =>
            {
                option.ConstraintMap["slugify"] = typeof(Helpers.SlugifyParameterTransformer);
            });

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                RequireHeaderSymmetry = false,
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller:slugify}/{action:slugify}/{id:slugify?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        public class CustomTelemetryInitialiser : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                if (telemetry == null) return;
                telemetry.Context.Cloud.RoleName = "GGS-ExternalPortal-WebApp";
            }
        }
        
        private static async Task<bool> CheckUserExists(MessageReceivedContext context, JsonWebToken token)
        {
            var getUserByProviderId = context.HttpContext.RequestServices.GetService<IGetUserByProviderIdService>();
            var user = await getUserByProviderId.Get(new GetUserRequest { ProviderId = token.Subject });
            return user.Found;
        }
        
        private static async Task PersistUser(MessageReceivedContext context, JsonWebToken token)
        {
            var addUser = context.HttpContext.RequestServices.GetService<IAddUserService>();
            await addUser.Add(new AddUserRequest()
            {
                ProviderId = token.Subject,
                Email = token.Claims.SingleOrDefault(Claim => Claim.Type == ClaimTypes.Email)?.Value
                        ?? token.Claims.SingleOrDefault(Claim => Claim.Type == "signInNames.emailAddress")?.Value,
                Name = token.Claims.SingleOrDefault(Claim => Claim.Type == "given_name")?.Value,
                Surname = token.Claims.SingleOrDefault(Claim => Claim.Type == "family_name")?.Value
            }, CancellationToken.None);
        }
    }
}
