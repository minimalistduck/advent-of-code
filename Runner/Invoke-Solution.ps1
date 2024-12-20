$year = "2024"
$day = "17";

$ideCodingPath = Join-Path $PSScriptRoot "..\\$year"
$sourceFile = Join-Path $ideCodingPath "Day$($day).cs"
Copy-Item $sourceFile $PSScriptRoot
#$SourceFile = Join-Path $IdeCodingPath "Day$($Day)-Old.cs"
#Copy-Item $SourceFile $PSScriptRoot

Push-Location $PSScriptRoot
dotnet run RunMe.csproj -- $ideCodingPath
Pop-Location
