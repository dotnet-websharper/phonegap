#r "packages/IntelliFactory.Build.0.0.6/lib/net40/IntelliFactory.Build.dll"
#r "packages/FAKE.2.1.158-alpha/tools/FakeLib.dll"
#r "packages/IntelliFactory.TypeScript.0.0.3-alpha/lib/net40/IntelliFactory.TypeScript.dll"
#r "packages/IntelliFactory.Parsec.0.0.1-alpha/lib/net40/IntelliFactory.Parsec.dll"
#r "packages/WebSharper.2.5.5-alpha/tools/net40/IntelliFactory.WebSharper.Core.dll"
#r "packages/WebSharper.2.5.5-alpha/tools/net40/IntelliFactory.WebSharper.Compiler.dll"

open System
open System.IO
open Fake
module WFE = IntelliFactory.WebSharper.Compiler.FrontEnd
module X = IntelliFactory.Build.XmlGenerator

let ( +/ ) a b = Path.Combine(a, b)
let RootDir = __SOURCE_DIRECTORY__
let DotBuildDir = RootDir +/ ".build"
let T x f = Target x f; x

let Raw = DotBuildDir +/ "Raw.dll"
let Out = DotBuildDir +/ "IntelliFactory.WebSharper.PhoneGap.dll"

module Config =
    let Company = "IntelliFactory"
    let Description = "Bindings to TypedPhoneGap"
    let LicenseUrl = "http://websharper.com/licensing"
    let PackageId = "IntelliFactory.WebSharper.PhoneGap"
    let Tags = ["WebSharper"; "TypeScript"; "PhoneGap"; "F#"]
    let AssemblyVersion = Version "0.0.0.0"
    let AssemblyFileVersion = Version "0.0.2.0"
    let Version = "0.0.2-alpha"
    let Website = "http://bitbucket.org/IntelliFactory/websharper.phonegap"

let BuildFromTypeScript = T "BuildFromTypeScript" <| fun () ->
    let comp = IntelliFactory.TypeScript.Compiler()
    comp.Log <-
        {
            new IntelliFactory.TypeScript.Logging.Log() with
                override this.Send(p, msg) =
                    stdout.WriteLine(msg)
        }
    comp.Namespace <- "IntelliFactory.WebSharper"
    comp.TypeName <- "PhoneGap"
    let source = RootDir +/ "PhoneGap.d.ts"
    let result = comp.Compile(source)
    match result with
    | None -> failwithf "Failed to compile from TypeScript"
    | Some a ->
        ensureDirectory DotBuildDir
        use out = File.Open(Raw, FileMode.Create)
        a.Write(out)
        tracefn "Built %s" Raw

let BuildWithWebSharper = T "BuildWithWebSharper" <| fun () ->
    let loader = WFE.Loader.Create (Set [RootDir]) stdout.WriteLine
    let typeScript =
        loader.LoadFile (
            RootDir +/
            "packages" +/
            "IntelliFactory.TypeScript.0.0.3-alpha" +/
            "lib" +/
            "net40" +/ 
            "IntelliFactory.TypeScript.dll"
        )
    let opts =
        {
            WFE.Options.Default with
                References = [ typeScript ]
        }
    let comp = WFE.Prepare opts (fun x -> stdout.WriteLine(string x))
    let ra = loader.LoadFile Raw
    if comp.CompileAndModify ra then
        let key =
            let ih = Environment.GetEnvironmentVariable("INTELLIFACTORY")
            if ih <> null then
                let p = ih +/ "keys" +/ "IntelliFactory.snk"
                if File.Exists(p) then
                    Reflection.StrongNameKeyPair(File.ReadAllBytes p)
                    |> Some
                else None
            else None
        ra.Write key Out
        tracefn "Compiled %s" Out
    else
        tracefn "Failed to compile %s" Out

//let result = comp.Compile(__SOURCE_DIRECTORY__ +/ "PhoneGap.d.ts")
//match result with
//| None -> tracefn "Problem.."
//| Some a ->
//    use out = File.Open(__SOURCE_DIRECTORY__ +/ "Raw.dll", FileMode.Create)
//    a.Write(out)
//    tracefn "OK"
//
//    ()


//module B = IntelliFactory.Build.CommonBuildSetup
//

// let Build

//
//let Metadata =
//    let m = B.Metadata.Create()
//    m.Author <- Some Config.Company
//    m.AssemblyVersion <- Some Config.AssemblyVersion
//    m.FileVersion <- Some Config.AssemblyFileVersion
//    m.Description <- Some Config.Description
//    m.Product <- Some Config.PackageId
//    m.Website <- Some Config.Website
//    m
//
//let Frameworks = [B.Net40]
//
//let Solution =
//    B.Solution.Standard __SOURCE_DIRECTORY__ Metadata [
//        B.Project.FSharp "IntelliFactory.TypeScript" Frameworks
//        B.Project.FSharp "IntelliFactory.TypeScript.TypeProvider" Frameworks
//    ]
//
//let BuildMain = T "BuildMain" Solution.Build
//let Build = T "Build" ignore
//let Clean = T "Clean" Solution.Clean

let BuildNuSpecXml (name: string) (deps: seq<string * string>) =
    let e n = X.Element.Create n
    let ( -- ) (a: X.Element) (b: string) = X.Element.WithText b a
    e "package" - [
        e "metadata" - [
            e "id" -- name
            e "version" -- Config.Version
            e "authors"-- Config.Company
            e "owners"-- Config.Company
            e "licenseUrl" -- Config.LicenseUrl
            e "projectUrl"-- Config.Website
            e "requireLicenseAcceptance" -- "true"
            e "description" -- Config.Description
            e "copyright" -- sprintf "Copyright (c) %O %s" DateTime.Now.Year Config.Company
            e "tags" -- String.concat " " Config.Tags
            e "dependencies" - [
                e "group" - [
                    for (k, v) in deps ->
                        e "dependency" + ["id", k; "version", v]
                ]
            ]
        ]
        e "files" - [
            e "file" + [
                "src", DotBuildDir +/ "IntelliFactory.WebSharper.PhoneGap.dll"
                "target", "lib" +/ "net40"
            ]
        ]
    ]

let BuildNuGet =
    T "BuildNuGet" <| fun () ->
        ensureDirectory DotBuildDir
        let nuSpec name = DotBuildDir +/ sprintf "%s.nuspec" name
        let nuGetExe = RootDir +/ ".nuget" +/ "NuGet.exe"
        let spec = nuSpec Config.PackageId
        X.WriteFile spec (BuildNuSpecXml Config.PackageId [])
        spec
        |> NuGetPack (fun p ->
            { p with
                OutputPath = DotBuildDir
                ToolPath = nuGetExe
                Version = Config.Version
                WorkingDir = DotBuildDir
            })

let Build = T "Build" ignore
BuildFromTypeScript ==> BuildWithWebSharper ==> BuildNuGet ==> Build
RunTargetOrDefault Build
