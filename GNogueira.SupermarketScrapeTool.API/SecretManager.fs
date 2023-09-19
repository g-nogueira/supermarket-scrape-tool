namespace GNogueira.SupermarketScrapeTool.API

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Azure.Identity
open Azure.Security.KeyVault.Secrets
open Microsoft.Extensions.Logging

type ISecretManager =
    abstract GetCosmosDbConnectionString: unit -> Async<Result<string, 'a>>
    abstract GetCosmosDbName: unit -> Async<Result<string, 'a>>
    abstract GetCosmosDbContainerName: unit -> Async<Result<string, 'a>>

type SecretManager(logger: ILogger<SecretManager>) =

    let azureKeyVaultUrl = "https://smkt-scrape-tool-secrets.vault.azure.net/"

    let logMessage text =
        logger.LogInformation $"Azure KeyVault: {text}"

    let secretClient = SecretClient(Uri azureKeyVaultUrl, DefaultAzureCredential())

    let getSecretAsync s =
        async {
            logMessage $"Getting {s}"

            let! secretValue = secretClient.GetSecretAsync s |> Async.AwaitTask

            return secretValue.Value.Value
        }

    interface ISecretManager with
        member _.GetCosmosDbConnectionString() =
            getSecretAsync "CosmosDbConnectionString"
            |> AsyncResult.ofAsync
            |> AsyncResult.teeError (fun ex -> logger.LogError "Error accessing Azure KeyVault.")

        member _.GetCosmosDbName() =
            getSecretAsync "CosmosDbName"
            |> AsyncResult.ofAsync
            |> AsyncResult.teeError (fun ex -> logger.LogError "Error accessing Azure KeyVault.")

        member _.GetCosmosDbContainerName() =
            getSecretAsync "CosmosDbContainerName"
            |> AsyncResult.ofAsync
            |> AsyncResult.teeError (fun ex -> logger.LogError "Error accessing Azure KeyVault.")
