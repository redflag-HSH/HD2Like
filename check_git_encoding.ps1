# Check each commit's version of RoutineManager.cs for encoding
$commits = @('0036723', 'ccbd98a', '2aeeccd')
$f = 'Assets/00.Scripts/Manager/RoutineManager.cs'

foreach ($c in $commits) {
    Write-Host "=== Commit $c ==="
    $content = git show "${c}:${f}" 2>&1
    if ($content -notmatch "^fatal") {
        # Write raw bytes to temp
        $bytes = [System.Text.Encoding]::GetEncoding(1252).GetBytes($content)
        $outPath = "c:\Users\MS\HD2Like\git_${c}.txt"
        [System.IO.File]::WriteAllText($outPath, $content, [System.Text.Encoding]::UTF8)
        Write-Host "Written to $outPath"
        Write-Host ($content | Select-Object -First 20)
    } else {
        Write-Host "Not found in this commit"
    }
}
