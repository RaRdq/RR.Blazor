#!/usr/bin/env pwsh
# Quick utility counter for RR.Blazor

param(
    [string]$CssPath = "wwwroot/css/main.css"
)

$cssContent = Get-Content $CssPath -Raw -Encoding UTF8

# Count all class selectors (starting with .)
# Updated pattern to capture classes correctly
$allClasses = [regex]::Matches($cssContent, '\.([a-zA-Z0-9_-]+(?:\\\\?[:@]?[a-zA-Z0-9_-]+)*)\s*[{,:]', [System.Text.RegularExpressions.RegexOptions]::Multiline)

# Filter utility classes (exclude BEM and component classes)
$utilityClasses = @()
foreach ($match in $allClasses) {
    $className = $match.Groups[1].Value
    # Clean up escaped characters
    $className = $className -replace '\\:', ':'
    $className = $className -replace '\\\\', ''
    
    # Skip if it's a component class (starts with capital), BEM element, or known component prefix
    if ($className -notmatch '^[A-Z]' -and 
        $className -notmatch '__' -and 
        $className -notmatch '^(nav|modal|sidebar|user|button|card|alert|badge|layout|fade|animate)-' -and
        $className -match '^[a-zA-Z]') {  # Must start with a letter
        $utilityClasses += $className
    }
}

$uniqueUtilities = $utilityClasses | Sort-Object -Unique

Write-Host "Total CSS classes found: $($allClasses.Count)"
Write-Host "Utility classes found: $($uniqueUtilities.Count)"
Write-Host ""
Write-Host "Sample utilities:"
$uniqueUtilities | Select-Object -First 20 | ForEach-Object { Write-Host "  - $_" }

# Group by prefix
$grouped = $uniqueUtilities | Group-Object { ($_ -split '-')[0] } | Sort-Object Count -Descending

Write-Host ""
Write-Host "Utilities by prefix:"
$grouped | Select-Object -First 15 | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count)"
}

return $uniqueUtilities.Count