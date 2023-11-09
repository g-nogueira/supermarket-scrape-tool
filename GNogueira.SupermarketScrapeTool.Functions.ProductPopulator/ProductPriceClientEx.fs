namespace GNogueira.SupermarketScrapeTool.Functions.ProductPopulator

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Common.Extensions.FSharp
open Microsoft.Azure.Cosmos

module ProductPriceClientEx =
    let fetchProductsFromSource (productPriceClient: IProductPriceClient) (source: string) =
        let toProductsDtos (products: seq<ProductPriceDto>) =
            products |> Seq.map ProductDto.fromProductPriceDto

        let appendDistinct (products: seq<ProductPriceDto>) (acc: seq<ProductDto>) =
            acc
            |> Seq.append (products |> toProductsDtos)
            |> Seq.sortByDescending (fun p -> p.updatedOn)
            |> Seq.distinctBy (fun p -> p.Name |> String.toLower)

        let rec fetchProducts pageNumber acc =
            productPriceClient.Search 1000 pageNumber None (Some source) None None None None
            |> AsyncResult.bind (function
                | EmptySeq -> AsyncResult.retn (ProductSource source, acc) // If no more products, return the accumulated products
                | products -> fetchProducts (pageNumber + 1) (acc |> appendDistinct products) // Fetch next page and accumulate products
            )

        // Start fetching products for the source from page 1
        fetchProducts 1 Seq.empty

    let enrichWithProducts productPriceClient (productSources: seq<string>) =
        productSources
        |> Seq.map (fetchProductsFromSource productPriceClient)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Seq.map (Result.teeError (fun e -> CompositionRoot.logger.Log (LogMessage.Exception (e.Message, e))))
        |> Seq.map (Result.defaultWith (fun e -> failwith e.Message))

    let readAllFromContainer (productPriceClient: IProductPriceClient) =
        productPriceClient.GetSources()
        |> AsyncResult.map (enrichWithProducts productPriceClient)

    type ProductPriceClient with
        static member GenerateProducts productPriceClient =
            readAllFromContainer productPriceClient
