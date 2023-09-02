module GNogueira.SupermarketScrapeTool.Service.SecretManager

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Microsoft.Azure.Cosmos
open Azure.Identity
open Azure.Security.KeyVault.Secrets

let azureKeyVaultUrl = "https://smkt-scrape-tool-secrets.vault.azure.net/"

let logMessage text =
    Logger.message $"Azure KeyVault: {text}"

let secretClient = SecretClient(Uri azureKeyVaultUrl, DefaultAzureCredential())

let getSecretAsync s =
    async {
        logMessage $"Getting {s}"

        let! secretValue = secretClient.GetSecretAsync s |> Async.AwaitTask

        return secretValue.Value.Value
    }

let getCosmosDbConnectionString () =
    getSecretAsync "CosmosDbConnectionString"
    |> AsyncResult.ofAsync
    |> AsyncResult.teeError Logger.exc

let getCosmosDbName () =
    getSecretAsync "CosmosDbName"
    |> AsyncResult.ofAsync
    |> AsyncResult.teeError Logger.exc

let getCosmosDbContainerName () =
    getSecretAsync "CosmosDbContainerName"
    |> AsyncResult.ofAsync
    |> AsyncResult.teeError Logger.exc

let createCosmosClient (cosmosDbConnectionString: string) =
    new CosmosClient(cosmosDbConnectionString)
