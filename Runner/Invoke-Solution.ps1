$year = "2024"
$day = "15";

$ideCodingPath = Join-Path $PSScriptRoot "..\\$year"
$sourceFile = Join-Path $ideCodingPath "Day$($day).cs"
Copy-Item $sourceFile $PSScriptRoot
#$SourceFile = Join-Path $IdeCodingPath "Day$($Day)-Old.cs"
#Copy-Item $SourceFile $PSScriptRoot

dotnet run RunMe.csproj
