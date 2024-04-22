namespace GNogueira.SupermarketScrapeTool.Clients

open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Common.Logging
open Microsoft.Azure.Cosmos

[<RequireQualifiedAccess>]
module CosmosDbInitializer =
    let createDatabase id (cosmosClient: CosmosClient) =
        cosmosClient.CreateDatabaseIfNotExistsAsync id |> AsyncResult.ofTask

    let createContainer (id: string) (database: DatabaseResponse) =
        database.Database.CreateContainerIfNotExistsAsync(id, "/id")
        |> AsyncResult.ofTask

    let createCosmosClient (cosmosDbConnectionString: string) =
        new CosmosClient(cosmosDbConnectionString)

    let initAzureConnection (secretManager: ISecretClient) (logger: ILogger) =

        let stopOnFail =
            function
            | Ok a -> a
            | Result.Error e -> failwith $"Stopped run due to fail to get Azure Cosmos DB connection string.\n{Error}"

        let connectionString =
            secretManager.GetCosmosDbConnectionString()
            |> Async.RunSynchronously
            |> stopOnFail

        connectionString |> createCosmosClient

type ICosmosDbClient =
    abstract PricesContainer: Container
    abstract ProductsContainer: Container

type CosmosDbClient(secretManager: ISecretClient, logger: ILogger) =

    let dbName =
        secretManager.GetCosmosDbName()
        |> AsyncResult.teeError (fun (e: exn) ->
            logger.Log(Error $"Error getting DB name from Secret Manager: {e.Message}"))
        |> AsyncResult.defaultValue ""
        |> Async.RunSynchronously

    let client = CosmosDbInitializer.initAzureConnection secretManager logger

    interface ICosmosDbClient with
        member this.PricesContainer = client.GetContainer(dbName, "Items")
        member this.ProductsContainer = client.GetContainer(dbName, "Products")
