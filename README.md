# Global Azure Bootcamp 2019 workshop. 𝐅𝐫𝐨𝐦 𝐖𝐞𝐛 𝐀𝐏𝐈 𝐭𝐨 𝐀𝐳𝐮𝐫𝐞 𝐒𝐞𝐫𝐯𝐢𝐜𝐞 𝐅𝐚𝐛𝐫𝐢𝐜 𝐬𝐞𝐫𝐯𝐢𝐜𝐞 𝐢𝐧 𝟏𝟐𝟎 𝐦𝐢𝐧𝐮𝐭𝐞𝐬.
# Stanislav Lebedenko
-------------------------------------------
So we all want to work with the fancy and newest tech, but sometimes we are stuck with a legacy project and it seems that there is no way out. But there is hope, you can try to start small, with time you have at the moment and migrate it to reliable and scalable microservices. This way you will create a bridgehead for further project expansion.

My intention is to give you a taste of Azure Service Fabric with pragmatic and easy jumpstart steps so you will be ready to migrate your project and survive this process afterwards :).
Welcome, it will be easy and fun, also you will get a checklist for the real-life migration.

-------------------------------------------
Web API example is Microsoft API versioning with Swagger
Source repository:  https://github.com/Microsoft/aspnet-api-versioning/tree/master/samples/aspnetcore/SwaggerSample

-------------------------------------------

Service Fabric project from Visual studio template in Cloud section
Please choose Visual C# => Cloud => Service Fabric Application => Stateless ASP.NET Core without authentication 

Steps to work with application.

Adding the Web Api project to your ASF application in several steps

1. Copy your API folder to already created ASF project
2. Rename folder from BootcampApi to BootcampService
3. Rename BootcampApi.csproj to BootcampService.csproj
4. Add project to your solution
5. Open and replace/add sections to BootcampService.scproj file
		  <PropertyGroup>
		    <TargetFramework>netcoreapp2.2</TargetFramework>
		    <IsServiceFabricServiceProject>True</IsServiceFabricServiceProject>
		    <ServerGarbageCollection>True</ServerGarbageCollection>
		    <RuntimeIdentifier>win7-x64</RuntimeIdentifier>
		    <TargetLatestRuntimePatch>False</TargetLatestRuntimePatch>
		    <RootNamespace>Microsoft.Examples</RootNamespace>
		   <GenerateDocumentationFile>true</GenerateDocumentationFile>
		  </PropertyGroup>
		
		  <ItemGroup>
		    <Folder Include="wwwroot\" />
		  </ItemGroup>
		
6. Rebuild solution.
7. Add nuget Microsoft.ServiceFabric.AspNetCore.Kestrel to BootcampService
8. Copy WebService.cs to BootcampService project and rename it to BootcampService.cs
9. Change namespace in BootcampService.cs to Microsoft.Examples
10. Copy ServiceEventSource.cs to BootcampService project
11. Change namespace in ServiceEventSource.cs to Microsoft.Examples
12. Copy Main() method from Program.cs in WebService project and replace Main() in Program.cs BootcampApi project.
13. Program.cs : rename WebService to BootcampService
14. Program.cs : rename WebServiceType to BootcampServiceType
15. Replace string in Startup.cs .SetCompatibilityVersion( Latest ); with .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
16. Delete CreateWebHostBuilder() method in Program.cs at BootcampApi project.
17. Copy folder PackageRoot to  to BootcampService project
18. ServiceManifest.xml : rename WebService text to BootcampService

Lets move on to ServiceFabricBootcampDemo project.
1. Open folder ApplicationPackageRoot folder and ApplicationManifest.xml
2. Open Visual studio replace dialog ctrl+H 
	a. Enter find value WebService
	b. Enter replace value BootcampService
	c. Select Current project from dropdown
	d. Hit replace
3. Right click on Services in project and select Add => Existing Service Fabric Service in Solution
	a. Select BootcampService
	b. Deselect WebService
4. Right click on ServiceFabricBootcampDemo select properties, make sure that Debug selected, 
	Set Application URL parameter to http://localhost:8427/swagger/index.html for easy debug.
5. 
6. Comment out 
	 // integrate xml comments
	 //options.IncludeXmlComments( XmlCommentsFilePath );

Configuration phase
1. First install following nuget packages
    <PackageReference Include="Microsoft.Azure.KeyVault" Version="3.0.3" />
    <PackageReference Include="Microsoft.Azure.Services.AppAuthentication" Version="1.0.3" />
    <PackageReference Include="Microsoft.Extensions.Configuration.AzureKeyVault" Version="2.2.0" />
