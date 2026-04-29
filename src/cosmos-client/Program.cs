// See https://aka.ms/new-console-template for more information
using cosmos_client;
using dotenv.net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

Console.WriteLine("Welcome to cosmos client!");


var envVars = DotEnv.Read();

var cosmosEndpointUrl = envVars["DOCUMENT_ENDPOINT"];
var accountKey = envVars["ACCOUNT_KEY"];

var databaseName = "products";
var containerName = "items";
var storedProcId = "upsertItem";

async Task CreateItemWithStoredProcedure(Container container,Item item)
{
    StoredProcedureExecuteResponse<Item> result =
    await container.Scripts.ExecuteStoredProcedureAsync<Item>(
        storedProcedureId: "upsertItem",
        partitionKey: new PartitionKey(item.Id),
        parameters:  [item] 
    );

    Console.WriteLine($"Created document with Id using stored procedure: {result.Resource.Id}");
}
async Task RegisterStoreProcedureAsync(Container container, string storedProcId)
{
    string storedProcBody = File.ReadAllText("./stored-procs/spCreateItem.js");

    // Create the stored procedure definition
    StoredProcedureProperties sprocProperties = new()
    {
        Id = storedProcId,
        Body = storedProcBody
    };

    // Register it with the container
    StoredProcedureResponse response = await container.Scripts.CreateStoredProcedureAsync(sprocProperties);
    Console.WriteLine($"Stored procedure created: {response.Resource.Id}");
}

async Task<bool> StoredProcedureExistsAsync(Container container, string storedProcId)
{
    try
    {
        StoredProcedureProperties storedProcedure = await container.Scripts.ReadStoredProcedureAsync(storedProcId);
        return true;

    }
    catch (CosmosException cosmosException) when (cosmosException.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.WriteLine("Comsosdb storeproc: {storeProcId} was not found");
        return false;
    }
}
if(string.IsNullOrEmpty(cosmosEndpointUrl) || string.IsNullOrEmpty(accountKey))
{
    Console.WriteLine("Please set the endpoint and account key environment variables");
    return;
}

try
{
    var cosmosClient = new CosmosClient(accountEndpoint: cosmosEndpointUrl, authKeyOrResourceToken: accountKey);


    DatabaseResponse databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
    
    if(databaseResponse.StatusCode == System.Net.HttpStatusCode.Created)
    {
        Console.WriteLine("Created product db");
    }
    else
    {
        Console.WriteLine("Database product db already exists!");
    }
    Container container = await databaseResponse.Database.CreateContainerIfNotExistsAsync(id:containerName, partitionKeyPath: "/id");

    if(!await StoredProcedureExistsAsync(container, storedProcId))
    {
        await RegisterStoreProcedureAsync(container, storedProcId);
        Console.WriteLine($"Created stored procedure with Id:{storedProcId}");

    }
    else
    {
        Console.WriteLine($"Stored procedure with Id {storedProcId} already exists!");
    }
   
    var item = new Item(){Description="test",Price=2,Name="newtest",Id=Guid.NewGuid().ToString()};

    await CreateItemWithStoredProcedure(container, item);

    var item2 = new Item(){Description="test",Price=2,Name="newtest",Id=Guid.NewGuid().ToString()};

    ItemResponse<Item> response = await container.CreateItemAsync(item2, partitionKey: new PartitionKey(item2.Id));

    
}
catch(CosmosException exception)
{
    Console.WriteLine($"Oops something went wrong with cosmos: {exception.Message} ");
}
catch(Exception exception)
{
    Console.WriteLine($"A more generic exception occured: {exception.Message}");
}
