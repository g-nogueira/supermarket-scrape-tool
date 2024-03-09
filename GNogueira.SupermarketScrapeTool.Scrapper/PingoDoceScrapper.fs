module GNogueira.SupermarketScrapeTool.Scrapper.PingoDoceScrapper

open System
open System.Net.Http
open FSharpPlus
open FSharp.Core
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Common.Result
open GNogueira.SupermarketScrapeTool.Scrapper.Models

let supermarket = PingoDoce
let pageStart = 0
let pageSize = 10000
let query = "[]"

let supermarketUrl =
    $"https://mercadao.pt/api/catalogues/6107d28d72939a003ff6bf51/products/search?query={query}&from={pageStart}&size={pageSize}&esPreference=0.6275797548940417"

type PingoDoceResponse =
    { sections: PingoDoceSections
      categories: PingoDoceCategory list
      brands: PingoDoceBrand list }

and PingoDoceSections = { ``null``: SectionDTO }

and SectionDTO =
    { total: int
      products: seq<PingoDoceProduct>
      order: int
      name: string option }

and PingoDoceProduct =
    { _index: string
      _type: string
      _id: string
      _score: float
      _source: PingoDoceSource }

and Brand = { id: string; name: string }

and PingoDoceSource =
    { firstName: string
      secondName: string
      thirdName: string
      unitPrice: float
      netContentUnit: string
      sku: string
      imagesNumber: int
      // The name used in the url
      // Example: https://mercadao.pt/store/pingo-doce/product/vela-formato-4-papstar-1-un
      slug: string
      // The ean code is used to identify the product globally
      ean: string
      brand: Brand }

and PingoDoceCategory = { id: string; name: string }
and PingoDoceBrand = { id: string; name: string }


type PingoDoceProduct with

    static member mkImageUrl dto =
        $"https://res.cloudinary.com/fonte-online/image/upload/c_fill,h_300,q_auto,w_300/v1/PDO_PROD/{dto._source.sku}_{dto._source.imagesNumber}"

    static member mkUrl dto =
        $"https://mercadao.pt/store/pingo-doce/product/{dto._source.slug}"

let makeRequest (url: string) =
    async {
        use httpClient = new HttpClient()

        httpClient.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36"
        )

        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return content
    }

let scrape () =
    let deserialize = Newtonsoft.Json.JsonConvert.DeserializeObject<PingoDoceResponse>

    let getProductDTOs data = data.sections.``null``.products

    let toProduct product =
        result {
            let productDto = product._source
            let! priceUnit = productDto.netContentUnit |> PriceUnit.ofString
            let productName = productDto.firstName
            let productPrice = productDto.unitPrice
            let productId = product._id
            let productUrl = product |> PingoDoceProduct.mkUrl
            let productBrand = productDto.brand.name
            let productImageUrl = product |> PingoDoceProduct.mkImageUrl
            let productEan = productDto.ean

            let productSource =
                { ProductSource.ExternalId = productId
                  Name = supermarket
                  Url = productUrl
                  ImageUrl = Some productImageUrl }

            return
                { Product.ProductId = ProductId(Guid.NewGuid())
                  Name = productName
                  PriceHistory =
                    { PriceEntry.Date = DateTime.Now
                      Price = productPrice
                      PriceUnit = priceUnit
                      Source = productSource }
                    |> Seq.singleton
                  Brand = productBrand
                  Sources = productSource |> Seq.singleton
                  Ean = Some productEan }
        }

    supermarketUrl
    |> makeRequest
    |> Async.map (deserialize >> getProductDTOs >> Seq.map toProduct)
