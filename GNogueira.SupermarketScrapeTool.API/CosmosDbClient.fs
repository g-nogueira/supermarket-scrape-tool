namespace GNogueira.SupermarketScrapeTool.API

open FsToolkit.ErrorHandling
open Microsoft.Azure.Cosmos
open Microsoft.Extensions.Logging

[<RequireQualifiedAccess>]
module CosmosDbInitializer =
    let createDatabase id (cosmosClient: CosmosClient) =
        cosmosClient.CreateDatabaseIfNotExistsAsync id |> AsyncResult.ofTask

    let createContainer (id: string) (database: DatabaseResponse) =
        database.Database.CreateContainerIfNotExistsAsync(id, "/id")
        |> AsyncResult.ofTask

    let createCosmosClient (cosmosDbConnectionString: string) =
        new CosmosClient(cosmosDbConnectionString)

    let initAzureConnection (secretManager: ISecretManager) (logger: ILogger) =

        let stopOnFail =
            function
            | Ok a -> a
            | Error e -> failwith $"Stopped run due to fail to get Azure Cosmos DB connection string.\n{Error}"

        let connectionString =
            secretManager.GetCosmosDbConnectionString()
            |> Async.RunSynchronously
            |> stopOnFail

        let dbName = secretManager.GetCosmosDbName() |> Async.RunSynchronously |> stopOnFail

        connectionString |> createCosmosClient

type ICosmosDbClient =
    abstract GetContainer: string -> Container
    abstract DefaultContainer: Container

type CosmosDbClient(secretManager: ISecretManager, logger: ILogger<CosmosDbClient>) =

    let dbName =
        secretManager.GetCosmosDbName()
        |> AsyncResult.teeError (fun (e: exn) ->
            logger.LogError $"Error getting DB name from Secret Manager: {e.Message}")
        |> AsyncResult.defaultValue ""
        |> Async.RunSynchronously

    let client = CosmosDbInitializer.initAzureConnection secretManager logger

    interface ICosmosDbClient with
        member this.DefaultContainer = client.GetContainer(dbName, "Items")

        member this.GetContainer containerName =
            client.GetContainer(dbName, containerName)
