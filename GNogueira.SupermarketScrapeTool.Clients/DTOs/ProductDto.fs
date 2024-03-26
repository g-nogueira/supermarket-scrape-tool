namespace GNogueira.SupermarketScrapeTool.Clients

open System

type ProductSourceDto =
    { ExternalId: string
      Name: string
      Url: string
      ImageUrl: string option }

type PriceEntryDto =
    { Date: DateTime
      Price: float
      PriceUnit: string
      Source: ProductSourceDto }

type ProductDto =
    { id: Guid
      Name: string
      Brand: string
      PriceHistory: PriceEntryDto seq
      Sources: ProductSourceDto seq
      Ean: string option
      UpdatedOn: DateTime }
