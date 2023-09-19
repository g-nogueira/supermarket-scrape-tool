namespace GNogueira.SupermarketScrapeTool.Clients

open FsToolkit.ErrorHandling
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Linq
open System.Linq
open Microsoft.FSharp.Control

type IProductClient =
    abstract GetById: string -> Async<Result<ProductDto, exn>>
    abstract GetPaged: count: int -> page: int -> Async<Result<ProductDto seq, exn>>

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
