module GNogueira.SupermarketScrapeTool.Scrapper.ContinenteScrapper

open System
open System.Text.RegularExpressions
open System.Net.Http
open FSharpPlus
open FSharp.Core
open HtmlAgilityPack
open FsToolkit.ErrorHandling
open Fizzler.Systems.HtmlAgilityPack
open GNogueira.SupermarketScrapeTool.Scrapper.Models
open GNogueira.SupermarketScrapeTool.Common
open GNogueira.SupermarketScrapeTool.Models

let pageStart = 0
// The maximum number of products that can be fetched is 24.
let pageSize = 24

// The number of products to scrape.
let scrapeSize = 5000

let supermarketUrl pageStart pageSize =
    $"https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Search-UpdateGrid?cgid=col-produtos&pmin=0%%2e01&start={pageStart}&sz={pageSize}"

let productUrl url = $"https://www.continente.pt/{url}"

let supermarket = Continente

[<AutoOpen>]
module ProductExtractor =
    
    type ProductPagination = {
        PageNumber: int
        PageSize: int
        Count: int
    }
    let findProductNodes (doc: HtmlDocument) : seq<HtmlNode> =
        doc.DocumentNode.QuerySelectorAll("[data-pid].product")
    
    let findPagination (doc: HtmlDocument) : Result<ProductPagination, string> =
        result {
            let! footer =
                doc.DocumentNode
                |> HtmlNode.tryQuerySelector ".grid-footer[data-sort-options]"
                |> Result.ofOption "Footer not found."
            
            let! count =
                footer
                |> HtmlNode.tryGetAttributeValue "data-total-count"
                |> Result.ofOption "Total count not found."
                |> Result.bind (fun s -> s |> tryParse |> Result.ofOption "Total count is not a number.")
            
            let! pageSize =
                footer
                |> HtmlNode.tryGetAttributeValue "data-page-size"
                |> Result.ofOption "Page size not found."
                |> Result.bind (fun s -> s |> tryParse |> Result.ofOption "Page size is not a number.")
            
            let! pageNumber =
                footer
                |> HtmlNode.tryGetAttributeValue "data-page-number"
                |> Result.ofOption "Page number not found."
                |> Result.bind (fun s -> s |> tryParse |> Result.ofOption "Page number is not a number.")
        
            return
                { PageNumber = pageNumber
                  PageSize = pageSize
                  Count = count }
            
        }

    type HtmlNode with

        static member getProductImageUrl(product: HtmlNode) =
            let selector = "img.ct-tile-image"
            
            product
            |> HtmlNode.tryGet selector
            |> Option.bind (HtmlNode.tryGetAttributeValue "data-src")
            |> Option.map String.trimWhiteSpaces
            |> Result.ofOption $"Product image url not found on CSS selector {selector}."

        static member getProductUrl(product: HtmlNode) =
            let selector = ".ct-pdp-details a"
            
            product
            |> HtmlNode.tryGet selector
            |> Option.bind (HtmlNode.tryGetAttributeValue "href")
            |> Option.map String.trimWhiteSpaces
            |> Result.ofOption $"Product url not found on CSS selector {selector}."

        static member getProductName(product: HtmlNode) =
            let selector = ".pwc-tile--description"
            
            product
            |> HtmlNode.tryGet selector
            |> Option.map (HtmlNode.innerText >> String.trimWhiteSpaces)
            |> Result.ofOption $"Product name not found on CSS selector {selector}."

        static member getProductPrice(product: HtmlNode) =
            let selector = ".js-product-price .value"
            
            product
            |> HtmlNode.tryGet selector
            |> Option.bind (HtmlNode.tryGetAttributeValue "content")
            |> Option.map (String.trimWhiteSpaces >> String.replace "," "." >> float)
            |> Result.ofOption $"Price not found on CSS selector {selector}."

        static member getProductId(product: HtmlNode) =
            let selector = "[data-pid]"
            
            product
            |> HtmlNode.tryGetAttributeValue "data-pid"
            |> Option.map String.trimWhiteSpaces
            |> Result.ofOption $"Product id not found on CSS selector {selector}."

        static member getPriceUnit(product: HtmlNode) =
            let selector = ".pwc-m-unit"
            
            product
            |> HtmlNode.tryGet selector
            |> Option.bind (HtmlNode.innerText >> (Regex.tryMatch "([a-zA-Z]+)"))
            |> Option.bind (Regex.tryGroup 1)
            |> Option.map String.toLower
            |> Result.ofOption $"Price unit not found on CSS selector {selector}."
            |> Result.bind PriceUnit.ofString

        static member getBrand(product: HtmlNode) =
            let selector = ".pwc-tile--brand.col-tile--brand"
            
            product
            |> HtmlNode.tryGet selector
            |> Option.map (HtmlNode.innerText >> String.trimWhiteSpaces)
            |> Result.ofOption $"Brand not found on CSS selector {selector}."

        static member getEan(doc: HtmlDocument) =
            let selector = ".nutriInfoTab .js-nutritional-tab-anchor"
            
            // https://www.continente.pt/on/demandware.store/Sites-continente-Site/default/Product-ProductNutritionalInfoTab?pid=2305902&ean=5601260037109&supplierid=5600000494387&enabledce=true
            doc.DocumentNode
            |> HtmlNode.tryQuerySelector selector
            |> Option.bind (HtmlNode.tryGetAttributeValue "data-url")
            |> Option.bind (Regex.tryMatch "ean=([0-9]+)")
            |> Option.bind (Regex.tryGroup 1)
            |> Result.ofOption $"EAN not found on CSS selector {selector}."

