namespace GNogueira.SupermarketScrapeTool.Common

open FSharp.Core
open FSharpPlus
module Result =
    let ofOption error =
        function
        | Some v -> Ok v
        | None -> Error error
    
    type ResultBuilder() =
        member _.Bind(x, f) = x |> Result.bind f
        member _.Return x = Ok x
        member _.ReturnFrom x = x
        member _.Zero() = Ok ()
        member _.Combine(a, b) =
            match a, b with
            | Ok _, Ok _ -> Ok ()
            | Error e, Ok _ -> Error e
            | Ok _, Error e -> Error e
            | Error e1, Error e2 -> Error (e1 + e2)
    
    let result = ResultBuilder()

module String =
    let isNullOrWhiteSpace (value: string) =
        match value |> String.trimWhiteSpaces with
        | null
        | "" -> true
        | _ -> false

module Option =
    let ofString (value: string) =
        match String.isNullOrWhiteSpace value with
        | true -> None
        | false -> Some value

module Regex =
    open System.Text.RegularExpressions
    let tryMatch (pattern: string) (input: string) =
        let regex = Regex(pattern)
        let match' = regex.Match(input)

        match match'.Success with
        | false -> None
        | true -> Some match'.Value

module Async =
    let inline tee f x =
        async {
            let! result = x
            f result
            return result
        }