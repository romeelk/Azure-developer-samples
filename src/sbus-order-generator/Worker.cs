namespace sbus_order_generator;

using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Microsoft.Extensions.Options;


public class Worker(ILogger<Worker> logger,IOptions<ServiceBusConfigOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {   
            // Create a DefaultAzureCredentialOptions object to configure the DefaultAzureCredential
        DefaultAzureCredentialOptions defaultCreds = new()
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true
        };  

        ServiceBusClient client = new(options.Value.NameSpace, new DefaultAzureCredential(defaultCreds));
        
        ServiceBusSender sender = client.CreateSender(options.Value.QueueName);

        logger.LogInformation("About to generate messages for queue: {} and namespace {}", options.Value.QueueName, options.Value.NameSpace);
        
        int messageCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
           
            await SentMessage(sender, messageCount);
            messageCount++;
            await Task.Delay(5000, stoppingToken);
        }
    }

    protected async Task SentMessage(ServiceBusSender sender, int messageNumber)
    {
        try 
        {
            await sender.SendMessageAsync(new ServiceBusMessage($"Order no:{messageNumber}"));
            logger.LogInformation("Sent order: {messageNumber}", messageNumber);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }
}
