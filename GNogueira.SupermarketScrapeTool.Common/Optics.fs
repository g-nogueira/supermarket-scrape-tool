namespace GNogueira.SupermarketScrapeTool.Common


module Optics =
    type Lens<'a, 'b> = { Get: 'a -> 'b; Set: 'b -> 'a -> 'a }
