namespace GNogueira.SupermarketScrapeTool.Service

open System
open Microsoft.Extensions.Logging

type ILogger =
    abstract Information: string -> unit
    abstract Error: string -> unit
    abstract Exception: string -> exn -> unit

type AzureFunctionLogger (logger : Microsoft.Extensions.Logging.ILogger) =
    interface ILogger with
        member this.Information msg = logger.LogInformation msg
        member this.Error msg = logger.LogError msg
        member this.Exception msg (exc : exn)  = logger.LogError(exc, msg)

type ConsoleLogger () =
    interface ILogger with
        member this.Information text = printfn $"{text}"
        member this.Error msg = printfn $"{msg}"
        member this.Exception  msg (exc : exn)  = printfn $"{exc}"