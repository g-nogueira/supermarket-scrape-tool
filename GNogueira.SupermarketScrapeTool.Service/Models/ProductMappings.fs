module GNogueira.SupermarketScrapeTool.Service.Models

open FSharpPlus
open System
open GNogueira.SupermarketScrapeTool.Models
open GNogueira.SupermarketScrapeTool.Service.Models

type ProductSource with

    static member toString =
        function
        | ProductSource.PingoDoce -> "pingodoce"
        | ProductSource.Continente -> "continente"

    static member ofString(value: string) =
        String.toLower
        >> function
            | "pingodoce" -> PingoDoce |> Ok
            | "continente" -> Continente |> Ok
            | _ -> $"Product Source not valid. Tried to parse {value}." |> exn |> Error

type PriceUnit with

    static member toString =
        function
        | Kg -> "kg"
        | Un -> "un"
        | Liter -> "liter"
        | Rolls -> "rolls"
        | Unknown -> "unknown"

    static member ofString(value: string) =
        match value |> String.toLower with
        | "kg"
        | "kgm" -> Kg |> Ok
        | "un" -> Un |> Ok
        | "ltr" -> Liter |> Ok
        | "ro" -> Rolls |> Ok
        | _ -> $"Price Unit not valid. Tried to parse {value}." |> exn |> Error

type ProductPrice with

    static member toDto(model: ProductPrice) =
        { ProductPriceDto.Date = model.Date
          Price = model.Price
          PriceUnit = model.PriceUnit |> PriceUnit.toString }

type Product with

    static member toDto(model: Product) =
        { ProductDto.id = model.Id |> ProductId.deconstruct
          ExternalId = model.ExternalId
          Date = model.Date
          Name = model.Name
          Prices = model.Prices |> Seq.map ProductPrice.toDto
          Source = model.Source |> ProductSource.toString
          Url = model.Url |> Option.defaultValue String.Empty
          ImageUrl = model.ImageUrl |> Option.defaultValue String.Empty }
