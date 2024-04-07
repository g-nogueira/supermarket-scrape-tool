namespace GNogueira.SupermarketScrapeTool.Scrapper.Models

open GNogueira.SupermarketScrapeTool.Common.Optics
open GNogueira.SupermarketScrapeTool.Models

[<AutoOpen>]
module Product =
    let Name =
        { Get = fun product -> product.Name
          Set = fun value product -> { product with Name = value } }

    let Brand =
        { Get = fun product -> product.Brand
          Set = fun value product -> { product with Brand = value } }

    let PriceHistory =
        { Get = fun product -> product.PriceHistory
          Set = fun value product -> { product with PriceHistory = value } }

    let Sources =
        { Get = fun product -> product.Sources
          Set = fun value product -> { product with Sources = value } }

    let Ean =
        { Get = fun product -> product.Ean
          Set = fun value product -> { product with Ean = value } }
