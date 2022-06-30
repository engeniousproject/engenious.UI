$found=Find-Package -Name "engenious.ContentTool" -ProviderName NuGet -AllowPrereleaseVersions -AllVersions  | Select-Object -first 1

Install-Package $found

$package=Get-Package -Name $found.Name -RequiredVersion $found.Version

$dir=Split-Path -Parent $package.Source
dotnet $dir/tools/engenious.ContentTool.dll /@:"./Content.ecp" /configuration:Release /outputDir:"." /intermediateDir:"./obj" /hidden:true


cp ./bin/Release/* .