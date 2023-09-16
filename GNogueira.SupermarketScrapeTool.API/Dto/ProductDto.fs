namespace GNogueira.SupermarketScrapeTool.API.Dto

open System

type ProductDto = {
    Id: Guid
    Name: string
    Price: float
    PriceUnit: string
}