namespace GNogueira.SupermarketScrapeTool.Scrapper

open Microsoft.Azure.Cosmos
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Scrapper.CompositionRoot
open GNogueira.SupermarketScrapeTool.Scrapper.AzureHelper
open GNogueira.SupermarketScrapeTool.Scrapper.Models

[<AutoOpen>]
module Start =

    let enrichWithDBProduct (scrappedProduct: ScrappedProduct) =
        let storageProduct = productClient.GetById scrappedProduct.Id

        let enrichSources (scrappedProduct: ScrappedProduct) (product: Product) =
            let isSameSource (source: ProductSource) (source': ProductSource) = source.Name = source'.Name

            product.Sources
            |> Seq.map (fun source ->
                match scrappedProduct.Source |> isSameSource source with
                | true -> scrappedProduct.Source
                | false -> source)

        let enrichPriceHistory (scrappedProduct: ScrappedProduct) (product: Product) =
            product.PriceHistory |> Seq.append [ scrappedProduct.CurrentPrice ]

        storageProduct
        |> AsyncResult.map (fun storageProduct ->
            storageProduct
            |> PriceHistory.Set(storageProduct |> enrichPriceHistory scrappedProduct)
            |> Sources.Set(storageProduct |> enrichSources scrappedProduct))

    let upsertProduct (container: Container) (product: ProductDto) =
        // logger.Log(LogMessage.Information $"Upserting Product '{source |> ProductSource.deconstruct}'.")

        asyncResult {
            let! response = container.UpsertItemAsync(product, PartitionKey(product.id))
            return response
        }

    let start () =
        asyncResult {
            let! container = initAzureConnection logger

            // Scrape products from sources
            let! scrappedProducts = scrapeProducts ()

            // Fetch products from DB
            let! products = productClient.GetAll()

            // Enrich the Products from DB with scrapped data
            let enrichedProducts =
                scrappedProducts
                |> Seq.map enrichWithDBProduct
                |> Async.Parallel
                |> Async.map (List.ofArray >> List.sequenceResultM)

            // Upsert products to DB
            let! savedProducts =
                enrichedProducts
                |> AsyncResult.map (Seq.map ProductDto.ofDomain)
                |> AsyncResult.bind (
                    Seq.map (upsertProduct container)
                    >> Async.Parallel
                    >> Async.map (List.ofArray >> List.sequenceResultM)
                )

            return savedProducts
        }
        |> AsyncResult.teeError (fun e -> logger.Log(LogMessage.Exception("Error running scrapper.", e)))
