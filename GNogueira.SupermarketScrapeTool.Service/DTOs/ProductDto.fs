﻿namespace GNogueira.SupermarketScrapeTool.Service.DTOs

open System

type ProductDto =
    { Id: Guid
      Date: string
      Name: string
      Price: float
      PriceUnit: string
      Source: string
      Url: string
      ImageUrl: string }