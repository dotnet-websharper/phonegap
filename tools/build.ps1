$pf = [environment]::GetEnvironmentVariable("ProgramFiles(x86)")
set-alias fsi "$pf/Microsoft SDKs/F#/3.1/Framework/v4.0/Fsi.exe"
if (test-path "../websharper.typescript/build/net40/IntelliFactory.WebSharper.TypeScript.dll") {
  fsi --define:LOCAL --exec tools/build.fsx
} else {
  fsi --exec tools/build.fsx
}
