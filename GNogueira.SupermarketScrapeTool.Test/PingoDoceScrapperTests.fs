namespace GNogueira.SupermarketScrapeTool.Tests

open Xunit
open FsUnit.Xunit
open FsUnitTyped

open GNogueira.SupermarketScrapeTool.Scrapper.PingoDoceScrapper

type PingoDoceScrapperTests() =

    let makeInvalidRequest () =
        "invalid-url" |> makeRequest |> Async.RunSynchronously

    member this.SetUp() =
        // Setup code here
        ()

    [<Fact>]
    member this.``Make Request - should fail with invalid URL``() =
        let expectedError = Result.Error "The URL is not valid."

        let result = makeInvalidRequest ()

        result |> shouldEqual expectedError

    [<Fact>]
    member this.``Parsing - should parse a valid JSON``() =

        // arrange
        let validJson = PingoDoceScrapperHelper.productListResponse
        let expectedResult = PingoDoceScrapperHelper.parsedProductListResponse

        // act
        let result = validJson |> PingoDoceResponse.ofJson

        // assert
        result |> shouldEqual expectedResult


    [<Fact>]
    member this.``Parsing - should generate Products from a valid JSON``() =
        // arrange
        let validJson = PingoDoceScrapperHelper.productListResponse
        let expectedResponseDto = PingoDoceScrapperHelper.parsedProductListResponse
        let expectedProduct = PingoDoceScrapperHelper.product

        // act
        let actualResponseDto =
            validJson
            |> PingoDoceResponse.ofJson
        
        let actualProduct =
            actualResponseDto
            |> _.sections.``null``.products
            |> Seq.head
            |> toProduct
        
        let actualProductId = actualProduct |> Result.map _.Id
        let actualProductName = actualProduct |> Result.map _.Name
        let actualProductPriceUnit = actualProduct |> Result.map _.CurrentPrice.PriceUnit
        let actualProductCurrentPriceSource = actualProduct |> Result.map _.CurrentPrice.Source
        let actualProductBrand = actualProduct |> Result.map _.Brand
        let actualProductSource = actualProduct |> Result.map _.Source
        let actualProductEan = actualProduct |> Result.map _.Ean

        // assert
        actualResponseDto |> should equal expectedResponseDto

        Ok expectedProduct.Id |> shouldEqual actualProductId
        Ok expectedProduct.Name |> shouldEqual actualProductName
        Ok expectedProduct.CurrentPrice.PriceUnit |> shouldEqual actualProductPriceUnit
        Ok expectedProduct.CurrentPrice.Source |> shouldEqual actualProductCurrentPriceSource
        Ok expectedProduct.Brand |> shouldEqual actualProductBrand
        Ok expectedProduct.Source |> shouldEqual actualProductSource
        Ok expectedProduct.Ean |> shouldEqual actualProductEan
