namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open System
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.API.DTOs
open GNogueira.SupermarketScrapeTool.Clients
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

    let getProductsPagedHandler (request: PagedRequestDto) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductClient>()

        let product =
            productClient.GetPaged request.ItemsPerPage request.Page
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let v1Endpoints : HttpHandler =
        choose [
            GET >=> routef "/products/%O" getProductByIdHandler
            GET >=> route "/products/" >=> bindQuery<PagedRequestDto> None getProductsPagedHandler
        ]
