namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open System
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.API.Clients
open Giraffe
open Microsoft.AspNetCore.Http

module ProductEndpoint =
    let getProductByIdHandler  (productId: Guid) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductClient>()

        let product =
            productClient.GetById (productId |> string)
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let getProductsPagedHandler (count: int, page: int) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductClient>()

        let product =
            productClient.GetPaged count page
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let allEndpoints : HttpHandler =
        choose [
            GET >=> routef "/products/%O" getProductByIdHandler
            GET >=> routef "/products/paged/%i/%i" getProductsPagedHandler
        ]
