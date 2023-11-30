namespace GNogueira.SupermarketScrapeTool.Functions.ProductPopulator

open System
open GNogueira.SupermarketScrapeTool.Clients

type ProductDto =
    { id: Guid
      Name: string
      Source: string
      Url: string
      ImageUrl: string
      updatedOn: DateTime
      Price: float }
    
    static member fromProductPriceDto (productPriceDto: ProductPriceDto) =
        { id = Guid.NewGuid()
          Name = productPriceDto.Name
          Source = productPriceDto.Source
          Url = productPriceDto.Url
          ImageUrl = productPriceDto.ImageUrl
          updatedOn = productPriceDto.Date
          Price = productPriceDto.Price }
