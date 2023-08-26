module GNogueira.SupermarketScrapeTool.Service.Models

open FSharpPlus
open System

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
      Source: ProductSource }
