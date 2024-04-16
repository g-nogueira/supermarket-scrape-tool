namespace GNogueira.SupermarketScrapeTool.Common

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
        | true -> None
        | s -> Some s
