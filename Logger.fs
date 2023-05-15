[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module GNogueira.SupermarketScrapeTool.Service.Logger

open System


let message text=
    printfn $"{text}"
    
let exc (exceptionObj : Exception) =
    printfn $"{exceptionObj.Message}"