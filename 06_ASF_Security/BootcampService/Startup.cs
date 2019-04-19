namespace Microsoft.Examples
{
    using System;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using  Microsoft.Extensions.Configuration.UserSecrets;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.PlatformAbstractions;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Fabric;
    using System.IO;
    using System.Reflection;

    using IdentityServer4.AccessTokenValidation;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Examples.Helpers;
    using Microsoft.IdentityModel.Logging;

    /// <summary>
    /// Represents the startup process for the application.
    /// </summary>
    public class Startup
    {
        private IHostingEnvironment environment;


        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="environment">The current configuration.</param>
        /// http://bootcamp2019-services-demo.northeurope.cloudapp.azure.com:8427/swagger/index.html 
        public Startup(IHostingEnvironment environment)
        {
            this.environment = environment;

            var configurationFileName = $"appsettings{(environment.IsProduction() ? string.Empty : "." + environment.EnvironmentName)}.json";
            
            var configurationSection = FabricRuntime.GetActivationContext()?
                .GetConfigurationPackageObject("Config")?
                .Settings?
                .Sections["BootcampConfiguration"];

            environment.EnvironmentName = configurationSection?.Parameters["ASPNETCORE_ENVIRONMENT"]?.Value;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configurationFileName, false, true)
                .AddUserSecrets<Startup>(false).AddEnvironmentVariables();

            if (environment.IsProduction() || environment.IsStaging())
            {
                var keyVaultEndpoint = configurationSection?.Parameters["KeyVaultEndpoint"]?.Value;

                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                }
            }

            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        /// <value>The current application configuration.</value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public void ConfigureServices(IServiceCollection services)
        {

            // the sample application always uses the latest version, but you may want an explicit version such as Version_2_2
            // note: Endpoint Routing is enabled by default; however, if you need legacy style routing via IRouter, change it to false
            services.AddMvc(options => options.EnableEndpointRouting = true).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                });
            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                options =>
                {
                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    // integrate xml comments
                    options.IncludeXmlComments(XmlCommentsFilePath);
                });
            services.AddSingleton<IConfiguration>(Configuration);

            IdentityModelEventSource.ShowPII = true;
            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                        options.Secure = CookieSecurePolicy.Always;
                    });

            services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigin",
                        builder =>
                            {
                                builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                                builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                                builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                            });

                });

            var identityProviderConfig = this.Configuration.GetSection("IdentityProviderConfig").Get<IdentityProviderConfig>();


            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = identityProviderConfig.Url;
                        options.ApiName = "resourceApi";
                        options.RequireHttpsMetadata = true;
                    });

            services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        "registeredUser",
                        policyAdmin =>
                            {
                                policyAdmin.RequireClaim("scope", "yourFancyApi");
                            });
                });

            services.AddTransient<IAuthorizationHelper, AuthorizationHelper>();
        }

        /// <summary>
        /// Configures the application using the provided builder, hosting environment, and API version description provider.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="env">The current hosting environment.</param>
        /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    // build a swagger endpoint for each discovered API version
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }
    }
}