# Script to fix VARCHAR(-1) to VARCHAR(MAX) in table scripts
# Shows progress

$tablesPath = "D:\Santosh\Work\Projects\WebBased\API\Database_Scripts\Tables"
$files = Get-ChildItem -Path $tablesPath -Filter "*.sql"
$total = $files.Count
$processed = 0

Write-Host "Processing $total files..."
Write-Host ""

foreach ($file in $files) {
    $processed++
    $content = Get-Content $file.FullName -Raw
    
    if ($content -match 'VARCHAR\(-1\)|NVARCHAR\(-1\)') {
        $content = $content -replace 'VARCHAR\(-1\)', 'VARCHAR(MAX)'
        $content = $content -replace 'NVARCHAR\(-1\)', 'NVARCHAR(MAX)'
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "[$processed/$total] Fixed: $($file.Name)"
    } else {
        Write-Host "[$processed/$total] Skipped: $($file.Name) (no VARCHAR(-1) found)"
    }
}

Write-Host ""
Write-Host "Done! Processed $processed files."


