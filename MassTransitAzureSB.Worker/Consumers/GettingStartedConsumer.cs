using MassTransit;
using MassTransitAzureSB__Contracts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MassTransitAzureSB__Worker__Consumers
{
    public class GettingStartedConsumer :
        IConsumer<GettingStarted>
    {
        readonly ILogger<GettingStartedConsumer> _logger;

        public GettingStartedConsumer(ILogger<GettingStartedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<GettingStarted> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.Value);
            return Task.CompletedTask;
        }
    }
}

