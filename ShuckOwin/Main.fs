namespace ShuckOwin

open IntelliFactory.WebSharper.Html.Server
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets

type Action =
    | Home
    | About

module Controls =

    [<Sealed>]
    type EntryPoint() =
        inherit Web.Control()

        [<JavaScript>]
        override __.Body =
            Client.Main() :> _

module Skin =
    open System.Web

    type Page =
        {
            Title : string
            Body : list<Element>
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

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Links (ctx: Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Home]
            LI ["About" => ctx.Link About]
        ]

    let HomePage =
        Skin.WithTemplate "HomePage" <| fun ctx ->
            [
                Div [Text "HOME"]
                Div [new Controls.EntryPoint()]
                Links ctx
            ]

    let AboutPage =
        Skin.WithTemplate "AboutPage" <| fun ctx ->
            [
                Div [Text "ABOUT"]
                Links ctx
            ]

    let MainSitelet =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
            Sitelet.Content "/About" About AboutPage
        ]

module SelfHostedServer =

    open global.Owin
    open Microsoft.Owin.Hosting
    open Microsoft.Owin.StaticFiles
    open Microsoft.Owin.FileSystems
    open IntelliFactory.WebSharper.Owin

    [<EntryPoint>]
    let Main = function
        | [| rootDirectory; url |] ->
            use server = WebApp.Start(url, fun appB ->
                appB.UseStaticFiles(
                        StaticFileOptions(
                            FileSystem = PhysicalFileSystem(rootDirectory)))
                    .UseSitelet(rootDirectory, Site.MainSitelet)
                |> ignore)
            stdout.WriteLine("Serving {0}", url)
            stdin.ReadLine() |> ignore
            0
        | _ ->
            eprintfn "Usage: ShuckOwin ROOT_DIRECTORY URL"
            1
