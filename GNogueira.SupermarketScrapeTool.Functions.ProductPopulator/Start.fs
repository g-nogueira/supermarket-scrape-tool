namespace GNogueira.SupermarketScrapeTool.Functions.ProductPopulator

open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Common.Secrets
open GNogueira.SupermarketScrapeTool.Models
open Microsoft.Azure.Cosmos
open FSharpPlus
open FsToolkit.ErrorHandling
open System
open GNogueira.SupermarketScrapeTool.Common.Extensions.FSharp
open GNogueira.SupermarketScrapeTool.Functions.ProductPopulator.CompositionRoot
open Microsoft.VisualBasic
open ProductPriceClientEx

[<AutoOpen>]
module Start =
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

    let saveItem container item =
        container
        |> addItemsToContainer<ProductDto> item
        |> AsyncResult.teeError (fun e -> printfn $"{e.Message}")

    let start () =
        asyncResult {
            let! container = initAzureConnection logger

            let saveSourceItems container (source: ProductSource, products: seq<ProductDto>) =
                logger.Log(LogMessage.Information $"Saving items from Source '{source |> ProductSource.deconstruct}'.")

                products
                |> Seq.map (saveItem container)
                |> Async.Parallel
                |> Async.map (fun x -> x |> List.ofArray |> List.sequenceResultM)

            let! sourcedProducts = ProductPriceClient.GenerateProducts productPriceClient

            let! savedProducts =
                sourcedProducts
                |> Seq.map (saveSourceItems container)
                |> Async.Parallel
                |> Async.map (List.ofArray >> List.sequenceResultM)

            return savedProducts
        }
        |> AsyncResult.teeError (fun e -> logger.Log(LogMessage.Exception("Error running scrapper.", e)))
