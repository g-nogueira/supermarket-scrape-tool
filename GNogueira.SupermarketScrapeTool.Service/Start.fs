namespace GNogueira.SupermarketScrapeTool.Service

open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Service.Models
open Microsoft.Azure.Cosmos
open Models
open FSharpPlus
open FsToolkit.ErrorHandling
open System
open CurrentLogger
open CosmosDBHelper

[<AutoOpen>]
module Start =
    [<Literal>]
    let azureContainerName = "Products"

    [<Literal>]
    let azureContainerPartitionKey = "/productId"

    let scrapeWebsite =
        function
        | Continente -> ContinenteScrapper.scrape ()
        | PingoDoce -> PingoDoceScrapper.scrape ()

    let websites = [ Continente; PingoDoce ]

    let saveItem container item =
        container
        |> addItemsToContainer<ProductDto> item
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

            return products
        }
        |> AsyncResult.teeError (logger.Exception "Error running scrapper.")

    let start () =
        asyncResult {

            let saveItems container (item: ProductDto) =
                logger.Information $"Saving item {item.Name} from {item.Source}"

                item |> saveItem container

            let! container = initAzureConnection logger azureContainerName azureContainerPartitionKey

            let logSuccess (products: seq<ProductDto>) =
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
