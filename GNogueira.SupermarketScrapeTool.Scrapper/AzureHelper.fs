module GNogueira.SupermarketScrapeTool.Scrapper.AzureHelper

open FSharpPlus
open FsToolkit.ErrorHandling
open Microsoft.Azure.Cosmos
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Common.Secrets

let azureContainerName = "Products"

let createDatabase id (cosmosClient: CosmosClient) =
    cosmosClient.CreateDatabaseIfNotExistsAsync id |> AsyncResult.ofTask

let createContainer (id: string) (database: DatabaseResponse) =
    database.Database.CreateContainerIfNotExistsAsync(id, "/id")
    |> AsyncResult.ofTask

let addItemsToContainer<'T> item (container: ContainerResponse) =
    container.Container.CreateItemAsync<'T> item |> AsyncResult.ofTask

let createCosmosClient (cosmosDbConnectionString: string) =
    new CosmosClient(cosmosDbConnectionString)

let initAzureConnection logger =
    let secretManager = SecretManager logger

    let stopOnFail =
        function
        | Ok a -> a
        | Result.Error e -> failwith $"Stopped run due to fail to get Azure Cosmos DB connection string.\n{Error}"

    let connectionString =
        secretManager.GetCosmosDbConnectionString()
        |> Async.RunSynchronously
        |> stopOnFail

    let dbName = secretManager.GetCosmosDbName() |> Async.RunSynchronously |> stopOnFail

    connectionString
    |> createCosmosClient
    |> createDatabase dbName
    |> AsyncResult.bind (createContainer azureContainerName)