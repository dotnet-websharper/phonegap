set-alias nuget tools/NuGet/NuGet.exe
$vn = "3.0." + 1 * $env:BUILD_NUMBER + "-alpha"
if ($env:NuGetPackageOutputPath) {
  nuget pack -out $env:NuGetPackageOutputPath -version $vn PhoneGap.nuspec
} else {
  nuget pack -out build/ -version $vn PhoneGap.nuspec
}
