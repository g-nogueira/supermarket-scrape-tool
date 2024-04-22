namespace GNogueira.SupermarketScrapeTool.Common

open System
open FSharp.Core

module String =
    let (|EmptyString|_|) (str: string) =
        match str |> String.IsNullOrWhiteSpace with
        | true -> Some()
        | false -> None
