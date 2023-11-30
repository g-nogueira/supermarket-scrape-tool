module GNogueira.SupermarketScrapeTool.Service.CosmosDBHelper

open Microsoft.Azure.Cosmos
open FsToolkit.ErrorHandling

let createDatabase id (cosmosClient: CosmosClient) =
    cosmosClient.CreateDatabaseIfNotExistsAsync id |> AsyncResult.ofTask

let createContainer (id: string) (partitionKey: string) (database: DatabaseResponse) =
    database.Database.CreateContainerIfNotExistsAsync(id, partitionKey)
    |> AsyncResult.ofTask

let addItemsToContainer<'T> item (container: ContainerResponse) =
    container.Container.CreateItemAsync<'T> item |> AsyncResult.ofTask

let createCosmosClient (cosmosDbConnectionString: string) =
    new CosmosClient(cosmosDbConnectionString)

let initAzureConnection logger azureContainerName partitionKey =
    let secretManager: SecretManager = SecretManager logger

    let stopOnFail =
        function
        | Ok a -> a
        | Error e -> failwith $"Stopped run due to fail to get Azure Cosmos DB connection string.\n{Error}"

    let connectionString =
        secretManager.GetCosmosDbConnectionString()
        |> Async.RunSynchronously
        |> stopOnFail

    let dbName = secretManager.GetCosmosDbName() |> Async.RunSynchronously |> stopOnFail

    connectionString
    |> createCosmosClient
    |> createDatabase dbName
    |> AsyncResult.bind (createContainer azureContainerName partitionKey)    