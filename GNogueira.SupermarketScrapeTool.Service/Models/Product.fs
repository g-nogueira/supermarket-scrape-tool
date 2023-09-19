module GNogueira.SupermarketScrapeTool.Service.Models

open FSharpPlus
open System
open GNogueira.SupermarketScrapeTool.Service.DTOs

type ProductSource =
    | PingoDoce
    | Continente

    static member toString =
        function
        | PingoDoce -> "Pingo Doce"
        | Continente -> "Continente"

    static member ofString(value: string) =
        match value |> String.toLower with
        | "pingo doce" -> PingoDoce
        | "continente" -> Continente
        | _ -> failwith $"Product Source not valid. Tried to parse {value}."

type PriceUnit =
    | Kg
    | Un
    | Liter
    | Rolls
    | Unknown

    static member toString =
        function
        | Kg -> "Kg"
        | Un -> "Un"
        | Liter -> "Liter"
        | Rolls -> "Rolls"
        | Unknown -> "Unknown"

    static member ofString(value: string) =
        match value |> String.toLower with
        | "kg"
        | "kgm" -> Kg
        | "un" -> Un
        | "ltr" -> Liter
        | "ro" -> Rolls
        | _ -> failwith $"Price Unit not valid. Tried to parse {value}."

type Product =
    { id: Guid
      Date: string
      Name: string
      Price: float
      PriceUnit: PriceUnit
      Source: ProductSource
      Url: Option<string>
      ImageUrl: Option<string> }
    
    static member toDto model =
        { ProductDto.Id = model.id
          Date = model.Date
          Name = model.Name
          Price = model.Price
          PriceUnit = model.PriceUnit |> PriceUnit.toString
          Source = model.Source |> ProductSource.toString
          Url = model.Url |> Option.defaultValue String.Empty
          ImageUrl = model.ImageUrl |> Option.defaultValue String.Empty }
