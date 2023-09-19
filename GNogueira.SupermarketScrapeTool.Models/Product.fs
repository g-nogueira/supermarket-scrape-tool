namespace GNogueira.SupermarketScrapeTool.Models

open System

type ProductId =
    | ProductId of Guid

    static member deconstruct value =
        let (ProductId id) = value
        id

type PriceUnit =
    | PriceUnit of string

    static member deconstruct value =
        let (PriceUnit pu) = value
        pu

type ProductSource =
    | ProductSource of string

    static member deconstruct value =
        let (ProductSource ps) = value
        ps

type Product =
    { Id: ProductId
      Name: string
      Price: float
      PriceUnit: PriceUnit
      Source: ProductSource
      Url: string
      ImageUrl: string
      Date: DateTime }