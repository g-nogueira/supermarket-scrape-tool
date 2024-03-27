namespace GNogueira.SupermarketScrapeTool.Common

open FSharp.Core
open FSharpPlus

module String =
    let isNullOrWhiteSpace (value: string) =
        match value |> String.trimWhiteSpaces with
        | null
        | "" -> true
        | _ -> false
