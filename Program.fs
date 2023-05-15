module GNogueira.SupermarketScrapeTool.Service.Main

open Microsoft.Azure.Cosmos
open Models
open FSharpPlus
open FsToolkit.ErrorHandling
open SecretManager
open System

let azureContainerName = "Items"

let createDatabase id (cosmosClient: CosmosClient) =
    cosmosClient.CreateDatabaseIfNotExistsAsync id |> AsyncResult.ofTask

let createContainer (id: string) (database: DatabaseResponse) =
    database.Database.CreateContainerIfNotExistsAsync(id, "/id")
    |> AsyncResult.ofTask

let addItemsToContainer<'T> item (container: ContainerResponse) =
    container.Container.CreateItemAsync<'T> item |> AsyncResult.ofTask

let saveToFile (filename: string) =
    let saveItem (item) =
        use writer = System.IO.File.AppendText(filename)
        let json = Newtonsoft.Json.JsonConvert.SerializeObject(item)
        writer.WriteLine(json)

    saveItem

let createCosmosClient (cosmosDbConnectionString: string) =
    new CosmosClient(cosmosDbConnectionString)

let scrapeWebsite =
    function
    | Continente -> ContinenteScrapper.scrape ()
    | PingoDoce -> PingoDoceScrapper.scrape ()

let websites = [ Continente; PingoDoce ]

let initAzureConnection () =
    let stopOnFail =
        function
        | Ok a -> a
        | Error e -> failwith $"Stopped run due to fail to get Azure Cosmos DB connection string.\n{Error}"

    let connectionString =
        getCosmosDbConnectionString () |> Async.RunSynchronously |> stopOnFail

    let dbName = getCosmosDbName () |> Async.RunSynchronously |> stopOnFail

    connectionString
    |> createCosmosClient
    |> createDatabase dbName
    |> AsyncResult.bind (createContainer azureContainerName)

//let saveMethod = saveToDynamoDB table // Use saveToDynamoDB or saveToFile based on your choice
//let saveItem = saveToFile "output.txt"
let saveItem container item =
    Logger.message $"Saving item {item.Name}"

    container
    |> AsyncResult.bind (addItemsToContainer<Product> item)
    |> AsyncResult.teeError (fun e -> printfn $"{e.Message}")

[<EntryPoint>]
let main argv =
    let teeError = Result.teeError Logger.exc
    let container = initAzureConnection ()

    let scrape website =
        $"Scraping {website |> ProductSource.toString}:" |> Logger.message

        scrapeWebsite website |> Seq.map (saveItem container)

    websites
    |> Seq.map scrape
    |> Seq.collect id
    |> Async.Parallel
    |> Async.map (teeError |> Array.map)
    |> Async.Ignore
    |> Async.RunSynchronously

    0
