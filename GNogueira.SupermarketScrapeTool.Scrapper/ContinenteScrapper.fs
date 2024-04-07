module GNogueira.SupermarketScrapeTool.Scrapper.ContinenteScrapper

open System
open System.Net.Http
open System.Text.RegularExpressions
open FSharpPlus
open FSharp.Core
open HtmlAgilityPack
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.OptionCE
open Fizzler.Systems.HtmlAgilityPack
open CompositionRoot
open GNogueira.SupermarketScrapeTool.Scrapper.Models
open GNogueira.SupermarketScrapeTool.Common
open GNogueira.SupermarketScrapeTool.Models


let pageStart = 0
let pageSize = 10000

let supermarketUrl =
    $"https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Search-UpdateGrid?cgid=col-produtos&pmin=0%%2e01&start={pageStart}&sz={pageSize}"

let productUrl url = $"https://www.continente.pt/{url}"

let supermarket = Continente

module HtmlNode =
    let invert f a b = f b a

    let tryQuerySelector selector (node: HtmlNode) =
        selector |> node.QuerySelector |> Option.ofObj

    let getAttributeValue (attribute: string) (def: string) (node: HtmlNode) = node.GetAttributeValue(attribute, def)

    let tryGetAttributeValue (attribute: string) (node: HtmlNode) =
        node.GetAttributeValue(attribute, "") |> Option.ofString

    let tryGetAny selectors (node: HtmlNode) =
        selectors |> Seq.tryPick ((invert tryQuerySelector) node)

    let tryGet selector (node: HtmlNode) = node |> tryQuerySelector selector

    let innerText (node: HtmlNode) = node.InnerText

[<AutoOpen>]
module ProductExtractor =
    let private productContainer = "[data-pid].product"

    let findProductNodes (doc: HtmlDocument) : seq<HtmlNode> =
        doc.DocumentNode.QuerySelectorAll(productContainer)

    type HtmlNode with

        static member getProductImageUrl(product: HtmlNode) =
            product
            |> HtmlNode.tryGet "img.ct-tile-image"
            |> Option.bind (HtmlNode.tryGetAttributeValue "data-src")
            |> Option.map String.trimWhiteSpaces
            |> Result.ofOption "Product image url not found."

        static member getProductUrl(product: HtmlNode) =
            product
            |> HtmlNode.tryGetAny [ ".pwc-tile--description"; ".product-set-title .text-product-set" ]
            |> Option.bind (HtmlNode.tryGetAttributeValue "href")
            |> Option.map (String.trimWhiteSpaces >> productUrl)
            |> Result.ofOption "Product url not found."

        static member getProductName(product: HtmlNode) =
            product
            |> HtmlNode.tryGetAny [ ".pwc-tile--description"; ".product-set-title .text-product-set" ]
            |> Option.map (HtmlNode.innerText >> String.trimWhiteSpaces)
            |> Result.ofOption "Product name not found."

        static member getProductPrice(product: HtmlNode) =
            product
            |> HtmlNode.tryGet ".js-product-price .value"
            |> Option.bind (HtmlNode.tryGetAttributeValue "content")
            |> Option.map (String.trimWhiteSpaces >> String.replace "," "." >> float)
            |> Result.ofOption "Price not found."

        static member getProductId(product: HtmlNode) =
            product
            |> HtmlNode.tryGet "[data-pid].product"
            |> Option.bind (HtmlNode.tryGetAttributeValue "data-pid")
            |> Option.map String.trimWhiteSpaces
            |> Result.ofOption "Product id not found."

        static member getPriceUnit(product: HtmlNode) =
            product
            |> HtmlNode.tryGet ".pwc-m-unit"
            |> Option.bind (HtmlNode.innerText >> (Regex.tryMatch "([a-zA-Z]+)"))
            |> Option.map String.toLower
            |> Result.ofOption "Price unit not found."
            |> Result.bind PriceUnit.ofString

        static member getBrand(product: HtmlNode) =
            product
            |> HtmlNode.tryGet ".pwc-tile--brand.col-tile--brand"
            |> Option.map (HtmlNode.innerText >> String.trimWhiteSpaces)
            |> Result.ofOption "Product brand not found."

        static member getEan(doc: HtmlDocument) =
            // https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Product-ProductNutritionalInfoTab?pid=2305902&ean=5601260037109&supplierid=5600000494387&enabledce=true
            doc.DocumentNode
            |> HtmlNode.tryQuerySelector ".nutriInfoTab .js-nutritional-tab-anchor"
            |> Option.bind (HtmlNode.tryGetAttributeValue "data-url")
            |> Option.bind (Regex.tryMatch "ean=([0-9]+)")
            |> Result.ofOption "Ean not found."


let makeRequest (url: string) =
    async {
        use httpClient = new HttpClient()

        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return content
    }

let stringToHtml s =
    let doc = HtmlDocument()
    doc.LoadHtml(s)
    doc

let scrape () =

    let productOfHtml (product: HtmlNode) =
        asyncResult {
            let! priceUnit = product |> HtmlNode.getPriceUnit
            let! productName = product |> HtmlNode.getProductName
            let! productPrice = product |> HtmlNode.getProductPrice
            let! productId = product |> HtmlNode.getProductId
            let! productUrl = product |> HtmlNode.getProductUrl
            let! productBrand = product |> HtmlNode.getBrand

            let! ean =
                makeRequest productUrl
                |> Async.RunSynchronously
                |> stringToHtml
                |> HtmlNode.getEan

            let productImageUrl = product |> HtmlNode.getProductImageUrl |> Option.ofResult

            let productSource =
                { ProductSource.ProductId = productId
                  Name = supermarket
                  ProductUrl = productUrl
                  ProductImageUrl = productImageUrl }

            return
                { ScrappedProduct.Id = ean |> ProductId
                  Name = productName
                  CurrentPrice =
                    { PriceEntry.Date = DateTime.Now
                      Price = productPrice
                      PriceUnit = priceUnit
                      Source = productSource }
                  Brand = productBrand
                  Source = productSource
                  Ean = ean }
        }

    supermarketUrl
    |> makeRequest
    |> Async.bind (stringToHtml >> findProductNodes >> Seq.map productOfHtml >> Async.Parallel >> Async.map Seq.ofArray)
