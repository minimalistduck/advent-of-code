
$YOfInterest = 2000000
$XRanges = @()
$ObjectXPositions = @()

class Range {
   [int]$Lower
   [int]$Upper
}

Get-Content "Input-Day15.txt" | Foreach-Object {
   # parse each line of input to give pairs of sensor and beacon position
   $firstSplit = $_ -split "="
   [int]$beaconY = $firstSplit | Select-Object -Last 1
   [int]$beaconX = $firstSplit[3] -split "," | Select-Object -First 1
   [int]$sensorX = $firstSplit[1] -split "," | Select-Object -First 1
   [int]$sensorY = $firstSplit[2] -split ":" | Select-Object -First 1
   # Write-Host "S:($sensorX,$sensorY) B:($beaconX,$beaconY)"
   
   # Which positions on $YOfInterest overlap with the sensor's beacon-free area
   # 1. How much of the distance is required to reach $YOfInterest in the vertical direction?
   $vertDistance = [Math]::Abs($sensorY - $YOfInterest)
   $sensorRange = [Math]::Abs($sensorX - $beaconX) + [Math]::Abs($sensorY - $beaconY)
   if ($sensorRange -ge $vertDistance) {
       $XRange = New-Object -Type Range
       $XRange.Lower = $sensorX - ($sensorRange - $vertDistance)
       $XRange.Upper = $sensorX + ($sensorRange - $vertDistance)
       $XRanges += $XRange
   }
   if ($sensorY -eq $YOfInterest) {
       $ObjectXPositions += $sensorX
   }
   if ($beaconY -eq $YOfInterest) {
       $ObjectXPositions += $beaconX
   }
}

$XRanges = $XRanges | Sort-Object -Property Lower
$CurrXRange = $XRanges | Select-Object -First 1
$MergedXRanges = @()
1..($XRanges.Length-1) | Foreach-Object {
   if ($CurrXRange.Upper -ge $XRanges[$_].Lower) {
       $CurrXRange.Upper = [Math]::Max($CurrXRange.Upper, $XRanges[$_].Upper)
   }
   else {
       $MergedXRange = $CurrXRange
       $CurrXRange = $XRanges[$_]
       $MergedXRanges += $MergedXRange
   }
}
$MergedXRanges += $CurrXRange

$MergedXRanges | Foreach-Object {
    Write-Host "$($_.Lower)..$($_.Upper)"
}

$MergedXRanges[0].Upper - $MergedXRanges[0].Lower + 1 | Write-Host

$ObjectXPositions | Write-Host