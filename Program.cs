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

                .UseWindowsService()
                .ConfigureServices(_ =>
                {
                    _.AddHostedService<Process>();

                }).Build();

            await host.RunAsync();

        }
    }

}
