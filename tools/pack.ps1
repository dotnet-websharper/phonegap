set-alias nuget tools/NuGet/NuGet.exe
$vn = "2.5." + 1 * $env:BUILD_NUMBER
if ($env:NuGetPackageOutputPath) {
  nuget pack -out $env:NuGetPackageOutputPath -version $vn PhoneGap.nuspec
} else {
  nuget pack -out build/ -version $vn PhoneGap.nuspec
}
