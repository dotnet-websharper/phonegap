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

    let PageWithBackBtnDiv id' cont =
        PageDiv id' [ HTML5.Attr.Data "add-back-btn" "true" ] -< cont

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

    let changePage (page: string) =
        mobile.ChangePage(page, ChangePageConfig(Transition = "slide"))

    let HomePage =
        let link text (page: string) =
            LI [
                A [ HRef ""; Text text ]
                |>! OnClick (fun _ _ -> changePage page)
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
        let plugin = PhoneGap.DeviceMotion.getPlugin()
        let watchHandle = ref null
        {
            Html =
                PageWithBackBtnDiv "accelerometer" [
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
                    ,   ignore
                    )
            Unload = fun () ->
                plugin.clearWatch(!watchHandle)
        }

    let CameraPage =
        lazy
        let img = Img []
        let plugin = Camera.getPlugin()
        let popoverHandle = ref null
        {
            Html =
                PageWithBackBtnDiv "camera" [
                    HeaderDiv [ H1 [ Text "Camera" ] ]
                    ContentDiv [
                        Button [ Text "Get picture" ] |>! OnClick (fun _ _ ->
                            plugin.getPicture(
                                fun pic ->
                                    img.SetAttribute("src", "data:image/jpeg;base64," + pic)
                            ,   ignore
                            ) |> ignore
                        )
                        img
                    ]
                ]
            Load   = ignore
            Unload = ignore
        }

    let CompassPage =
        lazy
        let headingDiv = Div []
        let plugin = DeviceOrientation.getPlugin()
        let watchHandle = ref null
        {
            Html =
                PageWithBackBtnDiv "compass" [
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
                    ,   ignore
                    )
            Unload = fun () ->
                plugin.clearWatch(!watchHandle)
        }

    let GPSPage =
        lazy
        let latDiv, lngDiv, altDiv = Div[], Div[], Div[] 
        let plugin = Geolocation.getPlugin()
        let watchHandle = ref null
        {
            Html =
                PageWithBackBtnDiv "gps" [
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
                    ,   ignore
                    )
            Unload = fun () ->
                plugin.clearWatch(!watchHandle)
        }

    let ContactsPage =
        lazy
        let contactsUL = ListViewUL []
        let plugin = Contacts.getPlugin()
        {
            Html =
                PageWithBackBtnDiv "contacts" [
                    HeaderDiv [ H1 [ Text "Contacts" ] ]
                    ContentDiv [
                        contactsUL
                    ]
                ]
            Load = fun () ->
                contactsUL.Clear()
                plugin.find(
                    [| "displayName" |] 
                ,   fun cts ->
                        for c in cts do
                            LI [ Text (As<Contacts.Properties> c).displayName ]
                            |> contactsUL.Append
                        JQuery.Of contactsUL.Body |> Mobile.ListView.Refresh
                ,   ignore
                ,   Contacts.FindOptions(multiple = true)
                )
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
