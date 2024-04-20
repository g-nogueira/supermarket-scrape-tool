namespace GNogueira.SupermarketScrapeTool.Common

open System
open System.Linq
open System.Linq.Expressions
open Microsoft.Azure.Cosmos.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter
open FSharp.Core

module Queryable =
    type Sorting =
    | Asc
    | Desc
    with
        static member ofString (s: string) =
            match s.ToLower() with
            | "asc" -> Ok Asc
            | "desc" -> Ok Desc
            | invalid -> Error $"Invalid sorting type. Expected 'asc' or 'desc', received: '{invalid}'."


module CosmosDB =
    open Queryable
    let private toLinq (expr : Expr<'a -> 'b>) =
        let linq = expr |> QuotationToExpression
        let call = linq :?> MethodCallExpression
        let lambda = call.Arguments.[0] :?> LambdaExpression
        Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters)

    let toFeedIterator (v: IQueryable<_>) = v.ToFeedIterator()
    let skip v (q:IQueryable<_>) = q.Skip v
    let take (value: 'T) (q:IQueryable<_>) =
        match box value with
        | :? int as v -> q.Take v
        | :? Range as v -> q.Take v
        | _ -> q
    let sort sortBy (keySelector: Expr<'a -> 'b>) (query: IQueryable<_>)=
        let exp = toLinq keySelector
        sortBy
        |> Option.map
                (function
                | Asc -> query.OrderBy exp
                | Desc -> query.OrderByDescending exp)
        |> Option.defaultValue (query.OrderByDescending exp)


