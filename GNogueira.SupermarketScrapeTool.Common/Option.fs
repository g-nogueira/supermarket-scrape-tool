namespace GNogueira.SupermarketScrapeTool.Common

open FSharp.Core
open FSharpPlus

module Option =
    let ofString (value: string) =
        match String.isNullOrWhiteSpace value with
        | true -> None
        | false -> Some value
