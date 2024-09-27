using MassTransit;
using MassTransitAzureSB__Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Serilog;
using System.IO;
using Serilog.Events;
namespace MassTransitAzureSB
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((host, log) =>
                {
                    if (host.HostingEnvironment.IsProduction())
                        log.MinimumLevel.Information();
                    else
                        log.MinimumLevel.Debug();

                    log.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
                    log.MinimumLevel.Override("Quartz", LogEventLevel.Information);
                    log.WriteTo.Console();
                })
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddUserSecrets<Program>().Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        string sbEndpoint = hostContext.Configuration.GetValue<string>("sb-endpoint");

                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(sbEndpoint);
                            cfg.ConfigureEndpoints(context);
                        });
                    });
                    services.AddHostedService<Worker>();
                });
    }
}
