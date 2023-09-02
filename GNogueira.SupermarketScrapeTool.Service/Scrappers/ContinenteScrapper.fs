module GNogueira.SupermarketScrapeTool.Service.ContinenteScrapper

open System
open System.Net.Http
open System.Text.RegularExpressions
open GNogueira.SupermarketScrapeTool.Service.Models
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack
open FSharpPlus
open FsToolkit.ErrorHandling.OptionCE

let pageStart = 0
let pageSize = 10000

let supermarketUrl =
    $"https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Search-UpdateGrid?cgid=col-produtos&pmin=0%%2e01&start={pageStart}&sz={pageSize}"

let supermarket = Continente
let productSelector = "[data-pid].product"

let nameSelectors =
    [ ".pwc-tile--description"; ".product-set-title .text-product-set" ]

let priceSelector = ".js-product-price .value"
let priceSelectorAttr = "content"
let priceUnitSelector = ".pwc-m-unit"
let imageSelector = "#product-set-img"
let imageSelectorAttr = "data-src"

let urlSelector =
    [ ".pwc-tile--description"; ".product-set-title .text-product-set" ]

let urlSelectorAttr = "href"

let makeRequest (url: string) =
    async {
        use httpClient = new HttpClient()

        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return content
    }


let getPriceUnit (product: HtmlNode) =
    let matchText pattern text = Regex.Match(text, pattern)
    let getGroup (id: int) (matchObj: Match) = matchObj.Groups[id].Value
    let toLower (text: string) = text.ToLower()

    product.InnerText
    |> matchText "([a-zA-Z]+)"
    |> getGroup 1
    |> toLower
    |> PriceUnit.ofString

let scrape () =
    let querySelector (node: HtmlNode) = node.QuerySelector >> Option.ofObj

    let getNode selectors (node: HtmlNode) =
        selectors |> Seq.tryPick (querySelector node)

    let getProductImageUrl (node: HtmlNode) =
        match imageSelectorAttr with
        | "" -> node.InnerText.Trim().Replace(",", ".")
        | attr -> node.GetAttributeValue(attr, "").Trim()

    let getProductUrl (node: HtmlNode) =
        match urlSelectorAttr with
        | "" -> node.InnerText.Trim().Replace(",", ".")
        | attr -> node.GetAttributeValue(attr, "").Trim()

    let getProductName (node: HtmlNode) =
        node.InnerText |> String.trimWhiteSpaces

    let getProductPrice (node: HtmlNode) =
        match priceSelectorAttr with
        | "" -> node.InnerText.Trim().Replace(",", ".")
        | attr -> node.GetAttributeValue(attr, "").Trim().Replace(",", ".")
        |> float

    let nodeToProduct (product: HtmlNode) =
        option {
            let! nameNode = product |> getNode nameSelectors
            let! priceNode = product |> getNode [ priceSelector ]
            let! priceUnitNode = product |> getNode [ priceUnitSelector ]
            let imageUrlNode = product |> getNode [ imageSelector ]
            let urlNode = product |> getNode urlSelector

            return
                { id = Guid.NewGuid()
                  Name = nameNode |> getProductName
                  Price = priceNode |> getProductPrice
                  PriceUnit = priceUnitNode |> getPriceUnit
                  Source = supermarket
                  Date = DateTime.Now.ToString("yyyy-MM-dd")
                  Url = urlNode |> Option.map getProductUrl
                  ImageUrl = imageUrlNode |> Option.map getProductImageUrl }
        }

    let findProductNodes (doc: HtmlDocument) =
        doc.DocumentNode.QuerySelectorAll(productSelector)

    let stringToHtml s =
        let doc = HtmlDocument()
        doc.LoadHtml(s)
        doc

    supermarketUrl
    |> makeRequest
    |> Async.map (stringToHtml >> findProductNodes >> Seq.map nodeToProduct >> Seq.choose id)
