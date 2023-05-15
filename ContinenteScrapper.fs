module GNogueira.SupermarketScrapeTool.Service.ContinenteScrapper

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Text.RegularExpressions
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack
open FSharpPlus
open Models

let pageStart = 0
let pageSize = 999999

let supermarketUrl =
    $"https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Search-UpdateGrid?cgid=col-produtos&pmin=0%%2e01&start={pageStart}&sz={pageSize}"

let supermarketName = Continente
let productSelector = "[data-pid].product"
let nameSelector = ".pwc-tile--description"
let priceSelector = ".js-product-price .value"
let priceSelectorAttr = "content"
let priceUnitSelector = ".pwc-m-unit"

let makeRequest (url: string) =
    async {
        use httpClient = new HttpClient()

        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return content
    }
    |> Async.RunSynchronously


let getPriceUnit (product: HtmlNode) =
    let matchText pattern text = Regex.Match(text, pattern)
    let getGroup (id: int) (matchObj: Match) = matchObj.Groups[id].Value
    let toLower (text: string) = text.ToLower()

    product.InnerText |> matchText "([a-zA-Z]+)" |> getGroup 1 |> toLower

let scrape () =
    let getNode selector (node: HtmlNode) = node.QuerySelector(selector)

    let getName (node: HtmlNode) =
        node.InnerText |> String.trimWhiteSpaces

    let getPrice (node: HtmlNode) =
        match priceSelectorAttr with
        | "" -> node.InnerText.Trim().Replace(",", ".")
        | attr -> node.GetAttributeValue(attr, "").Trim().Replace(",", ".")
        |> float

    let nodeToProduct (product: HtmlNode) =
        { id = Guid.NewGuid()
          Name = product |> getNode nameSelector |> getName
          Price = product |> getNode priceSelector |> getPrice
          PriceUnit = product |> getNode priceUnitSelector |> getPriceUnit |> PriceUnit.ofString
          Source = supermarketName
          Date = DateTime.Now.ToString("yyyy-MM-dd") }

    let findProductNodes (doc: HtmlDocument) =
        doc.DocumentNode.QuerySelectorAll(productSelector)

    let stringToHtml s =
        let doc = HtmlDocument()
        doc.LoadHtml(s)
        doc

    supermarketUrl
    |> makeRequest
    |> stringToHtml
    |> findProductNodes
    |> Seq.map nodeToProduct
