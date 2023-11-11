namespace GNogueira.SupermarketScrapeTool.Clients

open System
open System.Linq.Expressions
open FsToolkit.ErrorHandling
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Linq
open System.Linq
open Microsoft.FSharp.Control
open FSharpPlus
open FSharp.Core
open GNogueira.SupermarketScrapeTool.Common.FSharp.CosmosDB
open GNogueira.SupermarketScrapeTool.Common.FSharp.Queryable
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter

type IProductClient =
    abstract GetById: string -> Async<Result<ProductDto, exn>>
    abstract GetPaged: count: int -> page: int -> Async<Result<ProductDto seq, exn>>
    abstract GetSources: unit -> Async<Result<string seq, exn>>
    abstract Search: count: int -> page: int -> productName: Option<string> -> supermarket: Option<string> -> updatedAfter: Option<DateTime> -> updatedBefore: Option<DateTime> -> updatedAt: Option<DateTime> -> sorting: Option<Sorting> -> Async<Result<ProductDto seq, exn>>

type ProductClient(cosmosDbClient: ICosmosDbClient) =
    let productContainer = cosmosDbClient.ProductsContainer

    interface IProductClient with
        member this.GetById id =
            productContainer.ReadItemAsync<ProductDto>(id, PartitionKey id)
            |> AsyncResult.ofTask
            |> AsyncResult.map (fun ir -> ir.Resource)

        member this.GetPaged count page =
            async {
                let linqFeed =
                    productContainer.GetItemLinqQueryable<ProductDto>()
                    |> skip (count * (page - 1))
                    |> take count
                    |> toFeedIterator

                let rec readResultsAsync (linqFeed: FeedIterator<_>) acc =
                    async {
                        if linqFeed.HasMoreResults then
                            let! response = linqFeed.ReadNextAsync() |> Async.AwaitTask
                            return! acc @ (response.Resource |> Seq.toList) |> readResultsAsync linqFeed
                        else
                            return acc
                    }

                let! result = readResultsAsync linqFeed []

                return! result |> List.toSeq |> Ok |> Async.retn
            }

        member this.Search
            (count: int)
            (page: int)
            (productName: Option<string>)
            (supermarket: Option<string>)
            (updatedAfter: Option<DateTime>)
            (updatedBefore: Option<DateTime>)
            (updatedAt: Option<DateTime>)
            (sorting: Option<Sorting>)
            =
            async {

                let filterByName (productName: string option) (query: IQueryable<ProductDto>) =
                    match productName with
                    | Some name -> query.Where(fun product -> product.Name.ToLower().Contains(name.ToLower()))
                    | None -> query

                let filterBySupermarket (supermarket: string option) (query: IQueryable<ProductDto>) =
                    match supermarket with
                    | Some market -> query.Where(fun product -> product.Source.ToLower().Contains(market.ToLower()))
                    | None -> query

                let filterByDateRange (updatedAfter: DateTime option) (updatedBefore: DateTime option) (updatedAt: DateTime option) (query: IQueryable<ProductDto>) =
                    let filterByUpdatedAfter (q: IQueryable<ProductDto>) =
                         match updatedAfter with
                         | Some date -> q.Where(fun product -> product.updatedOn >= date)
                         | None -> q
                    let filterByUpdatedBefore (q: IQueryable<ProductDto>) =
                        match updatedBefore with
                        | Some date -> q.Where(fun product -> product.updatedOn <= date)
                        | None -> q
                    let filterByUpdatedAt (q: IQueryable<ProductDto>) =
                        match updatedAt with
                        | Some date -> q.Where(fun product -> product.updatedOn >= date && product.updatedOn <= date)
                        | None -> q
                    
                    query |> filterByUpdatedAfter |> filterByUpdatedBefore |> filterByUpdatedAt

                let linqFeed =
                    productContainer.GetItemLinqQueryable<ProductDto>()
                    |> filterByName productName
                    |> filterBySupermarket supermarket
                    |> filterByDateRange updatedAfter updatedBefore updatedAt
                    |> sort sorting <@ fun product -> product.updatedOn @>
                    |> skip (count * (page - 1))
                    |> take count
                    |> toFeedIterator
                    
                let rec readResultsAsync acc =
                    async {
                        if linqFeed.HasMoreResults then
                            let! response = linqFeed.ReadNextAsync() |> Async.AwaitTask
                            return! acc @ (response.Resource |> Seq.toList) |> readResultsAsync
                        else
                            return acc
                    }

                let! result = readResultsAsync []

                return! result |> List.toSeq |> Ok |> Async.retn
            }

        member this.GetSources() =
            async {
                let feedIterator = productContainer.GetItemQueryIterator<{|Source: string|}>("SELECT DISTINCT c.Source FROM c")

                let rec readResultsAsync acc =
                    async {
                        if feedIterator.HasMoreResults then
                            let! response = feedIterator.ReadNextAsync() |> Async.AwaitTask
                            return! acc @ (response.Resource |> Seq.toList) |> readResultsAsync
                        else
                            return acc
                    }

                let! result = readResultsAsync []

                return! result |> List.toSeq |> Seq.map (fun obj -> obj.Source) |> Ok |> Async.retn
            }