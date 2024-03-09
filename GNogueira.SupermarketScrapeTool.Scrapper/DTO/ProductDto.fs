namespace GNogueira.SupermarketScrapeTool.Scrapper.DTO

open System

type ProductSourceDto =
    { SourceId: Guid
      Name: string
      Url: string }

type PriceEntryDto =
    { PrinceEntryId: Guid
      Date: DateTime
      Price: decimal
      Source: ProductSourceDto }

type ProductExternalIdDto = { ExternalId: string; Source: string }

type ProductDto =
    { ProductId: Guid
      ExternalIds: ProductExternalIdDto seq
      Name: string
      Brand: string
      PriceHistory: PriceEntryDto seq }
