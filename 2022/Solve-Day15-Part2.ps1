
class Range {
   [int]$Lower
   [int]$Upper
}

class Observation {
   [int]$BeaconX
   [int]$BeaconY
   [int]$SensorX
   [int]$SensorY
   [int]$SensorRange
}

$Input = Get-Content "Input-Day15.txt"

$observations = @()
$Input | Foreach-Object {
   # parse each line of input to give pairs of sensor and beacon position
   $obs = New-Object -Type Observation
   $firstSplit = $_ -split "="
   $obs.BeaconY = $firstSplit | Select-Object -Last 1
   $obs.BeaconX = $firstSplit[3] -split "," | Select-Object -First 1
   $obs.SensorX = $firstSplit[1] -split "," | Select-Object -First 1
   $obs.SensorY = $firstSplit[2] -split ":" | Select-Object -First 1
   $obs.SensorRange = [Math]::Abs($obs.SensorX - $obs.BeaconX) + [Math]::Abs($obs.SensorY - $obs.BeaconY)

   Write-Host "S:($($obs.SensorX),$($obs.SensorY)) B:($($obs.BeaconX),$($obs.BeaconY))"
   $observations += $obs
}
Write-Host "----------------"
     
0..4000000 | Foreach-Object {
    $YOfInterest = $_
    $XRanges = @()
    $ObjectXPositions = @()
    $observations | Foreach-Object {
       # Which positions on $YOfInterest overlap with the sensor's beacon-free area
       $vertDistance = [Math]::Abs($_.SensorY - $YOfInterest)
       if ($_.SensorRange -ge $vertDistance) {
           $XRange = New-Object -Type Range
           $XRange.Lower = $_.SensorX - ($_.SensorRange - $vertDistance)
           $XRange.Upper = $_.sensorX + ($_.SensorRange - $vertDistance)
           $XRanges += $XRange
       }
       if ($_.SensorY -eq $YOfInterest) {
           $ObjectXPositions += $_.SensorX
       }
       if ($_.BeaconY -eq $YOfInterest) {
           $ObjectXPositions += $_.BeaconX
       }
    }

    $XRanges = $XRanges | Sort-Object -Property Lower
    #$XRanges | Foreach-Object {
    #   Write-Host "$($_.Lower)..$($_.Upper)"
    #}

    $CurrXRange = $XRanges | Select-Object -First 1
    $MergedXRanges = @()
    $Found = $False
    1..($XRanges.Length-1) | Foreach-Object {
       if ($CurrXRange.Upper -ge $XRanges[$_].Lower) {
           $CurrXRange.Upper = [Math]::Max($CurrXRange.Upper, $XRanges[$_].Upper)
       }
       else {
           $Found = $True
           Write-Host "Gap between $($CurrXRange.Upper) and $($XRanges[$_].Lower) when Y=$YOfInterest"
           $MergedXRange = $CurrXRange
           $CurrXRange = $XRanges[$_]
           $MergedXRanges += $MergedXRange
       }
    }
    $MergedXRanges += $CurrXRange
    
    $FirstXRange = $MergedXRanges | Select-Object -First 1
    $LastXRange = $MergedXRanges | Select-Object -Last 1
    
    if (($FirstXRange.Lower -gt 0) -or ($LastXRange.Upper -lt 4000000)) {
        $MergedXRanges | Foreach-Object {
            Write-Host "Ranges are $($_.Lower)..$($_.Upper) when Y=$YOfInterest"
        }
    }
    if ($Found) {
        Write-Host "But there are objects at:"
        $ObjectXPositions | Write-Host
    }
    
    if ($YOfInterest % 250000 -eq 0) {
        $Timestamp = [DateTime]::UtcNow.ToString("HH:mm:ss")
        Write-Host "Reached Y=$YOfInterest at $Timestamp"
    }
}
