#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Analyzes missing CSS classes from validation report to identify most common violations

.DESCRIPTION
    Reads the class-validation-report.json and provides statistics on the most frequently
    missing CSS classes to help prioritize fixes in RR.Blazor styles.

.PARAMETER ReportPath
    Path to the validation report JSON file (default: "./class-validation-report.json")

.PARAMETER TopCount
    Number of top missing classes to show (default: 30)

.EXAMPLE
    .\AnalyzeMissingClasses.ps1
    
.EXAMPLE
    .\AnalyzeMissingClasses.ps1 -ReportPath "custom-report.json" -TopCount 50
#>

param(
    [string]$ReportPath = "./class-validation-report.json",
    [int]$TopCount = 30
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Analyzing Missing CSS Classes" -ForegroundColor Cyan
Write-Host "📄 Report Path: $ReportPath" -ForegroundColor Yellow

if (-not (Test-Path $ReportPath)) {
    Write-Host "❌ Report file not found: $ReportPath" -ForegroundColor Red
    Write-Host "   Run ValidateClassUsage.ps1 first to generate the report." -ForegroundColor Yellow
    exit 1
}

# Load validation report
$report = Get-Content $ReportPath | ConvertFrom-Json

# Filter for missing class issues only
$missingClasses = $report.Issues | Where-Object { $_.Type -eq "MissingClass" } | Select-Object -ExpandProperty Content

Write-Host "📊 Total missing class violations: $($missingClasses.Count)" -ForegroundColor Green

# Group and count missing classes
$grouped = $missingClasses | Group-Object | Sort-Object Count -Descending | Select-Object -First $TopCount

Write-Host "`n🏆 Top $TopCount Missing Classes:" -ForegroundColor Cyan
Write-Host "Count | Class Name" -ForegroundColor Gray
Write-Host "------|----------" -ForegroundColor Gray

foreach ($group in $grouped) {
    $count = $group.Count.ToString().PadLeft(5)
    Write-Host "$count | $($group.Name)" -ForegroundColor White
}

# Categorize missing classes by type
$categories = @{
    "Typography" = @()
    "Layout" = @()
    "Spacing" = @()
    "Colors" = @()
    "Display" = @()
    "Flexbox" = @()
    "Custom" = @()
}

foreach ($group in $grouped) {
    $className = $group.Name
    
    if ($className -match '^text-') { $categories["Typography"] += $group }
    elseif ($className -match '^(flex|justify|align|items)') { $categories["Flexbox"] += $group }
    elseif ($className -match '^(d-|display-)') { $categories["Display"] += $group }
    elseif ($className -match '^(p|m|pa|ma|pt|pb|pl|pr|mt|mb|ml|mr|gap)-') { $categories["Spacing"] += $group }
    elseif ($className -match '^(bg-|text-|border-).*-(primary|secondary|success|error|warning|info|white|black)') { $categories["Colors"] += $group }
    elseif ($className -match '^(grid|col|row|container|w-|h-)') { $categories["Layout"] += $group }
    else { $categories["Custom"] += $group }
}

Write-Host "`n📂 Missing Classes by Category:" -ForegroundColor Cyan

foreach ($category in $categories.Keys | Sort-Object) {
    $items = $categories[$category]
    if ($items.Count -gt 0) {
        $totalViolations = ($items | Measure-Object -Property Count -Sum).Sum
        Write-Host "`n$category ($($items.Count) classes, $totalViolations violations):" -ForegroundColor Yellow
        
        $items | Sort-Object Count -Descending | Select-Object -First 10 | ForEach-Object {
            $count = $_.Count.ToString().PadLeft(4)
            Write-Host "  $count | $($_.Name)" -ForegroundColor Gray
        }
        
        if ($items.Count -gt 10) {
            Write-Host "        ... and $($items.Count - 10) more" -ForegroundColor DarkGray
        }
    }
}

# Generate utility patterns that should be added to RR.Blazor
Write-Host "`n🛠️ Suggested RR.Blazor Utility Patterns:" -ForegroundColor Cyan

$suggestions = @{
    "Typography" = @()
    "Layout" = @()
    "Colors" = @()
    "Display" = @()
}

# Typography patterns
$textSizes = $grouped | Where-Object { $_.Name -match '^text-(xs|sm|base|lg|xl|2xl|3xl|4xl|5xl|6xl|7xl|8xl)$' } | Sort-Object Name
if ($textSizes.Count -gt 0) {
    $sizes = ($textSizes | ForEach-Object { $_.Name -replace '^text-', '' }) -join ', '
    $suggestions["Typography"] += "text-[$sizes]"
}

# Color patterns
$textColors = $grouped | Where-Object { $_.Name -match '^text-(primary|secondary|success|error|warning|info|white|black|muted|disabled)$' } | Sort-Object Name
if ($textColors.Count -gt 0) {
    $colors = ($textColors | ForEach-Object { $_.Name -replace '^text-', '' }) -join ', '
    $suggestions["Colors"] += "text-[$colors]"
}

# Display patterns
$displayClasses = $grouped | Where-Object { $_.Name -match '^(d-flex|d-block|d-inline|d-none|d-grid)$' } | Sort-Object Name
if ($displayClasses.Count -gt 0) {
    $displays = ($displayClasses | ForEach-Object { $_.Name -replace '^d-', '' }) -join ', '
    $suggestions["Display"] += "d-[$displays]"
}

# Layout patterns  
$flexClasses = $grouped | Where-Object { $_.Name -match '^(flex-|justify-|align-)' } | Sort-Object Name
if ($flexClasses.Count -gt 0) {
    $flexValues = ($flexClasses | ForEach-Object { 
        if ($_.Name -match '^flex-(.+)') { "flex-$($matches[1])" }
        elseif ($_.Name -match '^justify-(.+)') { "justify-$($matches[1])" }
        elseif ($_.Name -match '^align-(.+)') { "align-$($matches[1])" }
    }) | Sort-Object -Unique
    $suggestions["Layout"] += $flexValues
}

foreach ($category in $suggestions.Keys | Sort-Object) {
    $items = $suggestions[$category]
    if ($items.Count -gt 0) {
        Write-Host "`n$category utilities to add:" -ForegroundColor Yellow
        $items | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Green
        }
    }
}

Write-Host "`n✨ Summary:" -ForegroundColor Cyan
Write-Host "• Total violations: $($missingClasses.Count)" -ForegroundColor White
Write-Host "• Unique missing classes: $($grouped.Count)" -ForegroundColor White
Write-Host "• Top priority: $(($grouped | Select-Object -First 5 | ForEach-Object { $_.Name }) -join ', ')" -ForegroundColor White

Write-Host "`n🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Add the suggested utility patterns to RR.Blazor/Styles/" -ForegroundColor Yellow
Write-Host "2. Run GenerateDocumentation.ps1 to update AI documentation" -ForegroundColor Yellow
Write-Host "3. Re-run ValidateClassUsage.ps1 to verify fixes" -ForegroundColor Yellow