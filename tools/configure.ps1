set-alias nuget tools/NuGet/NuGet.exe

function nuget-inst-pre($pkg) {
  nuget install -NoCache -PreRelease -ExcludeVersion -o packages $pkg
}

nuget-inst-pre WebSharper.TypeScript
nuget-inst-pre WebSharper.TypeScript.Lib
npm install typedphonegap
