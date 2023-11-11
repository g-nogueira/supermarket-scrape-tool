namespace GNogueira.SupermarketScrapeTool.Clients

open System

type ProductDto =
    { id: Guid
      Name: string
      Source: string
      Url: string
      ImageUrl: string
      updatedOn: DateTime }