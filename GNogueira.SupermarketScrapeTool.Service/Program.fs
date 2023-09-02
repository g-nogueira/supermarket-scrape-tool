module GNogueira.SupermarketScrapeTool.Service.Main

[<EntryPoint>]
let main argv =
    start (ConsoleLogger())
    |> ignore

    0