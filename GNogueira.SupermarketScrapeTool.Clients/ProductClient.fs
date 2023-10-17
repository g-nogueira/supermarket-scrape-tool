namespace GNogueira.SupermarketScrapeTool.Clients

open System
open FsToolkit.ErrorHandling
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Linq
open System.Linq
open Microsoft.FSharp.Control
open FSharpPlus
open FSharp.Core


    
module FSQuery =
    type Sorting =
    | Asc
    | Desc
    with
        static member ofString (s: string) =
            match s.ToLower() with
            | "asc" -> Ok Asc
            | "desc" -> Ok Desc
            | invalid -> Error $"Invalid sorting type. Expected 'asc' or 'desc', received: '{invalid}'."

    type Predicate = 
    | Above of int
    | Below of int
    | And of Predicate * Predicate
    | Or of Predicate * Predicate
    | Not of Predicate
    // | Equal of obj * obj

    let rec eval(t) =
      match t with
      | Above n -> <@ fun x -> x >= n @>
      | Below n -> <@ fun x -> x < n @>
      | And (t1,t2) -> <@ fun x -> (%eval t1) x && (%eval t2) x @>
      | Or (t1,t2) -> <@ fun x -> (%eval t1) x || (%eval t2) x @>
      | Not (t0) -> <@ fun x -> not((%eval t0) x) @>

type IProductClient =
    abstract GetById: string -> Async<Result<ProductDto, exn>>
    abstract GetPaged: count: int -> page: int -> Async<Result<ProductDto seq, exn>>
    abstract Search: count: int -> page: int -> productName: Option<string> -> supermarket: Option<string> -> createdAfter: Option<DateTime> -> createdBefore: Option<DateTime> -> createdAt: Option<DateTime> -> sorting: Option<FSQuery.Sorting> -> Async<Result<ProductDto seq, exn>>

type ProductClient(cosmosDbClient: ICosmosDbClient) =
    let productContainer = cosmosDbClient.DefaultContainer

    interface IProductClient with
        member this.GetById id =
            cosmosDbClient.DefaultContainer.ReadItemAsync<ProductDto>(id, PartitionKey id)
            |> AsyncResult.ofTask
            |> AsyncResult.map (fun ir -> ir.Resource)

        member this.GetPaged count page =
            async {
                let queryable = productContainer.GetItemLinqQueryable<ProductDto>()
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
            (sorting: Option<FSQuery.Sorting>)
            =
            async {
                let skip v (q:IQueryable<ProductDto>) = q.Skip v

                let take (value: 'T) (q:IQueryable<ProductDto>) =
                    match box value with
                    | :? int as v -> q.Take v
                    | :? Range as v -> q.Take v
                    | _ -> q
                
                let toFeedIterator (v: IQueryable<_>) = v.ToFeedIterator()

                let filterByName (productName: string option) (query: IQueryable<ProductDto>) =
                    match productName with
                    | Some name -> query.Where(fun product -> product.Name.ToLower().Contains(name.ToLower()))
                    | None -> query

                let filterBySupermarket (supermarket: string option) (query: IQueryable<ProductDto>) =
                    match supermarket with
                    | Some market -> query.Where(fun product -> product.Source.ToLower().Contains(market.ToLower()))
                    | None -> query

                let filterByDateRange (createdAfter: DateTime option) (createdBefore: DateTime option) (createdAt: DateTime option) (query: IQueryable<ProductDto>) =
                    let filterByCreatedAfter (q: IQueryable<ProductDto>) =
                         match createdAfter with
                         | Some date -> q.Where(fun product -> product.Date >= date)
                         | None -> q
                    let filterByCreatedBefore (q: IQueryable<ProductDto>) =
                        match createdBefore with
                        | Some date -> q.Where(fun product -> product.Date <= date)
                        | None -> q
                    let filterByCreatedAt (q: IQueryable<ProductDto>) =
                        match createdAt with
                        | Some date -> q.Where(fun product -> product.Date >= date && product.Date <= date)
                        | None -> q
                    
                    query |> filterByCreatedAfter |> filterByCreatedBefore |> filterByCreatedAt
                
                let sort sortBy (query: IQueryable<ProductDto>) =
                    sortBy
                    |> Option.map
                            (function
                            | FSQuery.Asc -> query.OrderBy(fun product -> product.Date)
                            | FSQuery.Desc -> query.OrderByDescending(fun product -> product.Date))
                    |> Option.defaultValue (query.OrderByDescending(fun product -> product.Date))

                let linqFeed =
                    productContainer.GetItemLinqQueryable<ProductDto>()
                    |> filterByName productName
                    |> filterBySupermarket supermarket
                    |> filterByDateRange createdAfter createdBefore createdAt
                    |> sort sorting
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