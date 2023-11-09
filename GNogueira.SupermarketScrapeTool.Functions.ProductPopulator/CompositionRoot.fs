namespace GNogueira.SupermarketScrapeTool.Functions.ProductPopulator

open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common.Logging
open GNogueira.SupermarketScrapeTool.Common.Secrets

module CompositionRoot =
    let mutable logger = ConsoleLogger() :> ILogger
    let setLogger newLogger = logger <- newLogger
    let secretManager = SecretManager logger
    let secretClient = SecretClient logger :> ISecretClient
    let cosmosDbClient = CosmosDbClient(secretClient, logger) :> ICosmosDbClient
    let productPriceClient = ProductPriceClient cosmosDbClient :> IProductPriceClient
