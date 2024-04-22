namespace GNogueira.SupermarketScrapeTool.Common

open FSharp.Core
open FSharpPlus

module Env =
    
    type Environment = Dev | Prod
    
    let isDevEnv =
        match System.Environment.GetEnvironmentVariable("SUPERMARKET_SCRAPE_TOOL_ENV") |> Option.ofString |> Option.map String.toLower with
        | Some "dev" | Some "development" -> true
        | None | Some _ -> false
    
    let current =
        match isDevEnv with
        | true -> Dev
        | false -> Prod