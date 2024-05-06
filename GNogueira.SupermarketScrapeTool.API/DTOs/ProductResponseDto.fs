namespace GNogueira.SupermarketScrapeTool.API.DTOs

open System
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Common
open GNogueira.SupermarketScrapeTool.Common.FSharp

[<CLIMutable>]
type ProductSourceDto =
    {
        /// The id used by the Source to identify the product.
        ProductId: string
        Name: string
        ProductUrl: string
        /// The image used by the Source to identify the product.
        ProductImageUrl: string
    }

    static member toDomain(dto: ProductSourceDto) =
        result {
            let! sourceName = dto.Name |> SourceName.ofString |> Result.mapError exn

            return
                { Product.ProductSource.ProductId = dto.ProductId
                  Name = sourceName
                  ProductUrl = dto.ProductUrl
                  ProductImageUrl = dto.ProductImageUrl |> Option.ofString }
        }

    static member ofDomain(source: ProductSource) =
        { ProductId = source.ProductId
          Name = source.Name |> deconstruct
          ProductUrl = source.ProductUrl
          ProductImageUrl = source.ProductImageUrl |> Option.defaultValue ""}

[<CLIMutable>]
type PriceEntryDto =
    { Date: DateTime
      Price: float
      PriceUnit: string
      Source: ProductSourceDto }

    static member toDomain(dto: PriceEntryDto) =
        result {
            let! source = dto.Source |> ProductSourceDto.toDomain
            let! priceUnit = dto.PriceUnit |> PriceUnit.ofString |> Result.mapError exn

            return
                { PriceEntry.Date = dto.Date
                  Price = dto.Price
                  PriceUnit = priceUnit
                  Source = source }
        }

    static member ofDomain(entry: PriceEntry) =
        { Date = entry.Date
          Price = entry.Price
          PriceUnit = entry.PriceUnit |> deconstruct
          Source = entry.Source |> ProductSourceDto.ofDomain }


[<CLIMutable>]
type ProductResponseDto =
    { id: string
      status: string
      Name: string
      Brand: string
      PriceHistory: PriceEntryDto seq
      Sources: ProductSourceDto seq
      Ean: string }

    static member toDomain(dto: ProductResponseDto) =
        result {
            let! priceHistory =
                dto.PriceHistory
                |> Seq.map PriceEntryDto.toDomain
                |> List.ofSeq
                |> List.sequenceResultM

            let! sources =
                dto.Sources
                |> Seq.map ProductSourceDto.toDomain
                |> List.ofSeq
                |> List.sequenceResultM

            return
                { Product.Id = dto.id |> ProductId
                  Name = dto.Name
                  Brand = dto.Brand
                  PriceHistory = priceHistory
                  Sources = sources
                  Ean = dto.Ean }
        }

    static member ofDomain(product: Product) =
        { ProductResponseDto.id = product.Id |> deconstruct
          status = "ok"
          Name = product.Name
          Brand = product.Brand
          PriceHistory = product.PriceHistory |> Seq.map PriceEntryDto.ofDomain
          Sources = product.Sources |> Seq.map ProductSourceDto.ofDomain
          Ean = product.Ean }