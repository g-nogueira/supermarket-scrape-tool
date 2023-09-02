namespace GNogueira.SupermarketScrapeTool.Service

open GNogueira.SupermarketScrapeTool.Service.DTOs
open GNogueira.SupermarketScrapeTool.Service.Models
open Microsoft.Azure.Cosmos
open Models
open FSharpPlus
open FsToolkit.ErrorHandling
open System

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

    let saveToFile (filename: string) =
        let saveItem (item) =
            use writer = System.IO.File.AppendText(filename)
            let json = Newtonsoft.Json.JsonConvert.SerializeObject(item)
            writer.WriteLine(json)

        saveItem

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
        |> addItemsToContainer<ProductDto> item
        |> AsyncResult.teeError (fun e -> printfn $"{e.Message}")

    let start (logger: ILogger) =
        asyncResult {
            let scrape website =
                logger.Information $"Scraping {website |> ProductSource.toString}:"

                scrapeWebsite website

            let saveItem container item =
                logger.Information $"Saving item {item.Name} from {item.Source |> ProductSource.toString}"

                item |> Product.toDto |> saveItem container

            let! container = initAzureConnection logger

            let! products =
                websites
                |> Seq.map scrape
                |> Async.Parallel
                |> Async.map (Seq.collect id)
                |> Async.map Ok

            let savedProducts =
                products
                |> Seq.map (saveItem container)
                |> Async.Parallel
                |> Async.map (Array.map (Result.teeError (logger.Exception String.Empty)))

            savedProducts |> Async.RunSynchronously |> ignore
        }
        |> AsyncResult.teeError (logger.Exception "Error running scrapper.")
