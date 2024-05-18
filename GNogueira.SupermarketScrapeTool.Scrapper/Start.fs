namespace GNogueira.SupermarketScrapeTool.Scrapper

open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Models.Exceptions
open GNogueira.SupermarketScrapeTool.Scrapper.CompositionRoot
open GNogueira.SupermarketScrapeTool.Scrapper.Models

[<AutoOpen>]
module Start =

    let enrichWithDBProduct (scrappedProduct: ScrappedProduct) =
        let storageProduct = productClient.GetById scrappedProduct.Id

        let enrichSources (scrappedProduct: ScrappedProduct) (product: Product) =
            let isSameSource (source: ProductSource) (source': ProductSource) = source.Name = source'.Name

            match product.Sources |> Seq.toList with
            | [] -> scrappedProduct.Source |> Seq.singleton
            | _ ->
                product.Sources
                |> Seq.map (fun source ->
                    match scrappedProduct.Source |> isSameSource source with
                    | true -> scrappedProduct.Source
                    | false -> source)

        let enrichPriceHistory (scrappedProduct: ScrappedProduct) (product: Product) =
            let isSameDayAs (price: PriceEntry) (price': PriceEntry) = price.Date.Date = price'.Date.Date
            let includesPrice (price: PriceEntry) = List.exists (isSameDayAs price)

            match product.PriceHistory |> Seq.toList with
            | [] -> scrappedProduct.CurrentPrice |> Seq.singleton
            | priceHistory when priceHistory |> includesPrice scrappedProduct.CurrentPrice ->
                product.PriceHistory
                |> Seq.map (fun dbPrice ->
                    match scrappedProduct.CurrentPrice |> isSameDayAs dbPrice with
                    | true -> scrappedProduct.CurrentPrice
                    | false -> dbPrice)
            | _ -> product.PriceHistory |> Seq.append [ scrappedProduct.CurrentPrice ]

        let createProduct (scrappedProduct: ScrappedProduct) =
            { Product.Id = scrappedProduct.Id
              Name = scrappedProduct.Name
              Brand = scrappedProduct.Brand
              PriceHistory = [ scrappedProduct.CurrentPrice ]
              Sources = [ scrappedProduct.Source ]
              Ean = scrappedProduct.Ean }

        storageProduct
        |> AsyncResult.map (fun storageProduct ->
            storageProduct
            |> Id.Set scrappedProduct.Id
            |> PriceHistory.Set(storageProduct |> enrichPriceHistory scrappedProduct)
            |> Sources.Set(storageProduct |> enrichSources scrappedProduct))
        |> AsyncResult.orElseWith (fun e ->
            match e with
            | :? ProductNotFoundException  -> scrappedProduct |> createProduct |> AsyncResult.ok
            | _ -> e |> AsyncResult.error)

    let start () =
        asyncResult {
            // Scrape products from sources
            let! scrappedProducts = scrapeProducts ()

            // Enrich the Products from DB with scrapped data
            let enrichedProducts =
                scrappedProducts
                |> Seq.map enrichWithDBProduct
                |> Async.Parallel
                |> Async.map (List.ofArray >> List.sequenceResultM)

            // Upsert products to DB
            let! savedProducts =
                enrichedProducts
                |> AsyncResult.bind (
                    Seq.map productClient.Upsert
                    >> Async.Parallel
                    >> Async.map (fun result ->
                        result
                        |> Seq.iter (
                            Result.teeError (fun e -> logger.Log(Exception("Error running scrapper.", e)))
                            >> ignore
                        )
                        result)
                    >> Async.map (List.ofArray >> List.sequenceResultM)
                )

            return savedProducts
        }
        |> AsyncResult.teeError (fun e -> logger.Log(Exception("Error running scrapper.", e)))
