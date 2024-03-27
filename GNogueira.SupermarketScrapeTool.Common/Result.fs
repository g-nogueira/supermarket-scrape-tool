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
        member _.Zero() = Ok()

        member _.Combine(a, b) =
            match a, b with
            | Ok _, Ok _ -> Ok()
            | Error e, Ok _ -> Error e
            | Ok _, Error e -> Error e
            | Error e1, Error e2 -> Error(e1 + e2)

    let result = ResultBuilder()
