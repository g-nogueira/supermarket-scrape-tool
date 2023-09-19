namespace GNogueira.SupermarketScrapeTool.API.Clients

open System

type ProductDto =
    { Id: Guid
      Name: string
      Price: float
      PriceUnit: string }
