namespace GNogueira.SupermarketScrapeTool.Common

module FSharp =
    let (|EmptySeq|_|) a = if Seq.isEmpty a then Some () else None


