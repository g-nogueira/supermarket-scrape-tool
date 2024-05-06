namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open GNogueira.SupermarketScrapeTool.API.DTOs
open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Models
open Microsoft.AspNetCore.Http

[<AutoOpen>]
module ProductEndpoint =
    let getProductByIdHandler (id: string) (productClient: IProductClient) =
            task {
                let getResult product =
                    match product with
                    | Result.Ok product -> (product |> ProductResponseDto.ofDomain |> TypedResults.Ok) :> IResult
                    | Result.Error e -> TypedResults.BadRequest(e) :> IResult

                let productId, productClient = id, productClient

                let! product = productClient.GetById(productId |> Product.ProductId)

                return getResult product
            }
    
    let getProductListHandler (productClient: IProductClient) =
        task {
            let getResult products =
                match products with
                | Result.Ok products -> (products |> Seq.map ProductResponseDto.ofDomain |> TypedResults.Ok) :> IResult
                | Result.Error e -> TypedResults.BadRequest(e) :> IResult

            let! products = productClient.GetAll()

            return getResult products
        }
    
    let getProductListPaginatedHandler (count: int) (page: int) (productClient: IProductClient) =
        task {
            let getResult products =
                match products with
                | Result.Ok products -> (products |> Seq.map ProductResponseDto.ofDomain |> TypedResults.Ok) :> IResult
                | Result.Error e -> TypedResults.BadRequest(e) :> IResult

            let! products = productClient.GetPaged count page

            return getResult products
        }


