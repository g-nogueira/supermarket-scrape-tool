namespace GNogueira.SupermarketScrapeTool.Common

open FSharp.Core
open System.Text.RegularExpressions

module Regex =
    let tryMatch (pattern: string) (input: string) =
        let regex = Regex(pattern)
        let match' = regex.Match(input)

        match match'.Success with
        | false -> None
        | true -> Some match'.Value
