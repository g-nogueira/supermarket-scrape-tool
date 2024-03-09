namespace GNogueira.SupermarketScrapeTool.Scrapper

open FSharpPlus
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Common.Extensions.FSharp

module ProductPriceClientEx =
    let fetchProductsFromSource (productPriceClient: IProductPriceClient) (source: string) =
        
        let toProductsDtos products =
            products |> Seq.map ProductDto.fromProductPriceDto

        let appendDistinct newProductPrices existingProducts =
            // Create a map of existing products indexed by name
            let existingProductMap =
                existingProducts
                |> Seq.groupBy (fun p -> p.Name.ToLower())
                |> Seq.map (fun (name, products) -> name, products |> Seq.maxBy (fun p -> p.Date))
                |> Map.ofSeq
            
            // Match new products with existing ones by name and keep the most recent
            let mergedProducts =
                newProductPrices
                |> Seq.map (fun newProductPrice ->
                    match Map.tryFind (newProductPrice.Name |> String.toLower) existingProductMap with
                    | Some existingProduct ->
                        if existingProduct.Date > newProductPrice.Date then
                            existingProduct
                        else
                            newProductPrice
                    | None -> newProductPrice
                )
            
            let existingProductNames = existingProducts |> Seq.map (fun p -> p.Name |> String.toLower) |> Set.ofSeq
            let newProducts =
                newProductPrices
                |> Seq.groupBy (fun p -> p.Name |> String.toLower)
                |> Seq.map (fun (_, products) -> products |> Seq.maxBy (fun p -> p.Date))
                // |> Seq.filter (fun p -> not (existingProductNames |> Set.contains (p.Name |> String.toLower)))

            // acc |> Seq.append newProducts

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
        static member FetchProducts productPriceClient =
            readAllFromContainer productPriceClient
