namespace GNogueira.SupermarketScrapeTool.FSharp

open System

module String =
    let inline contains (str: string) value =
        match box value with
        | :? string as s -> str.Contains(s, StringComparison.InvariantCulture)
        | :? char as c -> str.Contains(c, StringComparison.InvariantCulture)
        | _ -> failwith "Invalid argument type for 'value'"