let makeRequest (url: string) =
    new HttpClient()
    |> HttpClient.getAsync (url |> HttpClient.Url.String)
    |> AsyncResult.map _.Content
    |> AsyncResult.bind (
        HttpContent.readAsStringAsync
        >> Async.AwaitTask
        >> Async.Catch
        >> Async.map Result.ofChoice
        >> AsyncResult.mapError (_.Message)
    )

let stringToHtml s =
    let stripHtmlErrors (str: string) =
        
        // Finds a text like the following. Note the double closing </p> tag:
        // <p>
        // Recomendação diária: Segundo a Organização Mundial de Saúde, o grupo "Fruta", da Roda dos Alimentos, deverá contribuir para a nossa dieta com 3 a 5 porções diárias. Uma porção de morangos corresponde a 160g, ou seja, cerca de 8 unidades.</p>
        // </p>
        // And removes the second closing </p> tag.
        let removeSyntaxError2 str =
            Regex.Replace(str, "/<p>(\n.+?)<\/p>\n<\/p>/gm", "<p>$1</p>")
        
        let removeSyntaxError1 str =
            str
            |> String.replace "<li class=\"ct-footer-link\">Tel: 218 247 247 <small>(Chamada para a rede fixa nacional)</small></a>" ""
        
        // Removes ul nested inside a p tag.
        let removeSyntaxError3 str =
            Regex.Replace(str, "<p.*?>\n*(<ul>(?:\n|.)*?<\/ul>)\n*<\/p>", "$1")
        
        let removeSyntaxError4 str =
            Regex.Replace(str, "(<p class=\"mb-0\">\nBenef&iacute;cios:\n<\/p>\n<p class=\"mb-20\">(?:.|\n)+?)<\/p>\n<\/p>\n", "$1\n<\/p>\n")
        
        str
        |> removeSyntaxError1
        |> removeSyntaxError2
        |> removeSyntaxError3
        |> removeSyntaxError4
    
    HtmlDocument()
    |> HtmlDocument.loadHtml (s |> stripHtmlErrors)

let productOfHtml (product: HtmlNode) : Async<Result<ScrappedProduct, string>> =
    asyncResult {
        let enrichErrorWithProduct productId productName =
            Result.mapError (fun e -> $"{e}. ProductId is {productId} and ProductName is {productName}")
        
        let! productId = product |> HtmlNode.getProductId
        let! productName = product |> HtmlNode.getProductName |> Result.mapError (fun e -> $"{e}. ProductId is {productId}")
        
        let enrichError r = enrichErrorWithProduct productId productName r
        
        let! priceUnit = product |> HtmlNode.getPriceUnit |> enrichError
        let! productPrice = product |> HtmlNode.getProductPrice |> enrichError
        let! productUrl = product |> HtmlNode.getProductUrl |> enrichError
        let! productBrand = product |> HtmlNode.getBrand |> enrichError

        let! ean =
            makeRequest productUrl
            |> Async.map (Result.bind stringToHtml)
            |> Async.map (Result.bind HtmlNode.getEan)
            |> Async.map enrichError

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
    |> AsyncResult.mapError (fun e -> $"Failed to scrape product. Reason: '{e}'.")

let scrapePage (pageStart: int) : Async<Result<ScrappedProduct, string> seq> =
    async {
        let! docResult =
            supermarketUrl pageStart pageSize
            |> makeRequest
            |> AsyncResult.bind (stringToHtml >> Async.retn)

        match docResult with
        | Ok doc ->
            let productNodes = findProductNodes doc
            let products = productNodes |> Seq.map productOfHtml |> Async.Parallel |> Async.map Seq.ofArray
            return! products
        | Result.Error e ->
            return Seq.singleton (Result.Error e)
    }

let scrape () : Async<Result<ScrappedProduct, string> seq> =
    async {
        // Get the initial page to determine total products count
        let! docResult =
            supermarketUrl pageStart pageSize
            |> makeRequest
            |> AsyncResult.bind (stringToHtml >> Async.retn)

        match docResult with
        | Ok doc ->
            match doc |> findPagination with
            | Ok pagination ->
                let totalProducts = min scrapeSize pagination.Count
                let totalPages = (totalProducts + pageSize - 1) / pageSize // Calculate total pages needed
                
                let pageStarts = [for i in 0..totalPages-1 -> i * pageSize]
                
                let scrapeAllPages() =
                    pageStarts
                    |> Seq.map scrapePage
                    |> Async.Parallel
                
                let! allResults = scrapeAllPages()
                
                let combinedResults = allResults |> Seq.collect id
                
                return combinedResults
            | Result.Error e ->
                return Seq.singleton (Result.Error e)
        | Result.Error e ->
            return Seq.singleton (Result.Error e)
    }