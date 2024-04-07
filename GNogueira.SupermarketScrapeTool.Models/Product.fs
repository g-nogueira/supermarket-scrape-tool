namespace GNogueira.SupermarketScrapeTool.Models

open System
open FSharpPlus

[<AutoOpen>]
module Product =
    type SourceId =
        | SourceId of Guid

        member this.Deconstruct() =
            let (SourceId id) = this

            id

    type PriceUnit =
        | Kg
        | Un
        | Liter
        | Rolls
        | Unknown

    type SourceName =
        | PingoDoce
        | Continente

    /// The source most of the time will be a supermarket, but it could be a different type of store
    /// For each specific supermarket, a product will have its own Id, Url, and Image
    type ProductSource =
        { /// The id used by the Source to identify the product.
          ProductId: string
          Name: SourceName
          ProductUrl: string
          /// The image used by the Source to identify the product.
          ProductImageUrl: string option }

    type PriceEntry =
        { Date: DateTime
          Price: float
          PriceUnit: PriceUnit
          Source: ProductSource }

    type ProductId =
        | ProductId of Guid

        member this.Deconstruct() =
            let (ProductId id) = this

            id

    type ProductExternalId = { ExternalId: string; Source: string }

    type Product =
        { Id: ProductId
          Name: string
          Brand: string
          PriceHistory: PriceEntry seq
          Sources: ProductSource seq
          Ean: string option }

    type SourceName with

        static member toString =
            function
            | PingoDoce -> "pingodoce"
            | Continente -> "continente"

        static member ofString =
            String.toLower
            >> function
                | "pingodoce" -> PingoDoce |> Ok
                | "continente" -> Continente |> Ok
                | value -> $"Product Source not valid. Tried to parse {value}." |> Error

    type PriceUnit with

        static member toString =
            function
            | Kg -> "kg"
            | Un -> "un"
            | Liter -> "liter"
            | Rolls -> "rolls"
            | Unknown -> "unknown"

        static member ofString(value: string) =
            match value |> String.toLower with
            | "kg"
            | "kgm" -> Kg |> Ok
            | "un" -> Un |> Ok
            | "ltr" -> Liter |> Ok
            | "ro" -> Rolls |> Ok
            | _ -> $"Price Unit not valid. Tried to parse {value}." |> Error
