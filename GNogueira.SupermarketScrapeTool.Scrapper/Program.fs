module GNogueira.SupermarketScrapeTool.Scrapper.Main

open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Scrapper.CompositionRoot

module Scrapper =
    [<EntryPoint>]
    let main argv =

        setLogger (ConsoleLogger() :> ILogger)
        
        start ()
        |> Async.RunSynchronously
        |> ignore

        0