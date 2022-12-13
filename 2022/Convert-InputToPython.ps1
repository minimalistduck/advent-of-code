$outputFile = "input.py"
"input = [" | Out-File $outputFile
Get-Content "Input-Day13.txt" | Where-Object { $_ } | Foreach-Object {
    "$_," | Out-File $outputFile -Append
}
"]" | Out-File $outputFile -Append
