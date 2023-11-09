namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open System
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.API.DTOs
open GNogueira.SupermarketScrapeTool.Clients
open Giraffe
open Microsoft.AspNetCore.Http
open GNogueira.SupermarketScrapeTool.Common.FSharp.Queryable

module ProductEndpoint =
    let getProductByIdHandler  (productId: Guid) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductPriceClient>()

        let product =
            productClient.GetById (productId |> string)
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let getProductsPagedHandler (request: PagedRequestDto) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductPriceClient>()

        let product =
            productClient.GetPaged request.ItemsPerPage request.Page
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let searchProductsHandler (request: SearchProductRequestDto) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductPriceClient>()

        let product =
            productClient.Search
                request.ItemsPerPage
                request.Page
                request.Title
                request.Supermarket
                request.CreatedAfter
                request.CreatedBefore
                request.CreatedAt
                (request.Sorting |> (Option.bind (Sorting.ofString >> Result.toOption)))
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let testSomething (_ : Guid) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductPriceClient>()

        let product =
            productClient.GetSources()
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let parsingError (err : string) = RequestErrors.BAD_REQUEST err
    let v1Endpoints : HttpHandler =
        choose [
            GET
            >=> subRoute "/products"
               (choose [
                    route "/" >=> tryBindQuery<PagedRequestDto> parsingError None getProductsPagedHandler
                    routef "/%O" getProductByIdHandler
                    route "/search" >=> tryBindQuery<SearchProductRequestDto> parsingError None searchProductsHandler
                    routef "/test/%O" testSomething
                ])
        ]
