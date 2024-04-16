namespace GNogueira.SupermarketScrapeTool.Clients

open System
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Models

[<CLIMutable>]
type ProductSourceDto =
    {
        /// The id used by the Source to identify the product.
        ProductId: string
        Name: string
        ProductUrl: string
        /// The image used by the Source to identify the product.
        ProductImageUrl: string option
    }

    static member toDomain(dto: ProductSourceDto) =
        result {
            let! sourceName = dto.Name |> SourceName.ofString |> Result.mapError exn

            return
                { ProductSource.ProductId = dto.ProductId
                  Name = sourceName
                  ProductUrl = dto.ProductUrl
                  ProductImageUrl = dto.ProductImageUrl }
        }

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

[<CLIMutable>]
type ProductDto =
    { id: string
      status: string
      Name: string
      Brand: string
      PriceHistory: PriceEntryDto seq
      Sources: ProductSourceDto seq
      Ean: string }

    static member toDomain(dto: ProductDto) =
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
