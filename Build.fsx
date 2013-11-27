#load "tools/includes.fsx"
open IntelliFactory.Core
open IntelliFactory.Build

let name = "WebSharper.PhoneGap"
let bt = BuildTool().PackageId(name, "0.1")

[<Sealed>]
type P(?ps: Parameters) =
    let ps = defaultArg ps (Parameters.Default())
    let refs =
        let wsPaths =
            [
                "tools/net45/IntelliFactory.Core.dll"
                "tools/net45/IntelliFactory.WebSharper.Core.dll"
                "tools/net45/IntelliFactory.WebSharper.Compiler.dll"
            ]
        [
            bt.Reference.NuGet("WebSharper.TypeScript")
                .At(["lib/net45/IntelliFactory.TypeScript.dll"])
                .Reference()
            bt.Reference.NuGet("WebSharper")
                .At(wsPaths)
                .Reference()
        ]
    let fw = bt.Framework.Net45
    let resolved = bt.ResolveReferences fw refs
    interface IProject with
        member p.Build() = bt.FSharp.ExecuteScript("compile.fsx", refs = resolved)
        member p.Clean() = ()
        member p.PrepareReferences() = ()
        member p.Framework = fw
        member p.Name = name
        member p.Parametric =
            {
                new IParametric<IProject> with
                    member par.WithParameters(ps) = P(ps) :> IProject
                interface IParametric with
                    member par.Parameters = ps
            }
        member p.References = Seq.ofList refs

bt.Solution [
    P()
    bt.NuGet.NuSpec("PhoneGap.nuspec")
]
|> bt.Dispatch
