#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates CSS variable usage in SCSS and compiled CSS files
    
.DESCRIPTION
    This script analyzes SCSS files to ensure all CSS variables (--var-name) are defined.
    It validates against the design system variables and detects undefined variable usage.
    
.PARAMETER SolutionPath
    Path to the solution root directory (default: current directory)
    
.PARAMETER StylesPath
    Path to the RR.Blazor styles directory (default: "RR.Blazor/Styles")
    
.PARAMETER CompiledCSSPath
    Path to compiled CSS file (default: "RR.Blazor/wwwroot/css/main.css")
    
.PARAMETER ShowDetails
    Show detailed information about each issue (default: true)
    
.EXAMPLE
    .\ValidateCSSVariables.ps1
    
.EXAMPLE
    .\ValidateCSSVariables.ps1 -SolutionPath "C:\MyProject" -ShowDetails
#>

param(
    [string]$SolutionPath = ".",
    [string]$StylesPath = "RR.Blazor/Styles",
    [string]$CompiledCSSPath = "RR.Blazor/wwwroot/css/main.css",
    [switch]$ShowDetails = $true
)

$ErrorActionPreference = "Stop"

# Step 1: Extract all DEFINED CSS variables from the design system
Write-Host "Scanning CSS variable definitions..." -ForegroundColor Gray

$definedVariables = @{}

# Read variables from _variables.scss
$variablesFile = Join-Path $SolutionPath "$StylesPath/abstracts/_variables.scss"
if (Test-Path $variablesFile) {
    $content = Get-Content $variablesFile -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        $definedVariables[$varName] = "variables.scss"
    }
    Write-Host "  Found $($varMatches.Count) variables in _variables.scss" -ForegroundColor Gray
}

# Read variables from _colors.scss
$colorsFile = Join-Path $SolutionPath "$StylesPath/abstracts/_colors.scss"
if (Test-Path $colorsFile) {
    $content = Get-Content $colorsFile -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        $definedVariables[$varName] = "colors.scss"
    }
    Write-Host "  Found $($varMatches.Count) variables in _colors.scss" -ForegroundColor Gray
}

# Read variables from _typography.scss
$typographyFile = Join-Path $SolutionPath "$StylesPath/abstracts/_typography.scss"
if (Test-Path $typographyFile) {
    $content = Get-Content $typographyFile -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        $definedVariables[$varName] = "typography.scss"
    }
    Write-Host "  Found $($varMatches.Count) variables in _typography.scss" -ForegroundColor Gray
}

# Read variables from _spacing.scss
$spacingFile = Join-Path $SolutionPath "$StylesPath/abstracts/_spacing.scss"
if (Test-Path $spacingFile) {
    $content = Get-Content $spacingFile -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        $definedVariables[$varName] = "spacing.scss"
    }
    Write-Host "  Found $($varMatches.Count) variables in _spacing.scss" -ForegroundColor Gray
}

# Read variables from _shadows.scss
$shadowsFile = Join-Path $SolutionPath "$StylesPath/abstracts/_shadows.scss"
if (Test-Path $shadowsFile) {
    $content = Get-Content $shadowsFile -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        $definedVariables[$varName] = "shadows.scss"
    }
    Write-Host "  Found $($varMatches.Count) variables in _shadows.scss" -ForegroundColor Gray
}

# Read variables from all theme files
$themeFiles = Get-ChildItem -Path (Join-Path $SolutionPath "$StylesPath/themes") -Filter "*.scss" -ErrorAction SilentlyContinue
foreach ($themeFile in $themeFiles) {
    $content = Get-Content $themeFile.FullName -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        $definedVariables[$varName] = "themes/$($themeFile.Name)"
    }
    if ($varMatches.Count -gt 0) {
        Write-Host "  Found $($varMatches.Count) variables in themes/$($themeFile.Name)" -ForegroundColor Gray
    }
}

