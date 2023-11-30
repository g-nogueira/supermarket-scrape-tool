module GNogueira.SupermarketScrapeTool.Service.PingoDoceScrapper

open System
open System.Net.Http
open Models
open FSharpPlus
open FSharp.Core
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Service.Models

let supermarketName = PingoDoce
let pageStart = 0
let pageSize = 10000

let supermarketUrl =
    $"https://mercadao.pt/api/catalogues/6107d28d72939a003ff6bf51/products/search?query=[]&from={pageStart}&size={pageSize}&esPreference=0.6774024649118144"

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

and PingoDoceSource =
    { firstName: string
      secondName: string
      thirdName: string
      unitPrice: float
      netContentUnit: string
      sku: string
      imagesNumber: int
      slug: string }

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
        let productData = product._source
        let name = productData.firstName
        let price = productData.unitPrice
        let priceUnit = productData.netContentUnit |> PriceUnit.ofString
        let currentDate = DateTime.Now.ToString("yyyy-MM-dd")

        { ProductDto.id = Guid.NewGuid()
          ExternalId = "" 
          Name = name
          Prices = price
          // PriceUnit = priceUnit
          Source = supermarketName
          Date = currentDate
          Url = product |> PingoDoceProduct.mkUrl |> Some
          ImageUrl = product |> PingoDoceProduct.mkImageUrl |> Some }

    supermarketUrl
    |> makeRequest
    |> Async.map (deserialize >> getProductDTOs >> Seq.map toProduct)
