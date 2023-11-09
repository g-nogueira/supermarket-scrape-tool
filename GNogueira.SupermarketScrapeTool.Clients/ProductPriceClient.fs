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

type IProductPriceClient =
    abstract GetById: string -> Async<Result<ProductPriceDto, exn>>
    abstract GetPaged: count: int -> page: int -> Async<Result<ProductPriceDto seq, exn>>
    abstract GetSources: unit -> Async<Result<string seq, exn>>
    abstract Search: count: int -> page: int -> productName: Option<string> -> supermarket: Option<string> -> createdAfter: Option<DateTime> -> createdBefore: Option<DateTime> -> createdAt: Option<DateTime> -> sorting: Option<Sorting> -> Async<Result<ProductPriceDto seq, exn>>

type ProductPriceClient(cosmosDbClient: ICosmosDbClient) =
    let productContainer = cosmosDbClient.DefaultContainer

    interface IProductPriceClient with
        member this.GetById id =
            cosmosDbClient.DefaultContainer.ReadItemAsync<ProductPriceDto>(id, PartitionKey id)
            |> AsyncResult.ofTask
            |> AsyncResult.map (fun ir -> ir.Resource)

        member this.GetPaged count page =
            async {
                let queryable = productContainer.GetItemLinqQueryable<ProductPriceDto>()
                let matches = queryable.Skip(count * (page - 1)).Take(count)
                let linqFeed = matches.ToFeedIterator()

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

        member this.Search
            (count: int)
            (page: int)
            (productName: Option<string>)
            (supermarket: Option<string>)
            (createdAfter: Option<DateTime>)
            (createdBefore: Option<DateTime>)
            (createdAt: Option<DateTime>)
            (sorting: Option<Sorting>)
            =
            async {

                let filterByName (productName: string option) (query: IQueryable<ProductPriceDto>) =
                    match productName with
                    | Some name -> query.Where(fun product -> product.Name.ToLower().Contains(name.ToLower()))
                    | None -> query

                let filterBySupermarket (supermarket: string option) (query: IQueryable<ProductPriceDto>) =
                    match supermarket with
                    | Some market -> query.Where(fun product -> product.Source.ToLower().Contains(market.ToLower()))
                    | None -> query

                let filterByDateRange (createdAfter: DateTime option) (createdBefore: DateTime option) (createdAt: DateTime option) (query: IQueryable<ProductPriceDto>) =
                    let filterByCreatedAfter (q: IQueryable<ProductPriceDto>) =
                         match createdAfter with
                         | Some date -> q.Where(fun product -> product.Date >= date)
                         | None -> q
                    let filterByCreatedBefore (q: IQueryable<ProductPriceDto>) =
                        match createdBefore with
                        | Some date -> q.Where(fun product -> product.Date <= date)
                        | None -> q
                    let filterByCreatedAt (q: IQueryable<ProductPriceDto>) =
                        match createdAt with
                        | Some date -> q.Where(fun product -> product.Date >= date && product.Date <= date)
                        | None -> q
                    
                    query |> filterByCreatedAfter |> filterByCreatedBefore |> filterByCreatedAt

                let linqFeed =
                    productContainer.GetItemLinqQueryable<ProductPriceDto>()
                    |> filterByName productName
                    |> filterBySupermarket supermarket
                    |> filterByDateRange createdAfter createdBefore createdAt
                    |> sort sorting <@ fun product -> product.Date @>
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