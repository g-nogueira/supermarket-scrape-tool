namespace GNogueira.SupermarketScrapeTool.API.DTOs

[<CLIMutable>]
type PagedRequestDto =
    { /// The 1-based index of the page to be returned
      Page: int
      ItemsPerPage: int }
