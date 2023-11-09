module GNogueira.SupermarketScrapeTool.Functions.ProductPopulator.Main

open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Functions.ProductPopulator.CompositionRoot

module ProductPopulator =
    [<EntryPoint>]
    let main argv =

        setLogger (ConsoleLogger() :> ILogger)
        
        start ()
        |> Async.RunSynchronously
        |> ignore

        0