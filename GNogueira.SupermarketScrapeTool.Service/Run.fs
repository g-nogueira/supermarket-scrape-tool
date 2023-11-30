namespace GNogueira.SupermarketScrapeTool.Service

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Service.Models
open GNogueira.SupermarketScrapeTool.Service.Start
open Microsoft.Azure.WebJobs
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core
open CurrentLogger

module AzureFunction =
    [<FunctionName("SupermarketScrapeTool")>]
    let run
        ([<TimerTrigger("0 0 7 * * *")>] timerInfo: TimerInfo)
        ([<CosmosDB("SupermarketItems", "Items", Connection = "CosmosDbConnectionString")>] products:
            IAsyncCollector<ProductDto>)
        (log: ILogger)
        =
        logger <- AzureFunctionLogger log :> GNogueira.SupermarketScrapeTool.Service.ILogger

        logger.Information("F# HTTP trigger function processed a request.")

        scrapeProducts ()
        |> Async.RunSynchronously
        |> Result.teeError (logger.Exception "")
        |> Result.defaultValue []
        |> Seq.map (products.AddAsync >> Async.AwaitTask)
        |> (Async.Parallel >> Async.RunSynchronously)
        |> ignore