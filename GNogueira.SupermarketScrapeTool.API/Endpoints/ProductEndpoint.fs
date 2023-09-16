namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open System
open Giraffe
open GNogueira.SupermarketScrapeTool.API.Dto
open Microsoft.AspNetCore.Http

module ProductEndpoint =
    
    let getProductByIdHandler  (productId: Guid) (next: HttpFunc) (ctx: HttpContext) =
        // Replace this with logic to fetch product by ID from the Cosmos DB
        let product = { Id = productId; Name = "Sample Product"; Price = 9.99; PriceUnit = "USD" } |> Some

        match product with
        | Some p -> json p next ctx
        | None -> setStatusCode 404 next ctx
    
    let allEndpoints : HttpHandler =
        choose [
            GET >=> routef "/products/%O" getProductByIdHandler
        ]
