using sbus_order_generator;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Bind configuration section to options
        services.Configure<ServiceBusConfigOptions>(
            context.Configuration.GetSection("ServiceBusConfig"));

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
