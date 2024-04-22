namespace GNogueira.SupermarketScrapeTool.Scrapper

open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common.Logging

module CompositionRoot =
    let mutable logger = ConsoleLogger() :> ILogger
    let setLogger newLogger = logger <- newLogger
    let secretClient = SecretClient logger :> ISecretClient
    let cosmosDbClient = CosmosDbClient(secretClient, logger) :> ICosmosDbClient
    let productClient = ProductClient cosmosDbClient :> IProductClient
