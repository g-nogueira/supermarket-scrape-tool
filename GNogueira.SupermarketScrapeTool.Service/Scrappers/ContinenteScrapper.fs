module GNogueira.SupermarketScrapeTool.Service.ContinenteScrapper

open System
open System.Net.Http
open System.Text.RegularExpressions
open FSharpPlus
open FSharp.Core
open HtmlAgilityPack
open FsToolkit.ErrorHandling.OptionCE
open Fizzler.Systems.HtmlAgilityPack
open CurrentLogger
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Service.Models

let pageStart = 0
let pageSize = 10000

let supermarketUrl =
    $"https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Search-UpdateGrid?cgid=col-produtos&pmin=0%%2e01&start={pageStart}&sz={pageSize}"

let supermarket = Continente

[<AutoOpen>]
module ProductExtractor =
    let private productContainer = "[data-pid].product"

    let private querySelector (node: HtmlNode) = node.QuerySelector >> Option.ofObj

    let private getNode selectors (node: HtmlNode) =
        selectors |> Seq.tryPick (querySelector node)

    let findProductNodes (doc: HtmlDocument) =
        doc.DocumentNode.QuerySelectorAll(productContainer)

    type HtmlNode with

        static member getProductImageUrl(product: HtmlNode) =
            product
            |> getNode [ "#product-set-img" ]
            |> Option.map (fun node -> node.GetAttributeValue("data-src", "").Trim())

        static member getProductUrl(product: HtmlNode) =
            product
            |> getNode [ ".pwc-tile--description"; ".product-set-title .text-product-set" ]
            |> Option.map (fun node -> node.GetAttributeValue("href", "").Trim())

        static member getProductName(product: HtmlNode) =
            product
            |> getNode [ ".pwc-tile--description"; ".product-set-title .text-product-set" ]
            |> Option.map (fun node -> node.InnerText |> String.trimWhiteSpaces)

        static member getProductPrice(product: HtmlNode) =
            product
            |> getNode [ ".js-product-price .value" ]
            |> Option.map (fun node ->
                node.GetAttributeValue("content", "")
                |> String.trimWhiteSpaces
                |> String.replace "," "."
                |> float)

        static member getProductId(product: HtmlNode) =
            product
            |> getNode [ "[data-pid].product" ]
            |> Option.map (fun node -> node.GetAttributeValue("data-pid", "").Trim())

        static member getPriceUnit(product: HtmlNode) =
            let matchText pattern text = Regex.Match(text, pattern)
            let getGroup (id: int) (matchObj: Match) = matchObj.Groups[id].Value

            product
            |> getNode [ ".pwc-m-unit" ]
            |> Option.map (fun node -> node.InnerText |> matchText "([a-zA-Z]+)" |> getGroup 1 |> String.toLower)
            |> Option.bind (PriceUnit.ofString >> Result.toOption)


let makeRequest (url: string) =
    async {
        use httpClient = new HttpClient()

        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return content
    }

let scrape () =

    let productOfHtml (product: HtmlNode) =
        option {
            let getPriceUnit productNode =
                productNode
                |> HtmlNode.getPriceUnit
                |> Option.defaultValue Unknown
                |> PriceUnit.toString

            return
                { ProductDto.id = Guid.NewGuid()
                  Name = product |> HtmlNode.getProductName |> Option.defaultValue ""
                  Prices =
                    product
                    |> HtmlNode.getProductPrice
                    |> Option.map (fun price ->
                        { ProductPriceDto.Date = DateTime.Now
                          Price = price
                          PriceUnit = product |> getPriceUnit })
                    |> Option.map Seq.singleton
                    |> Option.defaultValue Seq.empty
                  Source = supermarket |> ProductSource.toString
                  Date = DateTime.Now
                  Url = product |> HtmlNode.getProductUrl |> Option.defaultValue ""
                  ImageUrl = product |> HtmlNode.getProductImageUrl |> Option.defaultValue ""
                  ExternalId = product |> HtmlNode.getProductId |> Option.defaultValue "" }
        }

    let stringToHtml s =
        let doc = HtmlDocument()
        doc.LoadHtml(s)
        doc

    supermarketUrl
    |> makeRequest
    |> Async.map (
        stringToHtml
        >> ProductExtractor.findProductNodes
        >> Seq.map productOfHtml
        >> Seq.choose id
    )
