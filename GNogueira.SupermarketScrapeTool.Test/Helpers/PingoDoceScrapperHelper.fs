namespace GNogueira.SupermarketScrapeTool.Tests

open System
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Scrapper.Models
open GNogueira.SupermarketScrapeTool.Scrapper.PingoDoceScrapper

module PingoDoceScrapperHelper =

    let invalidHtml = "<htmll><</html>"

    let requestUrl =
        "https://mercadao.pt/api/catalogues/6107d28d72939a003ff6bf51/products/search?&from=0&size=1&esPreference=0.6275797548940417"

    let parsedProductListResponse =
        { PingoDoceResponse.sections =
            { ``null`` =
                { total = 15230
                  SectionDTO.products =
                    Array.singleton
                        { PingoDoceProduct._id = "61efb72439642300115f899d"
                          _index = "pdo-prod-product-v4"
                          _type = "product"
                          _score = 0
                          _source =
                            { PingoDoceSource.firstName = "Vela Formato 4 Papstar"
                              secondName = ""
                              thirdName = ""
                              unitPrice = 0.89
                              netContentUnit = "UN"
                              sku = "736474"
                              imagesNumber = 1
                              slug = "vela-formato-4-papstar-1-un"
                              eans = [| "4002911193044" |]
                              brand =
                                { Brand.id = "5a775d9da0bf0f000f27a40a"
                                  name = "Papstar" } } }
                  order = 0
                  name = None } }
          categories = [||]
          brands = [||] }

    let product =
        { ScrappedProduct.Id = "4002911193044" |> ProductId
          Name = "Vela Formato 4 Papstar"
          CurrentPrice =
            { PriceEntry.Date = DateTime.UtcNow
              Price = 0.89
              PriceUnit = PriceUnit.Un
              Source =
                { ProductSource.ProductId = "61efb72439642300115f899d"
                  Name = SourceName.PingoDoce
                  ProductUrl = "https://mercadao.pt/store/pingo-doce/product/vela-formato-4-papstar-1-un"
                  ProductImageUrl = Some "https://res.cloudinary.com/fonte-online/image/upload/c_fill,h_300,q_auto,w_300/v1/PDO_PROD/736474_1" } }
          Brand = "Papstar"
          Source =
            { ProductSource.ProductId = "61efb72439642300115f899d"
              Name = SourceName.PingoDoce
              ProductUrl = "https://mercadao.pt/store/pingo-doce/product/vela-formato-4-papstar-1-un"
              ProductImageUrl = Some "https://res.cloudinary.com/fonte-online/image/upload/c_fill,h_300,q_auto,w_300/v1/PDO_PROD/736474_1" }
          Ean = "4002911193044" }

    let productListResponse =
        """
{
    "sections": {
        "null": {
            "total": 15230,
            "products": [
                {
                    "_index": "pdo-prod-product-v4",
                    "_type": "product",
                    "_id": "61efb72439642300115f899d",
                    "_score": 0,
                    "_source": {
                        "firstName": "Vela Formato 4 Papstar",
                        "secondName": "",
                        "thirdName": "",
                        "longDescription": "Vela Formato 4 Papstar",
                        "shortDescription": "Vela Formato 4 Papstar",
                        "sku": "736474",
                        "imagesNumber": 1,
                        "grossWeight": 0.023,
                        "capacity": "1 UN",
                        "netContent": 1,
                        "netContentUnit": "UN",
                        "averageWeight": 0,
                        "onlineStatus": "AVAILABLE",
                        "status": "PUBLISHED",
                        "slug": "vela-formato-4-papstar-1-un",
                        "tags": [],
                        "categories": [
                            {
                                "name": "Artigos de Festa e Descartáveis",
                                "id": "61eeddf9fd2bff003f5082c5"
                            },
                            {
                                "name": "Casa",
                                "id": "61eeddeffd2bff003f508244"
                            }
                        ],
                        "eans": [
                            "4002911193044"
                        ],
                        "brand": {
                            "name": "Papstar",
                            "id": "5a775d9da0bf0f000f27a40a"
                        },
                        "catalogueId": "6107d28d72939a003ff6bf51",
                        "categoriesArray": [
                            "Artigos de Festa e Descartáveis"
                        ],
                        "leafCategories": [
                            {
                                "name": "Artigos de Festa e Descartáveis",
                                "id": "61eeddf9fd2bff003f5082c5"
                            }
                        ],
                        "isPerishable": false,
                        "ancestorsCategoriesArray": [
                            "Casa"
                        ],
                        "regularPrice": 0.89,
                        "campaignPrice": 0,
                        "buyingPrice": 0.89,
                        "unitPrice": 0.89,
                        "promotion": {
                            "beginDate": null,
                            "amount": null,
                            "payAmount": null,
                            "endDate": null,
                            "takeAmount": null,
                            "type": null
                        },
                        "minimumOrderableQuantity": 1,
                        "maximumOrderableQuantity": 30,
                        "countriesOfOrigin": [],
                        "additionalInfo": "✱ Alertamos para o facto de o Mercadão (Fonte Negócios Online, S.A.) não ser responsável por possíveis divergências no que concerne à informação sobre os produtos apresentados no nosso website, partilhada pelo fornecedor ou fabricante, e a que vigora nos rótulos dos mesmos. Recomendamos que considere sempre a informação apresentada no produto que recebe.",
                        "durabilityDays": 0,
                        "activePromotion": false,
                        "advertising": [],
                        "suggest_catalogue": [
                            {
                                "input": "Vela Formato 4 Papstar",
                                "weight": 20,
                                "contexts": {
                                    "catalogue": "6107d28d72939a003ff6bf51"
                                }
                            },
                            {
                                "input": "Vela Formato 4 Papstar",
                                "weight": 20,
                                "contexts": {
                                    "catalogue": "6107d28d72939a003ff6bf51"
                                }
                            },
                            {
                                "input": "Papstar",
                                "weight": 19,
                                "contexts": {
                                    "catalogue": "6107d28d72939a003ff6bf51"
                                }
                            }
                        ],
                        "unitNoVatPrice": 0.7236,
                        "campaignNoVatPrice": 0,
                        "qualitativeIcons": [],
                        "capacityType": null,
                        "noVatPrice": 0.7236,
                        "vatTax": 0.23,
                        "buyingNoVatPrice": 0.7236,
                        "lowestBuyingPrice": 0.89
                    }
                }
            ],
            "order": 0,
            "name": null
        }
    },
    "categories": [],
    "brands": []
}"""
