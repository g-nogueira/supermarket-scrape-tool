namespace GNogueira.SupermarketScrapeTool.Tests

open System
open FSharpPlus
open HtmlAgilityPack
open Xunit
open FsUnit.Xunit
open FsUnitTyped
open GNogueira.SupermarketScrapeTool.Scrapper.ContinenteScrapper

type ContinenteScrapperTests() =

    [<Fact>]
    member this.``Make Request - should fail gracefully with invalid URL``() =
        let expectedError = Result.Error "Invalid URI was provided. Got 'invalid-url'."

        let result = "invalid-url" |> makeRequest |> Async.RunSynchronously

        result |> shouldEqual expectedError

    [<Fact>]
    member this.``StringToHtml - should fail gracefully with invalid HTML``() =
        // arrange
        let expectedError =
            Result.Error "The input is not a valid HTML string. Start tag <html> was not found"

        // act
        let actualError = stringToHtml ContinenteScrapperHelper.invalidHtml

        // assert
        actualError |> shouldEqual expectedError

    [<Fact>]
    member this.``StringToHtml - should convert a valid HTML string to an HtmlDocument``() =
        // arrange
        let validHtml = ContinenteScrapperHelper.productListResponse

        // act
        let result: Result<HtmlDocument, string> = validHtml |> stringToHtml

        // assert
        match result with
        | Ok doc ->
            doc |> should be ofExactType<HtmlDocument>
            doc.ParseErrors |> should be Empty
        | Error msg ->
            Assert.Fail(
                "Expected: Ok HtmlAgilityPack.HtmlDocument"
                + Environment.NewLine
                + "But was: Error "
                + msg
            )

    [<Fact>]
    member this.``findProductNodes - should return a list of valid nodes``() =
        // arrange
        let expectedLength = 1
        let expectedPriceUnit = ContinenteScrapperHelper.priceUnit |> Ok
        let expectedProductName = ContinenteScrapperHelper.productName |> Ok
        let expectedProductPrice = ContinenteScrapperHelper.productPrice |> Ok
        let expectedProductId = ContinenteScrapperHelper.productId |> Ok
        let expectedProductUrl = ContinenteScrapperHelper.productUrl |> Ok
        let expectedProductBrand = ContinenteScrapperHelper.productBrand |> Ok
        let expectedProductImageUrl = ContinenteScrapperHelper.productImageUrl |> Ok

        // act
        let result =
            ContinenteScrapperHelper.productListResponse
            |> stringToHtml
            |> Result.map findProductNodes

        let resultHead =
            result |> Result.bind (Seq.tryHead >> Option.toResultWith "Empty list.")

        let actualLength =
            if (result |> Result.isOk) then
                result |> Result.defaultValue [] |> Seq.length
            else
                -1

        let actualPriceUnit = resultHead |> Result.bind HtmlNode.getPriceUnit
        let actualProductName = resultHead |> Result.bind HtmlNode.getProductName
        let actualProductPrice = resultHead |> Result.bind HtmlNode.getProductPrice
        let actualProductId = resultHead |> Result.bind HtmlNode.getProductId
        let actualProductUrl = resultHead |> Result.bind HtmlNode.getProductUrl
        let actualProductBrand = resultHead |> Result.bind HtmlNode.getBrand
        let actualProductImageUrl = resultHead |> Result.bind HtmlNode.getProductImageUrl

        // assert
        actualLength |> shouldEqual expectedLength
        actualPriceUnit |> shouldEqual expectedPriceUnit
        actualProductName |> shouldEqual expectedProductName
        actualProductPrice |> shouldEqual expectedProductPrice
        actualProductId |> shouldEqual expectedProductId
        actualProductUrl |> shouldEqual expectedProductUrl
        actualProductBrand |> shouldEqual expectedProductBrand
        actualProductImageUrl |> shouldEqual expectedProductImageUrl
