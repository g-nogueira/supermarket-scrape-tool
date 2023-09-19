module GNogueira.SupermarketScrapeTool.Test

open Microsoft.AspNetCore.Hosting
open NUnit.Framework

[<SetUp>]
let Setup () =
    WebHostBuilder()
    ()

[<Test>]
let ``Get Product by ID Returns 200 OK`` () =
    Assert.Pass()
