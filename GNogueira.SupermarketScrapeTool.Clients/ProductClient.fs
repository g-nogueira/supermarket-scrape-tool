namespace GNogueira.SupermarketScrapeTool.Clients

open System
open System.Linq
open FSharp.Core
open FSharpPlus
open Microsoft.Azure.Cosmos
open Microsoft.FSharp.Control
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Common.FSharp.CosmosDB
open GNogueira.SupermarketScrapeTool.Common.FSharp.Queryable
open GNogueira.SupermarketScrapeTool.Models

type IProductClient =
    abstract GetById: string -> Async<Result<Product, exn>>
    abstract GetPaged: count: int -> page: int -> Async<Result<Product seq, exn>>
    abstract GetAll: unit -> Async<Result<Product seq, exn>>
    abstract GetSources: unit -> Async<Result<string seq, exn>>
    abstract Search: count: int -> page: int -> productName: Option<string> -> supermarket: Option<string> -> updatedAfter: Option<DateTime> -> updatedBefore: Option<DateTime> -> updatedAt: Option<DateTime> -> sorting: Option<Sorting> -> Async<Result<Product seq, exn>>

[<AutoOpen>]
module Extensions =
    type ProductSource with

        static member ofDto(dto: ProductSourceDto) =
            result {
                let! sourceName = dto.Name |> SourceName.ofString |> Result.mapError exn

                return
                    { ExternalId = dto.ExternalId
                      Name = sourceName
                      Url = dto.Url
                      ImageUrl = dto.ImageUrl }
            }

    type PriceEntry with

        static member ofDto(dto: PriceEntryDto) =
            result {
                let! priceUnit = dto.PriceUnit |> PriceUnit.ofString |> Result.mapError exn
                let! source = dto.Source |> ProductSource.ofDto

                return
                    { Date = dto.Date
                      Price = dto.Price
                      PriceUnit = priceUnit
                      Source = source }
            }
    type Product with

       static member ofDto(dto: ProductDto) =
            result {
                let! priceHistory = dto.PriceHistory |> Seq.map PriceEntry.ofDto |> Seq.sequenceResultM
                let! sources = dto.Sources |> Seq.map ProductSource.ofDto |> Seq.sequenceResultM
                
                return
                    { Id = dto.id |> ProductId
                      Name = dto.Name
                      Brand = dto.Brand
                      PriceHistory = priceHistory
                      Sources = sources
                      Ean = dto.Ean }
            }

type ProductClient(cosmosDbClient: ICosmosDbClient) =
    let productContainer = cosmosDbClient.ProductsContainer
    
    interface IProductClient with
        member _.GetById id =
            productContainer.ReadItemAsync<ProductDto>(id, PartitionKey id)
            |> AsyncResult.ofTask
            |> AsyncResult.map (fun itemResponse -> itemResponse.Resource)
            |> Async.map (Result.bind Product.ofDto)

        member _.GetPaged count page =
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

                return result |> List.toSeq |> Seq.map Product.ofDto |> Seq.sequenceResultM
            }

        member _.GetAll () =
            async {
                let linqFeed =
                    productContainer.GetItemLinqQueryable<ProductDto>()
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

                return result |> List.toSeq |> Seq.map Product.ofDto |> Seq.sequenceResultM
            }

        member _.Search
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
                    | Some market -> query.Where(fun product -> product.Sources.Any(fun source -> source.Name.ToLower().Contains(market.ToLower())))
                    | None -> query

                let filterByDateRange (updatedAfter: DateTime option) (updatedBefore: DateTime option) (updatedAt: DateTime option) (query: IQueryable<ProductDto>) =
                    let filterByUpdatedAfter (q: IQueryable<ProductDto>) =
                         match updatedAfter with
                         | Some date -> q.Where(fun product -> product.UpdatedOn >= date)
                         | None -> q
                    let filterByUpdatedBefore (q: IQueryable<ProductDto>) =
                        match updatedBefore with
                        | Some date -> q.Where(fun product -> product.UpdatedOn <= date)
                        | None -> q
                    let filterByUpdatedAt (q: IQueryable<ProductDto>) =
                        match updatedAt with
                        | Some date -> q.Where(fun product -> product.UpdatedOn >= date && product.UpdatedOn <= date)
                        | None -> q
                    
                    query |> filterByUpdatedAfter |> filterByUpdatedBefore |> filterByUpdatedAt

                let linqFeed =
                    productContainer.GetItemLinqQueryable<ProductDto>()
                    |> filterByName productName
                    |> filterBySupermarket supermarket
                    |> filterByDateRange updatedAfter updatedBefore updatedAt
                    |> sort sorting <@ fun product -> product.UpdatedOn @>
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

                return result |> List.toSeq |> Seq.map Product.ofDto |> Seq.sequenceResultM
            }

        member _.GetSources() =
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

                return! result |> List.toSeq |> Seq.map (fun obj -> obj.Source) |> AsyncResult.retn
            }