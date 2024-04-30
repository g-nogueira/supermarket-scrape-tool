namespace GNogueira.SupermarketScrapeTool.Scrapper.Models

open System
open System.Net.Http
open FSharp.Core
open FsToolkit.ErrorHandling
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack
open GNogueira.SupermarketScrapeTool.Common

module HttpContent =
    let readAsStringAsync (content: HttpContent) = content.ReadAsStringAsync()

module HttpClient =

    type Url =
        | String of string
        | Uri of Uri

        static member toString =
            function
            | String url -> url
            | Uri url -> url.ToString()

    let getAsync url (client: HttpClient) =
        async {
            return!
                match url with
                | String url -> client.GetAsync(url)
                | Uri url -> client.GetAsync(url)
                |> Async.AwaitTask
        }
        |> Async.Catch
        |> Async.map Result.ofChoice
        |> AsyncResult.mapError (fun ex ->
            match ex with
            | :? InvalidOperationException as _ -> $"Invalid URI was provided. Got '{url |> Url.toString}'."
            | _ -> ex.Message)


module HtmlDocument =
    let loadHtml str (doc: HtmlDocument) =
        doc.LoadHtml str

        match box doc with
        | null -> Result.Error "Document is null"
        | _ ->
            match doc.ParseErrors |> Array.ofSeq with
            | [||] -> doc |> Result.Ok
            | _ ->
                doc.ParseErrors
                |> Seq.head
                |> _.Reason
                |> sprintf "The input is not a valid HTML string. %s"
                |> Result.Error

module HtmlNode =
    let invert f a b = f b a

    let tryQuerySelector selector (node: HtmlNode) =
        selector |> node.QuerySelector |> Option.ofObj

    let getAttributeValue (attribute: string) (def: string) (node: HtmlNode) = node.GetAttributeValue(attribute, def)

    let tryGetAttributeValue (attribute: string) (node: HtmlNode) =
        node.GetAttributeValue(attribute, "") |> Option.ofString

    let tryGetAny selectors (node: HtmlNode) =
        selectors |> Seq.tryPick ((invert tryQuerySelector) node)

    let tryGet selector (node: HtmlNode) = node |> tryQuerySelector selector

    let innerText (node: HtmlNode) = node.InnerText
