#load "build/compile.includes.fsx"

open System
open System.IO
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Compiler

let ( +/ ) a b =
    Path.Combine(a, b)

let RootDir = __SOURCE_DIRECTORY__
let BuildDir = RootDir +/ "build"
let Raw = BuildDir +/ "Raw.dll"
let Out = BuildDir +/ "IntelliFactory.WebSharper.PhoneGap.dll"

let EnsureDirectory p =
    let d = DirectoryInfo(p)
    if not d.Exists then
        d.Create()

let BuildFromTypeScript () =
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
        EnsureDirectory BuildDir
        use out = File.Open(Raw, FileMode.Create)
        a.Write(out)
        printfn "Built %s" Raw

let BuildWithWebSharper () =
    let resolver = IntelliFactory.Core.AssemblyResolution.AssemblyResolver.Create()
    let loader = FrontEnd.Loader.Create resolver stdout.WriteLine
    let typeScript =
        typeof<IntelliFactory.TypeScript.Compiler>.Assembly.Location
        |> loader.LoadFile
    let opts =
        {
            FrontEnd.Options.Default with
                References = [ typeScript ]
        }
    let comp = FrontEnd.Prepare opts (fun x -> stdout.WriteLine(string x))
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
        printfn "Compiled %s" Out
    else
        printfn "Failed to compile %s" Out

let Main () =
    BuildFromTypeScript ()
    BuildWithWebSharper ()

Main ()
