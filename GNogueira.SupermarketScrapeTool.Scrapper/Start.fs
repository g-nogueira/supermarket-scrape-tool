namespace GNogueira.SupermarketScrapeTool.Scrapper

open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Common.Secrets
open GNogueira.SupermarketScrapeTool.Models
open Microsoft.Azure.Cosmos
open FSharpPlus
open FsToolkit.ErrorHandling
open System
open GNogueira.SupermarketScrapeTool.Common.Extensions.FSharp
open GNogueira.SupermarketScrapeTool.Scrapper.CompositionRoot
open Microsoft.VisualBasic
open ProductPriceClientEx
open GNogueira.SupermarketScrapeTool.Scrapper.AzureHelper

[<AutoOpen>]
module Start =
    let saveSourceItems container (source: ProductSource, products: seq<ProductDto>) =
        logger.Log(LogMessage.Information $"Saving items from Source '{source |> ProductSource.deconstruct}'.")

        products
        |> Seq.map (saveItem container)
        |> Async.Parallel
        |> Async.map (fun x -> x |> List.ofArray |> List.sequenceResultM)

    let start () =
        asyncResult {
            let! container = initAzureConnection logger

            // Scrape products from sources
            let! sourcedProducts = 
                ProductPriceClient.FetchProducts productPriceClient
            
            // Save products to Cosmos DB
            let! savedProducts =
                sourcedProducts
                |> Seq.map (saveSourceItems container)
                |> Async.Parallel
                |> Async.map (List.ofArray >> List.sequenceResultM)

            return savedProducts
        }
        |> AsyncResult.teeError (fun e -> logger.Log(LogMessage.Exception("Error running scrapper.", e)))