namespace GNogueira.SupermarketScrapeTool.Service

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Microsoft.Azure.Cosmos
open Azure.Identity
open Azure.Security.KeyVault.Secrets

type SecretManager(logger: ILogger) =

    let azureKeyVaultUrl = "https://smkt-scrape-tool-secrets.vault.azure.net/"

    let logMessage text =
        logger.Information $"Azure KeyVault: {text}"

    let secretClient = SecretClient(Uri azureKeyVaultUrl, DefaultAzureCredential())

    let getSecretAsync s =
        async {
            logMessage $"Getting {s}"

            let! secretValue = secretClient.GetSecretAsync s |> Async.AwaitTask

            return secretValue.Value.Value
        }

    member _.GetCosmosDbConnectionString() =
        getSecretAsync "CosmosDbConnectionString"
        |> AsyncResult.ofAsync
        |> AsyncResult.teeError (logger.Exception String.Empty)

    member _.GetCosmosDbName() =
        getSecretAsync "CosmosDbName"
        |> AsyncResult.ofAsync
        |> AsyncResult.teeError (logger.Exception String.Empty)

    member _.GetCosmosDbContainerName() =
        getSecretAsync "CosmosDbContainerName"
        |> AsyncResult.ofAsync
        |> AsyncResult.teeError (logger.Exception String.Empty)

    member _.CreateCosmosClient(cosmosDbConnectionString: string) =
        new CosmosClient(cosmosDbConnectionString)
