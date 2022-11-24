namespace CatalogService.Api.Extensions
{
    public  static class ConfigurationExtensions
    {
        public static IConfiguration AddSystemConfiguration(this IConfiguration configuration,IWebHostEnvironment env) 
        {
            return new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile($"Configurations/appsettings.json",optional:false)
                .AddJsonFile($"Configurations/appsettings.{env}.json",optional:true)
                .AddEnvironmentVariables()
                .Build();
            
        }
        public static IConfiguration AddSerilogConfiguration(this IConfiguration configuration, IWebHostEnvironment env) 
        {
            return new ConfigurationBuilder()
                   .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                   .AddJsonFile($"Configurations/serilog.json", optional: false)
                   .AddJsonFile($"Configurations/serilog.{env}.json", optional: true)
                   .AddEnvironmentVariables()
                   .Build();
        }
    }
}
