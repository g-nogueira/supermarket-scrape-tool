namespace GNogueira.SupermarketScrapeTool.Scrapper.Models

open GNogueira.SupermarketScrapeTool.Models.Product

type ScrappedProduct =
    { Id: ProductId
      Name: string
      Brand: string
      CurrentPrice: PriceEntry
      Source: ProductSource
      Ean: string }
