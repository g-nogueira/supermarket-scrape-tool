namespace GNogueira.SupermarketScrapeTool.Models

module Exceptions =
    type ProductNotFoundException(message: string) =
        inherit System.Exception(message)

    let isProductNotFoundException (ex: System.Exception) = ex :? ProductNotFoundException

    let (|ProductNotFound|_|) (ex: exn) =
        match ex with
        | :? ProductNotFoundException as ex -> Some ex
        | _ -> None
