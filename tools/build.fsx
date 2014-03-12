#if LOCAL
#r "../../websharper.typescript/build/net40/IntelliFactory.WebSharper.TypeScript.dll"
#else
#r "../packages/WebSharper.TypeScript/tools/net40/IntelliFactory.WebSharper.TypeScript.dll"
#endif

open System
open System.IO
module C = IntelliFactory.WebSharper.TypeScript.Compiler

let local args =
    Path.Combine(__SOURCE_DIRECTORY__, Path.Combine(Array.ofList args))

let dts = local [".."; "node_modules"; "typedphonegap"; "build"; "TypedPhoneGap.d.ts"]
let mjs = local [".."; "node_modules"; "typedphonegap"; "build"; "TypedPhoneGap.min.js"]
let lib = local [".."; "packages"; "WebSharper.TypeScript.Lib"; "lib"; "net40"; "IntelliFactory.WebSharper.TypeScript.Lib.dll"]
let snk = local [Environment.GetEnvironmentVariable("INTELLIFACTORY"); "keys"; "IntelliFactory.snk"]

let opts =
    {
        C.Options.Create("IntelliFactory.WebSharper.PhoneGap", [dts]) with
            AssemblyVersion = Some (Version "2.5.0.0")
            EmbeddedResources = [C.EmbeddedResource.FromFile(mjs)]
            Renaming = IntelliFactory.WebSharper.TypeScript.Renaming.RemovePrefix "TypedPhoneGap"
            References = [C.ReferenceAssembly.File lib] 
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
    let out = local [".."; "build"; "IntelliFactory.WebSharper.PhoneGap.dll"]
    let dir = DirectoryInfo(Path.GetDirectoryName(out))
    if not dir.Exists then
        dir.Create()
    printfn "Writing %s" out
    File.WriteAllBytes(out, asm.GetBytes())
