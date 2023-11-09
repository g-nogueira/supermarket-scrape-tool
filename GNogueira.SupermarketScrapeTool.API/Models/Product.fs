namespace GNogueira.SupermarketScrapeTool.API.Models

open System
open GNogueira.SupermarketScrapeTool.API.DTOs
open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Models

module ProductExtensions =

    type ProductPrice with    
        static member ofDto(dto: ProductPriceDto) =
            { ProductPrice.Id = dto.Id |> ProductPriceId
              Name = dto.Name
              Price = dto.Price
              PriceUnit = dto.PriceUnit |> PriceUnit
              Source = dto.Source |> ProductSource
              Url = dto.Url
              ImageUrl = dto.ImageUrl
              Date = dto.Date }

        static member toDto(domain: ProductPrice) =
            { ProductResponseDto.Id = (domain.Id |> ProductPriceId.deconstruct)
              Date = domain.Date |> string
              Name = domain.Name
              Price = domain.Price
              PriceUnit = domain.PriceUnit |> PriceUnit.deconstruct
              Source = domain.Source |> ProductSource.deconstruct
              Url = domain.Url
              ImageUrl = domain.ImageUrl }
