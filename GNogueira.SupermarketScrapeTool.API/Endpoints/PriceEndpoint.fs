namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open System
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.API.DTOs
open GNogueira.SupermarketScrapeTool.Clients
open Giraffe
open Microsoft.AspNetCore.Http
open GNogueira.SupermarketScrapeTool.Common.FSharp.Queryable

module PriceEndpoint =
    let getPriceByIdHandler  (priceId: Guid) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductPriceClient>()

        let product =
            productClient.GetById (priceId |> string)
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let getPricesPagedHandler (request: PagedRequestDto) (next: HttpFunc) (ctx: HttpContext) =
        let productClient = ctx.GetService<IProductPriceClient>()

        let product =
            productClient.GetPaged request.ItemsPerPage request.Page
            |> Async.RunSynchronously

        match product with
        | Ok p -> json p next ctx
        | Error e ->
            setBodyFromString e.Message |> ignore
            setStatusCode 404 next ctx

    let searchPricesHandler (request: SearchPriceRequestDto) (next: HttpFunc) (ctx: HttpContext) =
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
            >=> subRoute "/prices"
               (choose [
                    route "/" >=> tryBindQuery<PagedRequestDto> parsingError None getPricesPagedHandler
                    routef "/%O" getPriceByIdHandler
                    route "/search" >=> tryBindQuery<SearchPriceRequestDto> parsingError None searchPricesHandler
                    routef "/test/%O" testSomething
                ])
        ]
