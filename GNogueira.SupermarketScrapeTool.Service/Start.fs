﻿namespace GNogueira.SupermarketScrapeTool.Service

open GNogueira.SupermarketScrapeTool.Service.DTOs
open GNogueira.SupermarketScrapeTool.Service.Models
open Microsoft.Azure.Cosmos
open Models
open FSharpPlus
open FsToolkit.ErrorHandling
open System
open CurrentLogger

[<AutoOpen>]
module Start =
    let azureContainerName = "Items"

    let createDatabase id (cosmosClient: CosmosClient) =
        cosmosClient.CreateDatabaseIfNotExistsAsync id |> AsyncResult.ofTask

    let createContainer (id: string) (database: DatabaseResponse) =
        database.Database.CreateContainerIfNotExistsAsync(id, "/id")
        |> AsyncResult.ofTask

    let addItemsToContainer<'T> item (container: ContainerResponse) =
        container.Container.CreateItemAsync<'T> item |> AsyncResult.ofTask

    let createCosmosClient (cosmosDbConnectionString: string) =
        new CosmosClient(cosmosDbConnectionString)

    let scrapeWebsite =
        function
        | Continente -> ContinenteScrapper.scrape ()
        | PingoDoce -> PingoDoceScrapper.scrape ()

    let websites = [ Continente; PingoDoce ]

    let initAzureConnection logger =
        let secretManager = SecretManager logger

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
        |> AsyncResult.bind (createContainer azureContainerName)

    let saveItem container item =
        container
        |> addItemsToContainer<ProductPriceDto> item
        |> AsyncResult.teeError (fun e -> printfn $"{e.Message}")

    let scrapeProducts () =
        asyncResult {
            let scrape website =

                scrapeWebsite website

            let! products =
                websites
                |> Seq.map scrape
                |> Async.Parallel
                |> Async.map (Seq.collect id)
                |> Async.map Ok

            return products |> Seq.map ProductPrice.toDto
        }
        |> AsyncResult.teeError (logger.Exception "Error running scrapper.")

    let start () =
        asyncResult {

            let saveItems container (item: ProductPriceDto) =
                logger.Information $"Saving item {item.Name} from {item.Source}"

                item |> saveItem container

            let! container = initAzureConnection logger

            let logSuccess (products: seq<ProductPriceDto>) =
                products
                |> Seq.countBy (fun p -> p.Source)
                |> Seq.iter (fun (source, count) ->
                    logger.Information $"Successfully scraped websites. Got {count} products from {source}.")

            let savedProducts =
                scrapeProducts ()
                |> AsyncResult.tee logSuccess
                |> AsyncResult.map (Seq.map (saveItems container))
                |> AsyncResult.map Async.Parallel
                |> AsyncResult.map (Async.map (Array.map (Result.teeError (logger.Exception String.Empty))))

            savedProducts |> Async.RunSynchronously |> ignore
        }
        |> AsyncResult.teeError (logger.Exception "Error running scrapper.")
