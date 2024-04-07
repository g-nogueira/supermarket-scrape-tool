namespace GNogueira.SupermarketScrapeTool.Scrapper.Models

open System
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Common.FSharp

type ProductSourceDto =
    {
        /// The id used by the Source to identify the product.
        ProductId: string
        Name: string
        ProductUrl: string
        /// The image used by the Source to identify the product.
        ProductImageUrl: string option
    }

    static member ofDomain(source: ProductSource) =
        { ProductId = source.ProductId
          Name = source.Name |> deconstruct
          ProductUrl = source.ProductUrl
          ProductImageUrl = source.ProductImageUrl }

type PriceEntryDto =
    { Date: DateTime
      Price: float
      PriceUnit: string
      Source: ProductSourceDto }

    static member ofDomain(entry: PriceEntry) =
        { Date = entry.Date
          Price = entry.Price
          PriceUnit = entry.PriceUnit |> deconstruct
          Source = entry.Source |> ProductSourceDto.ofDomain }

type ProductDto =
    { id: string
      Name: string
      Brand: string
      PriceHistory: PriceEntryDto seq
      Sources: ProductSourceDto seq
      Ean: string }

    static member ofDomain(product: Product) =
        { id = product.Id |> deconstruct
          Name = product.Name
          Brand = product.Brand
          PriceHistory = product.PriceHistory |> Seq.map PriceEntryDto.ofDomain
          Sources = product.Sources |> Seq.map ProductSourceDto.ofDomain
          Ean = product.Ean }
