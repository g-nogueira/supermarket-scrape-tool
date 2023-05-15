module GNogueira.SupermarketScrapeTool.Service.PingoDoceScrapper

open System
open System.Net.Http
open Models

let supermarketName = PingoDoce
let pageStart = 0
let pageSize = 999999

let supermarketUrl =
    $"https://mercadao.pt/api/catalogues/6107d28d72939a003ff6bf51/products/search?query=[]&from={pageStart}&size={pageSize}&esPreference=0.6774024649118144"


type SourceDTO =
    { firstName: string
      secondName: string
      thirdName: string
      unitPrice: float
      netContentUnit: string

    }

type ProductDTO =
    { _index: string
      _type: string
      _id: string
      _score: float
      _source: SourceDTO }

type SectionDTO =
    { total: int
      products: seq<ProductDTO>
      order: int
      name: string option }

type SectionsDTO = { ``null``: SectionDTO }

type CategoryDTO = { id: string; name: string }

type BrandDTO = { id: string; name: string }

type PingoDoceResponseDTO =
    { sections: SectionsDTO
      categories: CategoryDTO list
      brands: BrandDTO list }

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
    |> Async.RunSynchronously

let scrape () =
    let deserialize =
        Newtonsoft.Json.JsonConvert.DeserializeObject<PingoDoceResponseDTO>

    let getProductDTOs data = data.sections.``null``.products

    let toProduct product =
        let productData = product._source
        let name = productData.firstName
        let price = productData.unitPrice
        let priceUnit = productData.netContentUnit |> PriceUnit.ofString
        let currentDate = DateTime.Now.ToString("yyyy-MM-dd")

        { id = Guid.NewGuid()
          Name = name
          Price = price
          PriceUnit = priceUnit
          Source = supermarketName
          Date = currentDate }

    makeRequest supermarketUrl |> deserialize |> getProductDTOs |> Seq.map toProduct
