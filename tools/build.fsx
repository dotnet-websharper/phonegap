#if LOCAL
#r "../../websharper.typescript/build/net40/WebSharper.TypeScript.dll"
#else
#r "../packages/WebSharper.TypeScript/tools/net40/WebSharper.TypeScript.dll"
#endif
#I "../packages/NuGet.Core/lib/net40-client"
#r "NuGet.Core"
#r "../packages/IntelliFactory.Core/lib/net45/IntelliFactory.Core.dll"
#r "../packages/IntelliFactory.Build/lib/net45/IntelliFactory.Build.dll"

open System
open System.IO
open IntelliFactory.Build
module C = WebSharper.TypeScript.Compiler

let bt =
    BuildTool().PackageId("WebSharper.PhoneGap").VersionFrom("WebSharper")
        .WithFramework(fun fw -> fw.Net40)
        .WithFSharpVersion(FSharpVersion.FSharp30)

let asmVersion =
    let v = PackageVersion.Full.Find(bt)
    sprintf "%i.%i.0.0" v.Major v.Minor

let local args =
    Path.Combine(__SOURCE_DIRECTORY__, Path.Combine(Array.ofList args))

let dts = local [".."; "node_modules"; "typedphonegap"; "build"; "TypedPhoneGap.d.ts"]
let mjs = local [".."; "node_modules"; "typedphonegap"; "build"; "TypedPhoneGap.min.js"]
let lib = local [".."; "packages"; "WebSharper.TypeScript.Lib"; "lib"; "net40"; "WebSharper.TypeScript.Lib.dll"]
let snk = local [Environment.GetEnvironmentVariable("INTELLIFACTORY"); "keys"; "IntelliFactory.snk"]

let fsCore =
    local [
        Environment.GetEnvironmentVariable("ProgramFiles(x86)"); "Reference Assemblies"
        "Microsoft"; "FSharp"; ".NETFramework"; "v4.0"; "4.3.0.0"; "FSharp.Core.dll"
    ]

let opts =
    {
        C.Options.Create("WebSharper.PhoneGap", [dts]) with
            AssemblyVersion = Some (Version asmVersion)
            EmbeddedResources = [C.EmbeddedResource.FromFile(mjs).AsWebResource("text/javascript")]
            Renaming = WebSharper.TypeScript.Renaming.RemovePrefix "TypedPhoneGap"
            References = [C.ReferenceAssembly.File lib; C.ReferenceAssembly.File fsCore]
            StrongNameKeyFile = Some snk
            Verbosity = C.Level.Verbose
            WebSharperResources = [C.WebSharperResource.Create("TypedPhoneGapResource", "TypedPhoneGap.min.js")]
    }

let result =
    C.Compile opts

for msg in result.Messages do
    printfn "%O" msg

match result.CompiledAssembly with
| None -> ()
| Some asm ->
    let out = local [".."; "build"; "WebSharper.PhoneGap.dll"]
    let dir = DirectoryInfo(Path.GetDirectoryName(out))
    if not dir.Exists then
        dir.Create()
    printfn "Writing %s" out
    File.WriteAllBytes(out, asm.GetBytes())

    bt.Solution [
        bt.NuGet.CreatePackage()
            .Configure(fun c ->
                { c with
                    Authors = ["IntelliFactory"]
                    Title = Some "WebSharper.PhoneGap 3.4.0"
                    LicenseUrl = Some "http://websharper.com/licensing"
                    ProjectUrl = Some "http://websharper.com"
                    Description = "WebSharper bindings to PhoneGap / Cordova 3.4.0"
                    RequiresLicenseAcceptance = true })
            .AddDependency("WebSharper.TypeScript.Lib")
            .AddFile("build/WebSharper.PhoneGap.dll", "lib/net40/WebSharper.PhoneGap.dll")
            .AddFile("README.md", "docs/README.md")
    ]
    |> bt.Dispatch
