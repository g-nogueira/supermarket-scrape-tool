namespace GNogueira.SupermarketScrapeTool.Clients

open System
open System.IO
open FsToolkit.ErrorHandling
open Azure.Identity
open Azure.Security.KeyVault.Secrets
open GNogueira.SupermarketScrapeTool.Common
open Thoth.Json.Net

type ISecretClient =
    abstract GetCosmosDbConnectionString: unit -> Async<Result<string, exn>>
    abstract GetCosmosDbName: unit -> Async<Result<string, exn>>
    abstract GetCosmosDbContainerName: unit -> Async<Result<string, exn>>

type AzureSecretClient = SecretClient

type SecretClient(logger: ILogger) =

    let azureKeyVaultUrl = "https://smkt-scrape-tool-secrets.vault.azure.net/"

    let logMessage text =
        logger.Log(Information $"Azure KeyVault: {text}")

    let secretClient = AzureSecretClient(Uri azureKeyVaultUrl, DefaultAzureCredential())

    let readDevSecret s =
        // For a straightforward doc on Thoth.Json.Net, see https://jordanmarr.github.io/fsharp/thoth-json-net-intro/
        let secretsDecoder: Decoder<string> =
            Decode.object (fun get -> get.Required.Field s Decode.string)

        File.ReadAllText ".secrets" |> Decode.fromString secretsDecoder

    let getSecretAsync s =
        logMessage $"Getting {s}"

        match Env.current with
        | Env.Dev -> readDevSecret s |> AsyncResult.ofResult |> AsyncResult.mapError exn
        | _ ->
            secretClient.GetSecretAsync s
            |> Async.AwaitTask
            |> Async.map (_.Value.Value)
            |> AsyncResult.ofAsync
            |> AsyncResult.mapError exn

    interface ISecretClient with
        member _.GetCosmosDbConnectionString() =
            getSecretAsync "CosmosDbConnectionString"
            |> AsyncResult.teeError (fun e ->
                logger.Log(Exception("Error getting secret CosmosDbConnectionString.", e)))

        member _.GetCosmosDbName() =
            getSecretAsync "CosmosDbName"
            |> AsyncResult.teeError (fun e -> logger.Log(Exception("Error getting secret CosmosDbName.", e)))

        member _.GetCosmosDbContainerName() =
            getSecretAsync "CosmosDbContainerName"
            |> AsyncResult.teeError (fun e -> logger.Log(Exception("Error getting secret CosmosDbContainerName.", e)))
