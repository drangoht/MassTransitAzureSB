using MassTransit;
using MassTransitAzureSB__Contracts;
using MassTransitAzureSB__Worker__Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Threading.Tasks;

namespace MassTransitAzureSB__Worker
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
            .ConfigureLogging( logging =>
            {
                logging.AddSerilog();
            })
                .ConfigureServices((hostContext, services) =>
                {
                    
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        // By default, sagas are in-memory, but should be changed to a durable
                        // saga repository.
                        //x.SetInMemorySagaRepositoryProvider();

                        var entryAssembly = Assembly.GetEntryAssembly();
                        x.AddConsumer<GettingStartedConsumer>();
                        //x.AddConsumers(entryAssembly);
                        //x.AddSagaStateMachines(entryAssembly);
                        //x.AddSagas(entryAssembly);
                        //x.AddActivities(entryAssembly);

                        string sbEndpoint = hostContext.Configuration.GetValue<string>("sb-endpoint");


                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(sbEndpoint);
                            cfg.ConfigureEndpoints(context);
                        });
                    });
                });
    }
}
