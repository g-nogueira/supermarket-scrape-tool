namespace GNogueira.SupermarketScrapeTool.Common

[<AutoOpen>]
module Logging =

    open Microsoft.Extensions.Logging

    type LogMessage =
        | Debug of string
        | Information of string
        | Warning of string
        | Error of string
        | Exception of string * exn

    type ILogger =
        abstract Log: LogMessage -> unit

    type AzureFunctionLogger(logger: Microsoft.Extensions.Logging.ILogger) =
        interface ILogger with
            member this.Log msg =
                match msg with
                | Debug text -> logger.LogDebug(text)
                | Information text -> logger.LogInformation(text)
                | Warning text -> logger.LogWarning(text)
                | Error text -> logger.LogError(text)
                | Exception(text, exc) -> logger.LogError(exc, text)

    type ConsoleLogger() =
        interface ILogger with
            member this.Log msg =
                match msg with
                | Debug text -> printfn $"{text}"
                | Information text -> printfn $"{text}"
                | Warning text -> printfn $"{text}"
                | Error text -> printfn $"{text}"
                | Exception(text, exc) -> printfn $"{exc}"

    module CurrentLogger =
        let mutable logger: ILogger = ConsoleLogger()
