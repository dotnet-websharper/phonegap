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

    let row text value =
        TR [ 
            TD [ Div [ Text text ] ]
            TD [ value ] 
        ]

    let AccelerometerPage =
        lazy
        let xDiv, yDiv, zDiv = Div [], Div [], Div []
        try
            let plugin = PhoneGap.DeviceMotion.getPlugin()
            let watchHandle = ref null
            {
                Html =
                    PageDiv "accelerometer" [
                        HeaderDiv [ H1 [ Text "Accelerometer" ] ]
                        ContentDiv [
                            Table [
                                row "X: " xDiv
                                row "Y: " yDiv
                                row "Z: " zDiv
                            ]
                        ]
                    ]
                Load = fun () ->
                    watchHandle :=
                        plugin.watchAcceleration(
                            fun acc ->
                                xDiv.Text <- string acc.x
                                yDiv.Text <- string acc.y
                                zDiv.Text <- string acc.z
                            ,
                            ignore
                        )
                Unload = fun () -> plugin.clearWatch(!watchHandle)
            }
        with e ->
            {
                Html =
                    PageDiv "accelerometer" [
                        HeaderDiv [ H1 [ Text "Accelerometer" ] ]
                        ContentDiv [ Text "Accelerometer not enabled" ]
                    ]
                Load = ignore
                Unload = ignore
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
        let headingDiv = Div []
        try
            let plugin = DeviceOrientation.getPlugin()
            let watchHandle = ref null
            {
                Html =
                    PageDiv "compass" [
                        HeaderDiv [ H1 [ Text "Compass" ] ]
                        ContentDiv [
                            Div [ Text "Heading:" ]
                            headingDiv
                        ]
                    ]
                Load = fun () ->
                    watchHandle :=
                        plugin.watchHeading(
                            fun ori ->
                                headingDiv.Text <- string ori.magneticHeading
                            ,
                            ignore
                        )
                Unload = fun () -> plugin.clearWatch(!watchHandle)
            }
        with _ ->
            {
                Html =
                    PageDiv "compass" [
                        HeaderDiv [ H1 [ Text "Compass" ] ]
                        ContentDiv [
                            Div [ Text "Compass Not Enabled" ]
                        ]
                    ]
                Load = ignore
                Unload = ignore
            }
            

    let GPSPage =
        lazy
        let latDiv, lngDiv, altDiv = Div[], Div[], Div[] 
        try
            let plugin = Geolocation.getPlugin()
            let watchHandle = ref null
            {
                Html =
                    PageDiv "gps" [
                        HeaderDiv [ H1 [ Text "GPS" ] ]
                        ContentDiv [
                            Table [
                                row "Latitude: " latDiv
                                row "Longitude: " lngDiv
                                row "Altitude: " altDiv
                            ]
                        ]
                    ]
                Load = fun () ->
                    watchHandle :=
                        plugin.watchPosition(
                            fun pos ->
                                latDiv.Text <- string pos.coords.latitude
                                lngDiv.Text <- string pos.coords.longitude
                                altDiv.Text <- string pos.coords.altitude
                            , 
                            ignore
                        )
                Unload = fun () -> plugin.clearWatch(!watchHandle)
            }
        with e ->
            {
                Html =
                    PageDiv "gps" [
                        HeaderDiv [ H1 [ Text "GPS" ] ]
                        ContentDiv [
                            Text "Geolocation plugin not enabled"
                        ]
                    ]
                Load = ignore
                Unload = ignore
            }

    let ContactsPage =
        lazy
        try
            let plugin = Contacts.getPlugin()
            {
                Html =
                    PageDiv "contacts" [
                        HeaderDiv [ H1 [ Text "Contacts" ] ]
                        ContentDiv []
                    ]
                Load = ignore
                Unload = ignore
            }
        with e ->
            {
                Html =
                    PageDiv "contacts" [
                        HeaderDiv [ H1 [ Text "Contacts" ] ]
                        ContentDiv [ Text "Contacts plugin not enabled" ]
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
