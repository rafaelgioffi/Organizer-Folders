using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SortFIlesDown
{
    public class Program
    {
        static void Main(string[] args)
         => HostBuilder(args).GetAwaiter().GetResult();
    
       public static async Task HostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseWindowsService()
                .ConfigureServices(_ =>
                {
                    _.AddHostedService<Process>();
                
                }).Build();

            await host.RunAsync();  
        }
    }

}
