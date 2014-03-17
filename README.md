# WebSharper.PhoneGap

[PhoneGap][pg] is a framework for creating mobile apps using
HTML5/JavaScript/CSS.  It provides a standardized API for accessing
mobile device features, and build tools for creating native packages.
WebSharper.PhoneGap brings [WebSharper][ws] integration for PhoneGap,
allowing you write [F#][fsharp] apps that work without modification on
iOS, Android, Windows Phone, and many other platforms.  Current
version supports PhoneGap 3.4.0 API.

## Usage

Minimal example (requires `WebSharper.PhoneGap` [NuGet][ng] package):

```fsharp
open IntelliFactory.WebSharper.PhoneGap

let Program =
    JavaScript.Log("==> starting Program")
    Events.deviceReady.add <| fun () ->
        JavaScript.Log("==> deviceReady")
```

For a complete example WebSharper.PhoneGap project, see
[PhoneGapDemo](https://github.com/intellifactory/websharper.samples/tree/master/PhoneGapDemo).

The `IntelliFactory.WebSharper.PhoneGap` namespace also contains
modules corresponding to various PhoneGap plugins, such as Camera,
Contacts, DeviceMotion, and Geolocation.

Bindings are currently generated via the [TypedPhoneGap][tpg] project.
See its sources for a definitive API reference.

## Quick links

* [License](http://websharper.com/licensing)
* [TypedPhoneGap sources at GitHub](https://github.com/intellifactory/TypedPhoneGap)
* [Mercurial sources at Bitbucket](https://bitbucket.org/IntelliFactory/typedphonegap)
* [Issue tracker](https://bitbucket.org/IntelliFactory/websharper.phonegap/issues)

[fsharp]: http://fsharp.org
[ng]: http://www.nuget.org/packages/WebSharper.PhoneGap/
[pg]: http://phonegap.com/
[tpg]: https://github.com/intellifactory/TypedPhoneGap
[ws]: http://websharper.com/
