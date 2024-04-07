module GNogueira.SupermarketScrapeTool.Scrapper
open FsToolkit.ErrorHandling
open GNogueira.SupermarketScrapeTool.Scrapper
open GNogueira.SupermarketScrapeTool.Scrapper.CurrentLogger
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Common
open FSharp.Core
open FSharpPlus

let websitesToScrape = [ Continente; PingoDoce ]

let scrapeWebsite =
    function
    | Continente -> ContinenteScrapper.scrape ()
    | PingoDoce -> PingoDoceScrapper.scrape ()
let scrapeProducts () =
    websitesToScrape
    |> Seq.map scrapeWebsite
    |> Async.Parallel
    |> Async.map (Seq.collect id)
    |> Async.tee (Seq.iter ((Result.teeError logger.Error) >> ignore))
    |> Async.map (Seq.choose Result.toOption)