# Also extract from compiled CSS to catch runtime-generated variables
$compiledCSSFullPath = Join-Path $SolutionPath $CompiledCSSPath
if (Test-Path $compiledCSSFullPath) {
    $content = Get-Content $compiledCSSFullPath -Raw
    $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+):\s*[^;]+;')
    foreach ($match in $varMatches) {
        $varName = "--$($match.Groups[1].Value)"
        if (-not $definedVariables.ContainsKey($varName)) {
            $definedVariables[$varName] = "compiled-css"
        }
    }
    Write-Host "  Found additional variables in compiled CSS" -ForegroundColor Gray
}

Write-Host "`nTotal defined variables: $($definedVariables.Count)" -ForegroundColor Green

# Step 2: Find all USED CSS variables in SCSS files
Write-Host "Analyzing SCSS var() usage patterns..." -ForegroundColor Gray

$issues = @()
$scssFiles = Get-ChildItem -Path (Join-Path $SolutionPath $StylesPath) -Recurse -Filter "*.scss"

foreach ($file in $scssFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrEmpty($content)) { continue }
    
    $relativePath = $file.FullName.Replace((Join-Path $SolutionPath $StylesPath), "").TrimStart('\', '/')
    
    # Find all var(--variable-name) usages
    $varUsages = [regex]::Matches($content, 'var\((--[a-zA-Z0-9-]+)(?:,\s*[^)]+)?\)')
    
    foreach ($usage in $varUsages) {
        $varName = $usage.Groups[1].Value
        $lineNumber = ($content.Substring(0, $usage.Index) -split "`n").Length
        
        # Check if variable is defined
        if (-not $definedVariables.ContainsKey($varName)) {
            # Try to find a close match for suggestion
            $suggestion = ""
            $varBaseName = $varName -replace '^--', ''
            
            # Common mapping patterns
            $mappings = @{
                'color-gray-100' = '--color-surface'
                'color-gray-200' = '--color-surface-secondary'
                'color-gray-300' = '--color-border'
                'color-gray-400' = '--color-border-strong'
                'color-gray-500' = '--color-text-muted'
                'color-gray-600' = '--color-text-secondary'
                'color-gray-700' = '--color-text-muted'
                'color-gray-800' = '--color-text'
                'color-gray-900' = '--color-text-primary'
                'color-info-light' = '--color-info-bg'
                'offset-2-5' = '--space-2-5'
                'offset-10' = '--space-10'
                'primary-30' = '--color-primary-light'
                'primary-10' = '--color-primary-bg'
            }
            
            if ($mappings.ContainsKey($varBaseName)) {
                $suggestion = "Use '$($mappings[$varBaseName])' instead"
            } else {
                # Try to find similar variable
                $similar = $definedVariables.Keys | Where-Object { 
                    $_ -like "*$($varBaseName -replace 'gray', 'surface')*" -or
                    $_ -like "*$($varBaseName -replace 'offset', 'space')*" -or
                    $_ -like "*$($varBaseName -replace '-light', '-bg')*"
                } | Select-Object -First 1
                
                if ($similar) {
                    $suggestion = "Did you mean '$similar'?"
                }
            }
            
            $issues += [PSCustomObject]@{
                File = "Styles/$relativePath"
                Line = $lineNumber
                Variable = $varName
                Context = $usage.Value
                Type = "UndefinedVariable"
                Suggestion = $suggestion
            }
        }
    }
}

# Step 3: Check for hardcoded color values that should use variables
Write-Host "Detecting hardcoded color/spacing values..." -ForegroundColor Gray

