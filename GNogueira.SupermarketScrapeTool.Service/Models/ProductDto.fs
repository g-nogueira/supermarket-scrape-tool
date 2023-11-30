namespace GNogueira.SupermarketScrapeTool.Service.Models

open System

type ProductPriceDto =
    { Date: DateTime
      Price: float
      PriceUnit: string }

type ProductDto =
    { id: Guid
      ExternalId: string
      Date: DateTime
      Name: string
      Prices: seq<ProductPriceDto>
      Source: string
      Url: string
      ImageUrl: string }
