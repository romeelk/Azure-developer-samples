using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;

ServiceBusClient client;

ServiceBusProcessor orderProcessor;

Console.WriteLine("Welcome to Service bus order processor");

string servicebusNameSpace=Environment.GetEnvironmentVariable("namespace");
string serviceBusQueue=Environment.GetEnvironmentVariable("queue");

if(string.IsNullOrEmpty(servicebusNameSpace))
{
    Console.WriteLine("Missing service bus namespace environment var missing");
    Environment.Exit(0);
}

if(string.IsNullOrEmpty(serviceBusQueue))
{
    Console.WriteLine("Service bus queue environment var missing");
    Environment.Exit(0);
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    string orderMesage = args.Message.Body.ToString();
    Console.WriteLine($"Order message received {orderMesage}");
    await args.CompleteMessageAsync(args.Message);
}

Task OrderErrorHandler(ProcessErrorEventArgs errorArgs)
{
    Console.WriteLine($"There was an error {errorArgs.Exception.ToString()}");
    return Task.CompletedTask;
}

var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
client = new ServiceBusClient(
            servicebusNameSpace,
            new DefaultAzureCredential(),
            clientOptions);

orderProcessor = client.CreateProcessor(serviceBusQueue, new ServiceBusProcessorOptions());

try
{
    orderProcessor.ProcessMessageAsync+=MessageHandler;

    orderProcessor.ProcessErrorAsync+=OrderErrorHandler;

    Console.WriteLine("Starting processing order messages from order queue");

    await orderProcessor.StartProcessingAsync();

    Console.WriteLine("Hit any key to stop processing");

    Console.ReadKey();

    await orderProcessor.StopProcessingAsync();
    Console.WriteLine("Stopped processing order messages");

}
catch(ServiceBusException exception)
{
        Console.WriteLine($"An error occured with Service bus {exception.ToString()}");
}
finally
{
    await orderProcessor.DisposeAsync();
    await client.DisposeAsync();
}