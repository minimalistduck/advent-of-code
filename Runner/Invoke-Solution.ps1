$year = "2025"
$day = "06";

$ideCodingPath = Join-Path $PSScriptRoot "..\\$year"
$sourceFile = Join-Path $ideCodingPath "Day$($day).cs"
Copy-Item $sourceFile $PSScriptRoot
#$SourceFile = Join-Path $IdeCodingPath "Day$($Day)-Old.cs"
#Copy-Item $SourceFile $PSScriptRoot

Push-Location $PSScriptRoot
$inputFilePath = Join-Path $ideCodingPath "Day$($day)-input.txt"
dotnet run RunMe.csproj -- $inputFilePath
Pop-Location
