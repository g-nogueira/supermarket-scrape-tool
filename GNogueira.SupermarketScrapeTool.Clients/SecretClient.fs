namespace GNogueira.SupermarketScrapeTool.Clients

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Azure.Identity
open Azure.Security.KeyVault.Secrets
open GNogueira.SupermarketScrapeTool.Common.Logging

type ISecretClient =
    abstract GetCosmosDbConnectionString: unit -> Async<Result<string, 'a>>
    abstract GetCosmosDbName: unit -> Async<Result<string, 'a>>
    abstract GetCosmosDbContainerName: unit -> Async<Result<string, 'a>>

type AzureSecretClient = SecretClient
type SecretClient(logger: ILogger) =

    let azureKeyVaultUrl = "https://smkt-scrape-tool-secrets.vault.azure.net/"

    let logMessage text =
        logger.Log (Information $"Azure KeyVault: {text}")

    let secretClient = AzureSecretClient(Uri azureKeyVaultUrl, DefaultAzureCredential())

    let getSecretAsync s =
        async {
            logMessage $"Getting {s}"

            let! secretValue = secretClient.GetSecretAsync s |> Async.AwaitTask

            return secretValue.Value.Value
        }

    interface ISecretClient with
        member _.GetCosmosDbConnectionString() =
            getSecretAsync "CosmosDbConnectionString"
            |> AsyncResult.ofAsync
            |> AsyncResult.teeError (fun ex -> logger.Log(Error "Error accessing Azure KeyVault."))

        member _.GetCosmosDbName() =
            getSecretAsync "CosmosDbName"
            |> AsyncResult.ofAsync
            |> AsyncResult.teeError (fun ex -> logger.Log(Error "Error accessing Azure KeyVault."))

        member _.GetCosmosDbContainerName() =
            getSecretAsync "CosmosDbContainerName"
            |> AsyncResult.ofAsync
            |> AsyncResult.teeError (fun ex -> logger.Log(Error "Error accessing Azure KeyVault."))
