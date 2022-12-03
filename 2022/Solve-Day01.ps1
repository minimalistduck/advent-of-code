$calPerElf = @()
$curElfCal = 0
Get-Content "Input-Day01.txt" | Foreach-Object {
    if ($_) {
        $curElfCal += [int]$_
    }
    else {
        $calPerElf += $curElfCal
        $curElfCal = 0
    }
}

$ordered = $calPerElf | Sort-Object -Descending
Write-Host "Part 1"
$ordered | Select-Object -First 1
Write-Host "Part 2"
$ordered | Select-Object -First 3 | Measure-Object -Sum