2. Add secrets key to csproj file  <UserSecretsId>2e1ea454-3747-4351-831e-a86bff66b67e</UserSecretsId>
3. Create appSettings.Development.json and appSettings.Staging.json
4. Then replace method Startup in startup.cs with following code
   public Startup(IHostingEnvironment environment)
	        {
	            this.environment = environment;
	
	            var configurationFileName = $"appsettings{(environment.IsProduction() ? string.Empty : "." +    environment.EnvironmentName)}.json";
	            
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
5. Proceed with changes to ASF configuration files
6. Add following section to PackageRoot/Config/Settings.xml
	  <Section Name="BootcampConfiguration">
	    <Parameter Name="ASPNETCORE_ENVIRONMENT" Value="Development" />
	    <Parameter Name="AppInsightsKey" Value="" />
	    <Parameter Name="KeyVaultEndpoint" Value="" />
	  </Section>
	
7. Comment out section below in PackageRoot/ServiceManifest
	    <!--<EnvironmentVariables>
	      <EnvironmentVariable Name="ASPNETCORE_ENVIRONMENT" Value=""/>
	    </EnvironmentVariables>-->
	
8. Open ServiceFabricBootcampDemo
9. ApplicationRoot/ApplicationManifest.xml add to Parameters section
	    <Parameter Name="ASPNETCORE_ENVIRONMENT" DefaultValue="" />
	    <Parameter Name="AppInsightsKey" DefaultValue="" />
	    <Parameter Name="KeyVaultEndpoint" DefaultValue="" />
	
	Delete sections ConfigOverrides and EnvironmentOverrides
	Add new section.
	    <ConfigOverrides>
	      <ConfigOverride Name="Config">
	        <Settings>
	          <Section Name="BootcampConfiguration">
	            <Parameter Name="ASPNETCORE_ENVIRONMENT" Value="[ASPNETCORE_ENVIRONMENT]" />
	            <Parameter Name="AppInsightsKey" Value="[AppInsightsKey]" />
	            <Parameter Name="KeyVaultEndpoint" Value="[KeyVaultEndpoint]" />
	          </Section>
	        </Settings>
	      </ConfigOverride>
	    </ConfigOverrides>
	
10. Open folder ApplicationParameters and add parameters below to cloud file
	    <Parameter Name="ASPNETCORE_ENVIRONMENT" Value="Production" />
	    <Parameter Name="AppInsightsKey" Value="d4602dc5-2587-436e-bc88-988456ed33c2" />
	    <Parameter Name="KeyVaultEndpoint" Value="https://bootcamp2019demovault.vault.azure.net/" />
	
11. Continue with Nuget package installation
	    <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.6.1" />
	    <PackageReference Include="Microsoft.ApplicationInsights.ServiceFabric.Native" Version="2.2.2" />
	Downgrade following package
	    <PackageReference Include="Microsoft.ServiceFabric.AspNetCore.Kestrel" Version="3.2.162" />
	
12. Add usings to 
13. Add to the BootcampService.cs update to Configure Services
	                                      services => services
	                                            .AddSingleton<StatelessServiceContext>(serviceContext))
	                                            .AddSingleton<StatelessServiceContext>(serviceContext)
	                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext))
	                                            .AddSingleton<ITelemetryModule>(new ServiceRemotingDependencyTrackingTelemetryModule())
	                                            .AddSingleton<ITelemetryModule>(new ServiceRemotingRequestTrackingTelemetryModule()))
	Also add configuration below.
	                                    .UseApplicationInsights()
	
14. Inject configuration in Startup.cs
	            services.AddSingleton<IConfiguration>(Configuration);
15. Update OrderController V3 
	Add constructor 
	        private readonly IConfiguration config;
	
	        private TelemetryClient telemetry = new TelemetryClient(TelemetryConfiguration.Active);
	
	        public OrdersController(IConfiguration config)
	        {
	            this.config = config;
	        }
	Upgate action 
	        public IActionResult Get(int id)
	        {
	            telemetry.TrackEvent($"Someone fetched Order with API {id}");
	
	            var secret = this.config != null ? this.config["FancySecret"] ?? $"Not found" : "Fancy secret not found";
	
	            return this.Ok(new Order() { Id = id, Customer = secret });
	        }
	
16. Security
17. Add references in BootcampService csproj file
	    <PackageReference Include="IdentityServer4.AccessTokenValidation" Version="2.6.0" />
	    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="5.3.0" />
	    <PackageReference Include="Microsoft.IdentityModel.Clients.ActiveDirectory" Version="3.19.8" />
	
18. Add Folder Configuration, add IdentityProviderConfig.cs
	 public class IdentityProviderConfig
	    {
	        public string Url { get; set; }
	    }
	
	
	
