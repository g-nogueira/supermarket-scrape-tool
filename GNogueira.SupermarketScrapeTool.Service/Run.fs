namespace GNogueira.SupermarketScrapeTool.Service

open GNogueira.SupermarketScrapeTool.Service.Start
open Microsoft.Azure.WebJobs
open Microsoft.Extensions.Logging

module AzureFunction =
    [<FunctionName("SupermarketScrapeTool")>]
    let run ([<TimerTrigger("0 0 7 * * *")>] timerInfo: TimerInfo) (log: ILogger) =
        log.LogInformation("F# HTTP trigger function processed a request.")
        
        
        start (AzureFunctionLogger log)
