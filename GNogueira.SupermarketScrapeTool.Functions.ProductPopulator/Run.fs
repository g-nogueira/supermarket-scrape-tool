namespace GNogueira.SupermarketScrapeTool.Functions.ProductPopulator

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Functions.ProductPopulator.CompositionRoot
open Microsoft.Azure.WebJobs
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Core

module AzureFunction =

    [<FunctionName("SupermarketScrapeTool")>]
    let run
        ([<TimerTrigger("0 0 7 * * *")>] timerInfo: TimerInfo)
        ([<CosmosDB("SupermarketItems", "Items", Connection = "CosmosDbConnectionString")>] products:
            IAsyncCollector<ProductDto>)
        (log: ILogger)
        =

        setLogger (AzureFunctionLogger log)
        logger.Log (Information "F# HTTP trigger function processed a request.")

        start ()
        |> Async.RunSynchronously
        |> Result.teeError (fun e -> logger.Log (Exception ("", e)))
        |> Result.defaultValue []
        |> ignore