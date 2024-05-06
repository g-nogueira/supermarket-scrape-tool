module GNogueira.SupermarketScrapeTool.API.App

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore
open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common
open GNogueira.SupermarketScrapeTool.API.Endpoints

let mapV1Endpoints (app: RouteGroupBuilder) =
    app
        .MapGet("/products/all", Func<IProductClient, Task<IResult>> getProductListHandler)
        .WithName("GetProducts")
        .WithDescription("Get all products")

        .WithOpenApi()
    |> ignore

    app
        .MapGet("/products", Func<int, int, IProductClient, Task<IResult>> getProductListPaginatedHandler)
        .WithName("GetProductsPaginated")
        .WithDescription("Get all products paginated")
        .WithOpenApi()
    |> ignore

    app
        .MapGet("/products/{id}", Func<string, IProductClient, Task<IResult>> getProductByIdHandler)
        .WithName("GetProductById")
        .WithDescription("Get a product by id")
        .Accepts<string>("id", "The product id")
        .WithOpenApi()
    |> ignore

    app

let configureServices (builder: WebApplicationBuilder) =
    let configureLogging (builder: ILoggingBuilder) =
        builder.AddConsole().AddDebug() |> ignore

    let services = builder.Services

    let swaggerGenOptions (options: SwaggerGen.SwaggerGenOptions) =
        let openApiInfo =
            OpenApiInfo(
                Version = "v1",
                Title = "Supermarket Scrape Tool API",
                Description = "An API for compating supermarket products",
                Contact = OpenApiContact(Name = "Gustavo Nogueira", Url = Uri("https://github.com/g-nogueira/supermarket-scrape-tool/issues"))
            // TODO: Add license
            // License = OpenApiLicense(
            //     Name = "Example License",
            //     Url = Uri("https://example.com/license")
            // )
            )

        options.SwaggerDoc("v1", openApiInfo)

    services
        .AddCors()
        .AddLogging(configureLogging)
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(swaggerGenOptions)

        .AddSingleton<ISecretClient, SecretClient>()
        .AddSingleton<ICosmosDbClient, CosmosDbClient>()
        .AddSingleton<ILogger>(ConsoleLogger())
        .AddSingleton<IProductClient, ProductClient>()
    |> ignore

    builder

[<EntryPoint>]
let main args =
    let buildApp (builder: WebApplicationBuilder) = builder.Build()

    let useSwagger (app: IApplicationBuilder) = app.UseSwagger()

    let useSwaggerUI (app: IApplicationBuilder) = app.UseSwaggerUI()

    let useDeveloperExceptionPage (app: IApplicationBuilder) =
        app.UseDeveloperExceptionPage() |> ignore

    let onDevEnv f (app: WebApplication) =
        if Env.isDevEnv then f app |> ignore

        app

    let configureApi (app: WebApplication) =
        app.MapGroup("api").WithTags("Root") |> mapV1Endpoints |> ignore

        app

    let run (app: WebApplication) = app.Run()

    WebApplication.CreateBuilder(args)
    |> configureServices
    |> buildApp
    |> onDevEnv (
        useSwagger
        >> useSwaggerUI
        >> useDeveloperExceptionPage
        >> ignore)
    |> configureApi
    |> run

    0
