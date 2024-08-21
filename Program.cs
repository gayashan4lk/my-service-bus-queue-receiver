using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
    .Build();

var SERVICE_BUS_NAMESPACE_CONNECTION_STRING = configuration["AppSettings:SERVICE_BUS_NAMESPACE_CONNECTION_STRING"];
var SERVICE_BUS_QUEUE_NAME = configuration["AppSettings:SERVICE_BUS_QUEUE_NAME"];

Console.WriteLine($"SERVICE_BUS_NAMESPACE_CONNECTION_STRING:{SERVICE_BUS_NAMESPACE_CONNECTION_STRING}");
Console.WriteLine($"SERVICE_BUS_QUEUE_NAME:{SERVICE_BUS_QUEUE_NAME}");

var client = new ServiceBusClient(
    SERVICE_BUS_NAMESPACE_CONNECTION_STRING,
    new ServiceBusClientOptions()
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    });

var processor = client.CreateProcessor(SERVICE_BUS_QUEUE_NAME, new ServiceBusProcessorOptions());

try
{
    processor.ProcessMessageAsync += MessageHandler;

    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    // start processing
    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine($"Exception occured: {args.Exception}");
    return Task.CompletedTask;
}