namespace PhoneGapDemo

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.JQuery
open IntelliFactory.WebSharper.JQuery.Mobile
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.PhoneGap

[<JavaScript>]
module Client =

    let mobile = Mobile.Instance

    let HeaderDiv cont =
        Div [ HTML5.Attr.Data "role" "header" ] -< cont

    let ContentDiv cont =
        Div [ HTML5.Attr.Data "role" "content" ] -< cont

    let PageDiv id' cont =
        Div [
            HTML5.Attr.Data "role" "page"
            Id id'
        ] -< cont |>! OnAfterRender (fun el ->
            JQuery.Of el.Body |> Mobile.Page.Init
        ) 

    let ListViewUL cont =
        UL [
            HTML5.Attr.Data "role" "listview"
            HTML5.Attr.Data "inset" "true"
        ] -< cont   

    type JQMPage =
        {
            Html   : Element
            Load   : unit -> unit
            Unload : unit -> unit
        }

    let HomePage =
        let link text (page: string) =
            LI [
                A [ HRef ""; Text text ]
                |>! OnClick (fun _ _ -> mobile.ChangePage page)
            ] 
        {
            Html =
                PageDiv "home" [
                    HeaderDiv [ H1 [ Text "PhoneGap API Demo" ] ]
                    ContentDiv [
                        H2 [ Text "Examples:" ]
                        ListViewUL [
                            link "Accelerometer" "#accelerometer"
                            link "Camera"        "#camera"   
                            link "Compass"       "#compass" 
                            link "GPS"           "#gps"
                            link "Contacts"      "#contacts"
                        ]
                    ]
                ] 
            Load   = ignore
            Unload = ignore
        }

    let AccelerometerPage =
        lazy
        let xDiv, yDiv, zDiv = Div [], Div [], Div []
        let accPlugin = DeviceMotion.getPlugin()
        let watchHandle = ref null
        {
            Html =
                let row text value =
                    TR [ 
                        TD [ Div [ Text text ] ]
                        TD [ value ] 
                    ]
                PageDiv "accelerometer" [
                    HeaderDiv [ H1 [ Text "Accelerometer" ] ]
                    ContentDiv [
                        Table [
                            row "X" xDiv
                            row "Y" yDiv
                            row "Z" zDiv
                        ]
                    ]
                ]
            Load = fun () ->
                watchHandle :=
                    accPlugin.watchAcceleration(
                        fun acc ->
                            xDiv.Text <- string acc.x
                            yDiv.Text <- string acc.y
                            zDiv.Text <- string acc.z
                        ,
                        ignore
                    )
            Unload = fun () -> accPlugin.clearWatch(!watchHandle)
        }

    let CameraPage =
        lazy
        {
            Html =
                PageDiv "camera" [
                    HeaderDiv [ H1 [ Text "Camera" ] ]
                    ContentDiv []
                ]
            Load = ignore
            Unload = ignore
        }

    let CompassPage =
        lazy
        {
            Html =
                PageDiv "compass" [
                    HeaderDiv [ H1 [ Text "Compass" ] ]
                    ContentDiv [
                    ]
                ]
            Load = ignore
            Unload = ignore
        }

    let GPSPage =
        lazy
        {
            Html =
                PageDiv "gps" [
                    HeaderDiv [ H1 [ Text "GPS" ] ]
                    ContentDiv []
                ]
            Load = ignore
            Unload = ignore
        }

    let ContactsPage =
        lazy
        {
            Html =
                PageDiv "contacts" [
                    HeaderDiv [ H1 [ Text "Contacts" ] ]
                    ContentDiv []
                ]
            Load = ignore
            Unload = ignore
        }

    let getJQMPage pageRef =
        match pageRef with
        | "#home"          -> Some HomePage
        | "#accelerometer" -> Some AccelerometerPage.Value
        | "#camera"        -> Some CameraPage.Value
        | "#compass"       -> Some CompassPage.Value
        | "#gps"           -> Some GPSPage.Value
        | "#contacts"      -> Some ContactsPage.Value
        | _ -> None
