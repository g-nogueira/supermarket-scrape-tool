module GNogueira.SupermarketScrapeTool.Service.Main

open CurrentLogger

[<EntryPoint>]
let main argv =

    logger <- ConsoleLogger ()

    start ()
    |> ignore

    0