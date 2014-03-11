namespace PhoneGapDemo

open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets

type Action =
    | Home

module Controls =
    open IntelliFactory.WebSharper.Html
    open IntelliFactory.WebSharper.JQuery
    open IntelliFactory.WebSharper.JQuery.Mobile

    [<Sealed>]
    type EntryPoint() =
        inherit Web.Control()

        [<JavaScript>]
        override __.Body =
            let currentPage = ref None
            Mobile.Events.PageBeforeChange.On(JQuery.Of Dom.Document.Current, fun (e, data) ->
                match data.ToPage with
                | :? string as pageUrl -> 
                    match Client.getJQMPage pageUrl with
                    | Some pageObj ->
                        let body = JQuery.Of "body"                  
                        let toPage =
                            match body.Children pageUrl with
                            | p when p.Length = 0 ->
                                let page = pageObj.Html
                                body.Append page.Body |> ignore
                                (page :> IPagelet).Render()
                                JQuery.Of page.Body
                            | p -> p
                        !currentPage |> Option.iter (fun (p: Client.JQMPage) -> p.Unload())
                        pageObj.Load()
                        currentPage := Some pageObj
                    | None _ -> ()
                | _ -> ()
            )
            upcast Div [] |>! OnAfterRender (fun _ -> Client.mobile.ChangePage "#home")

module Skin =
    open System.Web

    type Page =
        {
            Title : string
            Body : list<Content.HtmlElement>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("body", fun x -> x.Body)

    let WithTemplate title body : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Title = title
                Body = body context
            }

module Site =
    let HomePage =
        Skin.WithTemplate "HomePage" <| fun ctx ->
            [
                Div [new Controls.EntryPoint()]
            ]

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
        ]

[<Sealed>]
type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home]

[<assembly: Website(typeof<Website>)>]
do ()
