namespace GNogueira.SupermarketScrapeTool.Common

open System
open FSharp.Core
open FSharpPlus

module String =
    let isNullOrWhiteSpace (str: string) =
        match str |> String.trimWhiteSpaces with
        | null
        | "" -> true
        | _ -> false

    let (|EmptyString|_|) (str: string) =
        match str |> isNullOrWhiteSpace with
        | true -> Some()
        | false -> None
