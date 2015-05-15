set-alias nuget tools/NuGet/NuGet.exe
set-alias npm "C:\Program Files\nodejs\npm.cmd"
function nuget-inst-pre($pkg) {
  nuget install -NoCache -PreRelease -ExcludeVersion -o packages $pkg
}

nuget-inst-pre IntelliFactory.Build
nuget-inst-pre WebSharper.TypeScript
nuget-inst-pre WebSharper.TypeScript.Lib
npm install typedphonegap
