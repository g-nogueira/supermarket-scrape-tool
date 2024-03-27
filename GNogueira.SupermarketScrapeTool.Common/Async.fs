namespace GNogueira.SupermarketScrapeTool.Common

open FSharp.Core

module Async =
    let inline tee f x =
        async {
            let! result = x
            f result
            return result
        }
