namespace GNogueira.SupermarketScrapeTool.API.DTOs

open System

[<CLIMutable>]
type PagedRequestDto =
    { /// The 1-based index of the page to be returned
      Page: int
      ItemsPerPage: int }

[<CLIMutable>]
type SearchProductRequestDto =
    { Page: int
      ItemsPerPage: int
      Title: Option<string>
      Supermarket: Option<string>
      CreatedAfter: Option<DateTime>
      CreatedBefore: Option<DateTime>
      CreatedAt: Option<DateTime>
      Sorting: Option<string> }
