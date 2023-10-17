namespace GNogueira.SupermarketScrapeTool.Clients

open System

type ProductDto =
    { Id: Guid
      Date: DateTime
      Name: string
      Price: float
      PriceUnit: string
      Source: string
      Url: string
      ImageUrl: string }