19. Add CertificateConfiguration.cs
	 public class CertificateConfiguration
	    {
	        public static X509Certificate2 GetCertificate()
	        {
	            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
	            try
	            {
	                certStore.Open(OpenFlags.ReadOnly);
	
	                var certificates = certStore.Certificates;
	
	                var domainCertificates = certificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=YouFancyDomain", false);
	
	                if (domainCertificates.Count > 0)
	                {
	                    return domainCertificates[0]; // validate certificate with thumbprint
	                }
	                else
	                {
	                    var localhostCertificates = certificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=localhost", false);
	
	                    return localhostCertificates.Count == 0 ? null : localhostCertificates[0];
	                }
	            }
	            finally
	            {
	                certStore.Close();
	            }
	        }
	    }
20. Add folder Helpers, add class AuthorizationHelper and interface
	  public class AuthorizationHelper : IAuthorizationHelper
	    {
	        private readonly TelemetryClient telemetry = new TelemetryClient(TelemetryConfiguration.Active);
	
	
	        public bool ValidateClaims(ClaimsPrincipal authorizedUser, string fancyClaimValue)
	        {
	            var userClaims = authorizedUser.Claims.ToList();
	            var userEmail = userClaims.FirstOrDefault(x => x.Type.Contains("email"))?.Value;
	
	            foreach (var claim in userClaims)
	            {
	                if (claim.Value != fancyClaimValue)
	                {
	                    continue;
	                }
	
	                this.telemetry.TrackEvent($"Authorization check success for user {userEmail}, to fancy claimValue {fancyClaimValue}, on {DateTime.Now}");
	                return true;
	            }
	
	            this.telemetry.TrackEvent($"Authorization violation by user {userEmail}, to fancy claimValue {fancyClaimValue}, on {DateTime.Now}");
	
	            return false;
	        }
	    }
	
21. Replace port configuration in solution 
	  <Endpoint Protocol="http" Name="ServiceEndpoint" Type="Input" Port="8427" />
	  To
		<Endpoint Protocol="https" Name="ServiceEndpoint" Type="Input" Port="443" />
		
22. Add in BootcampService/PackageRoot/ServiceManifest.xml section after CodePackage Name="Code"
	    <SetupEntryPoint>
	      <ExeHost>
	        <Program>Setup.bat</Program>
	        <WorkingFolder>CodePackage</WorkingFolder>
	      </ExeHost>
	    </SetupEntryPoint>
	
23. Generate self signed certificate
	  cd './program files/microsoft sdks/service fabric/clustersetup/secure'
	  .\CertSetup.ps1 -Install -CertSubjectName CN=localhost

25. Crucial point, Export generated in 23 cert with password and install self signed certificate to you VMSS instances. Wait for installation finis
	Connect-AzureRmAccount

	$vaultname="yourFancyVault"
	$certname="SomeCertName"
	$certpw="SuperPassword"
	$groupname="your-fabric-group"
	$clustername = "your-cluster-name"
	$ExistingPfxFilePath="C:\certificates\SomeCertName.pfx"
	
	$appcertpwd = ConvertTo-SecureString -String $certpw -AsPlainText -Force
	
	Write-Host "Reading pfx file from $ExistingPfxFilePath"
	$cert = new-object System.Security.Cryptography.X509Certificates.X509Certificate2 $ExistingPfxFilePath, $certpw

	$bytes = [System.IO.File]::ReadAllBytes($ExistingPfxFilePath)
	$base64 = [System.Convert]::ToBase64String($bytes)

	$jsonBlob = @{
	   data = $base64
	   dataType = 'pfx'
	   password = $certpw
	   } | ConvertTo-Json

	$contentbytes = [System.Text.Encoding]::UTF8.GetBytes($jsonBlob)
	$content = [System.Convert]::ToBase64String($contentbytes)

	$secretValue = ConvertTo-SecureString -String $content -AsPlainText -Force

# Upload the certificate to the key vault as a secret
Write-Host "Writing secret to $certname in vault $vaultname"
$secret = Set-AzureKeyVaultSecret -VaultName $vaultname -Name $certname -SecretValue $secretValue

# Add a certificate to all the VMs in the cluster.
Add-AzureRmServiceFabricApplicationCertificate -ResourceGroupName $groupname -Name $clustername -SecretIdentifier $secret.Id -Verbose
24. Add two files from Repo.
	  Setup.bat and SetCertAccess.ps1 and set in options copy if newer for both files.
	
25. Add section to appSetting files
	  "AllowedHosts": "*",
	  "IdentityProviderConfig": {
	    "Url": "https://microsoft.com/"
	  }
26. Change Kestrel config in BootCampService.cs
	                            .UseKestrel(options =>
	                                {
	                                    var port = serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint").Port;
	                                    options.Listen(IPAddress.IPv6Any, port, listenOptions =>
	                                        {
	                                            listenOptions.UseHttps(CertificateConfiguration.GetCertificate());
	                                            listenOptions.NoDelay = true;
	                                        });
	                                })
27. Add to V3 OrdersController atrribute    
  [Authorize("registeredUser")]

