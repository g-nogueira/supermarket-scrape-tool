namespace GNogueira.SupermarketScrapeTool.API.DTOs

open System

type ProductResponseDto =
    { Id: Guid
      Date: string
      Name: string
      Price: float
      PriceUnit: string
      Source: string
      Url: string
      ImageUrl: string }