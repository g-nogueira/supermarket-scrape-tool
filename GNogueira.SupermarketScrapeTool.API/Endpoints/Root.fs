namespace GNogueira.SupermarketScrapeTool.API.Endpoints

open Giraffe

module RootController =
    let routes : HttpHandler =
        choose [ProductEndpoint.allEndpoints]