$outputFile = "input.py"
"input = [" | Out-File $outputFile -Encoding utf8
Get-Content "Input-Day13.txt" | Where-Object { $_ } | Foreach-Object {
    "$_," | Out-File $outputFile -Append -Encoding utf8
}
"]" | Out-File $outputFile -Append -Encoding utf8
"" | Out-File $outputFile -Append -Encoding utf8
