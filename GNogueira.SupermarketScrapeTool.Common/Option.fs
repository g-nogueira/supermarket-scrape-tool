namespace GNogueira.SupermarketScrapeTool.Common

open System
open FSharp.Core
open FSharpPlus

module Option =
    let ofString (value: string) =
        match String.IsNullOrWhiteSpace value with
        | true -> None
        | false -> Some value
