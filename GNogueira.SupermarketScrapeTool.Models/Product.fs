module GNogueira.SupermarketScrapeTool.Models

open System

type ProductSource =
    | PingoDoce
    | Continente


type PriceUnit =
    | Kg
    | Un
    | Liter
    | Rolls
    | Unknown

type ProductPriceId =
    | ProductPriceId of Guid

    static member deconstruct value =
        let (ProductPriceId id) = value
        id

type ProductPrice =
    { Date: DateTime
      Price: float
      PriceUnit: PriceUnit }

type ProductId =
    | ProductId of Guid

    static member deconstruct value =
        let (ProductId id) = value
        id

type Product =
    { Id: ProductId
      ExternalId: string
      Date: DateTime
      Name: string
      Prices: seq<ProductPrice>
      Source: ProductSource
      Url: Option<string>
      ImageUrl: Option<string> }
