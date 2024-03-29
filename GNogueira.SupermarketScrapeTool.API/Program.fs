module GNogueira.SupermarketScrapeTool.API.App

open System
open GNogueira.SupermarketScrapeTool.API.Endpoints
open GNogueira.SupermarketScrapeTool.Clients
open GNogueira.SupermarketScrapeTool.Common
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Logging

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose
        [ subRoute
              "/api"
              (choose
                  [ subRoute "/v1" (choose [ PriceEndpoint.v1Endpoints; ProductEndpoint.v1Endpoints ])
                    subRoute "/v2" (choose []) ])
          setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:5000", "https://localhost:5001", "http://127.0.0.1:5173")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler).UseHttpsRedirection())
        .UseCors(configureCors)
        .UseGiraffe(webApp)

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore
    services.AddLogging() |> ignore

    services.AddSingleton<ISecretClient, SecretClient>() |> ignore
    services.AddSingleton<ICosmosDbClient, CosmosDbClient>() |> ignore
    services.AddSingleton<Logging.ILogger>(ConsoleLogger()) |> ignore
    services.AddSingleton<IProductPriceClient, ProductPriceClient>() |> ignore
    services.AddSingleton<IProductClient, ProductClient>() |> ignore


[<EntryPoint>]
let main args =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureLogging(configureLogging)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0