foreach ($file in $scssFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrEmpty($content)) { continue }
    
    $relativePath = $file.FullName.Replace((Join-Path $SolutionPath $StylesPath), "").TrimStart('\', '/')
    
    # Skip files that are allowed to have hardcoded values
    if ($relativePath -match '_variables\.scss|_colors\.scss|_reset\.scss') {
        continue
    }
    
    # Find hex colors
    $hexColors = [regex]::Matches($content, '#[0-9a-fA-F]{3,8}\b')
    foreach ($color in $hexColors) {
        $lineNumber = ($content.Substring(0, $color.Index) -split "`n").Length
        
        # Skip if it's in a comment
        $lineContent = ($content -split "`n")[$lineNumber - 1]
        if ($lineContent -match '^\s*//' -or $lineContent -match '/\*') {
            continue
        }
        
        $issues += [PSCustomObject]@{
            File = "Styles/$relativePath"
            Line = $lineNumber
            Variable = $color.Value
            Context = $lineContent.Trim()
            Type = "HardcodedColor"
            Suggestion = "Use a CSS variable instead (e.g., var(--color-primary))"
        }
    }
    
    # Find rgb/rgba colors
    $rgbColors = [regex]::Matches($content, 'rgba?\([^)]+\)')
    foreach ($color in $rgbColors) {
        $lineNumber = ($content.Substring(0, $color.Index) -split "`n").Length
        
        # Skip if it's in a CSS variable definition or mixin
        $lineContent = ($content -split "`n")[$lineNumber - 1]
        if ($lineContent -match '^\s*//' -or $lineContent -match '--[a-zA-Z0-9-]+:' -or $lineContent -match '@mixin') {
            continue
        }
        
        # Skip if it's a dynamic opacity (common pattern)
        if ($color.Value -match 'rgba?\([^,]+,\s*[^,]+,\s*[^,]+,\s*(var\(|0\.\d+)\)') {
            continue
        }
        
        $issues += [PSCustomObject]@{
            File = "Styles/$relativePath"
            Line = $lineNumber
            Variable = $color.Value
            Context = $lineContent.Trim()
            Type = "HardcodedColor"
            Suggestion = "Use a CSS variable instead (e.g., var(--color-primary))"
        }
    }
}

# Step 4: Report findings
Write-Host "Validation complete: $($issues.Count) issues found" -ForegroundColor Gray

$issuesByType = $issues | Group-Object Type
foreach ($group in $issuesByType) {
    $color = switch ($group.Name) {
        "UndefinedVariable" { "Red" }
        "HardcodedColor" { "Yellow" }
        default { "White" }
    }
    Write-Host "  $($group.Name): $($group.Count)" -ForegroundColor $color
}

if ($ShowDetails -and $issues.Count -gt 0) {
    
    # Group by file for better readability
    $issuesByFile = $issues | Group-Object File | Sort-Object Name
    
    foreach ($fileGroup in $issuesByFile) {
        Write-Host "`n$($fileGroup.Name):" -ForegroundColor Cyan
        
        $sortedIssues = $fileGroup.Group | Sort-Object Line
        foreach ($issue in $sortedIssues) {
            $typeIcon = switch ($issue.Type) {
                "UndefinedVariable" { "[ERROR]" }
                "HardcodedColor" { "[WARNING]" }
                default { "[INFO]" }
            }
            
            Write-Host "  $typeIcon Line $($issue.Line): $($issue.Variable)" -ForegroundColor White
            if ($issue.Context) {
                Write-Host "     Context: $($issue.Context)" -ForegroundColor Gray
            }
            if ($issue.Suggestion) {
                Write-Host "     SUGGESTION: $($issue.Suggestion)" -ForegroundColor Green
            }
        }
    }
}

# Step 5: Generate report
$report = @{
    Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    TotalDefinedVariables = $definedVariables.Count
    TotalIssues = $issues.Count
    IssuesByType = ($issuesByType | ForEach-Object { @{ Type = $_.Name; Count = $_.Count } })
    Issues = $issues
}

$reportPath = "css-variables-validation-report.json"
$report | ConvertTo-Json -Depth 10 | Set-Content $reportPath
Write-Host "`nJSON report saved to: $reportPath" -ForegroundColor Cyan

# Exit code for CI/CD integration
if ($issues.Count -gt 0) {
    Write-Host "`nValidation failed: $($issues.Count) issues" -ForegroundColor Red
    exit 1
} else {
    Write-Host "CSS variables: 0 undefined references" -ForegroundColor Gray
    exit 0
}
