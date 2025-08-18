#!/usr/bin/env pwsh
<#
.SYNOPSIS
Analyzes CSS variables for duplicates, logical duplicates, and incorrect usage in RR.Blazor\Styles

.DESCRIPTION
Focused analysis of SCSS files for:
- Duplicate CSS variable definitions
- Logical duplicates (similar naming patterns that should be the same)
- Incorrect variable usage patterns
- Missing variable definitions that are referenced
- Unused variable definitions

.EXAMPLE
./AnalyzeCSSVariableUsage.ps1 -StylesPath "RR.Blazor\Styles"
#>

param(
    [string]$StylesPath = "RR.Blazor\Styles",
    [switch]$DetailedReport,
    [switch]$ExportMarkdown
)

# Color output functions
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }

Write-Info "🎯 CSS Variable Usage Analyzer for RR.Blazor"
Write-Info "═══════════════════════════════════════════════"

# Collections for analysis
$variableDefinitions = @{}     # Variables defined with values
$variableUsages = @{}          # Variables used in var() functions
$duplicateDefinitions = @()    # Same variable defined multiple times
$logicalDuplicates = @()       # Variables that seem to serve same purpose
$incorrectUsage = @()          # Usage patterns that look wrong
$unusedDefinitions = @()       # Defined but never used
$missingDefinitions = @()      # Used but never defined

# Find all SCSS files in the Styles directory
$scssFiles = Get-ChildItem -Path $StylesPath -Recurse -Filter "*.scss" -ErrorAction SilentlyContinue

Write-Info "📁 Analyzing $($scssFiles.Count) SCSS files in $StylesPath"

foreach ($scssFile in $scssFiles) {
    try {
        $content = Get-Content $scssFile.FullName -Raw -ErrorAction Continue
        $lines = Get-Content $scssFile.FullName -ErrorAction Continue
        
        # Extract CSS custom property definitions (--variable: value)
        $definitionMatches = [regex]::Matches($content, '--([a-zA-Z0-9-]+)\s*:\s*([^;}]+)[;}]', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $definitionMatches) {
            $varName = $match.Groups[1].Value.Trim()
            $varValue = $match.Groups[2].Value.Trim()
            $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
            
            if (-not $variableDefinitions.ContainsKey($varName)) {
                $variableDefinitions[$varName] = @()
            }
            
            $variableDefinitions[$varName] += @{
                File = $scssFile.FullName
                RelativePath = $scssFile.FullName -replace [regex]::Escape((Get-Location).Path + "\"), ""
                Line = $lineNumber
                Value = $varValue
                Context = $lines[$lineNumber - 1] -replace '^\s+', ''
            }
        }
        
        # Extract CSS custom property usages (var(--variable))
        $usageMatches = [regex]::Matches($content, 'var\(\s*--([a-zA-Z0-9-]+)(?:\s*,\s*([^)]+))?\s*\)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $usageMatches) {
            $varName = $match.Groups[1].Value.Trim()
            $defaultValue = if ($match.Groups[2].Success) { $match.Groups[2].Value.Trim() } else { $null }
            $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
            
            if (-not $variableUsages.ContainsKey($varName)) {
                $variableUsages[$varName] = @()
            }
            
            $variableUsages[$varName] += @{
                File = $scssFile.FullName
                RelativePath = $scssFile.FullName -replace [regex]::Escape((Get-Location).Path + "\"), ""
                Line = $lineNumber
                DefaultValue = $defaultValue
                Context = $lines[$lineNumber - 1] -replace '^\s+', ''
            }
        }
        
    }
    catch {
        Write-Warning "⚠️  Could not process $($scssFile.Name): $($_.Exception.Message)"
    }
}

Write-Info "🔍 Analyzing patterns and inconsistencies..."

# Find duplicate definitions (same variable name defined multiple times)
foreach ($varName in $variableDefinitions.Keys) {
    $definitions = $variableDefinitions[$varName]
    if ($definitions.Count -gt 1) {
        # Check if values are actually different
        $uniqueValues = $definitions | Select-Object -ExpandProperty Value -Unique
        if ($uniqueValues.Count -gt 1) {
            $duplicateDefinitions += @{
                Variable = $varName
                Definitions = $definitions
                UniqueValues = $uniqueValues
                Severity = "High"
                Issue = "Same variable defined with different values"
            }
        } else {
            $duplicateDefinitions += @{
                Variable = $varName
                Definitions = $definitions
                UniqueValues = $uniqueValues
                Severity = "Medium"
                Issue = "Same variable defined multiple times with same value (redundant)"
            }
        }
    }
}

# Find logical duplicates (variables that seem to serve the same purpose)
$variablesByCategory = @{}
foreach ($varName in $variableDefinitions.Keys) {
    # Categorize variables by their naming patterns
    $category = ""
    if ($varName -match "^color-") { $category = "color" }
    elseif ($varName -match "^space-|^spacing-") { $category = "spacing" }
    elseif ($varName -match "^font-|^text-") { $category = "typography" }
    elseif ($varName -match "^duration-|^ease-|^transition-") { $category = "animation" }
    elseif ($varName -match "^radius-|^border-") { $category = "border" }
    elseif ($varName -match "^shadow-") { $category = "shadow" }
    elseif ($varName -match "^z-") { $category = "zindex" }
    else { $category = "other" }
    
    if (-not $variablesByCategory.ContainsKey($category)) {
        $variablesByCategory[$category] = @()
    }
    $variablesByCategory[$category] += $varName
}

# Look for potential logical duplicates within categories
foreach ($category in $variablesByCategory.Keys) {
    $variables = $variablesByCategory[$category]
    foreach ($var1 in $variables) {
        foreach ($var2 in $variables) {
            if ($var1 -ne $var2) {
                $value1 = $variableDefinitions[$var1][0].Value
                $value2 = $variableDefinitions[$var2][0].Value
                
                # Check if they have the same value but different names
                if ($value1 -eq $value2) {
                    # Check if names are suspiciously similar
                    $similarity = Get-StringSimilarity $var1 $var2
                    if ($similarity -gt 0.7) {
                        $logicalDuplicates += @{
                            Variables = @($var1, $var2)
                            Value = $value1
                            Category = $category
                            Similarity = $similarity
                            Severity = "Medium"
                            Issue = "Variables with similar names and same value"
                        }
                    }
                }
            }
        }
    }
}

# Find incorrect usage patterns
foreach ($varName in $variableUsages.Keys) {
    $usages = $variableUsages[$varName]
    
    # Pattern 1: Using --progress-value instead of --progress-width type mismatches
    if ($varName -match "(value|width|height|size)" -and $variableDefinitions.Keys -contains ($varName -replace "value", "width")) {
        $incorrectUsage += @{
            Variable = $varName
            Usages = $usages
            Severity = "High"
            Issue = "Potential naming mismatch - using '$varName' but '--$($varName -replace "value", "width")' exists"
            SuggestedFix = $varName -replace "value", "width"
        }
    }
    
    # Pattern 2: Inconsistent naming conventions
    if ($varName -match "([a-zA-Z]+)_([a-zA-Z]+)" -and $variableDefinitions.Keys -contains ($varName -replace "_", "-")) {
        $incorrectUsage += @{
            Variable = $varName
            Usages = $usages
            Severity = "Medium"
            Issue = "Inconsistent naming convention - using underscore instead of hyphen"
            SuggestedFix = $varName -replace "_", "-"
        }
    }
    
    # Pattern 3: Deprecated or old variable names
    $deprecatedPatterns = @(
        @{ Old = "primary-color"; New = "color-primary" },
        @{ Old = "text-color"; New = "color-text" },
        @{ Old = "bg-color"; New = "color-surface" }
    )
    
    foreach ($pattern in $deprecatedPatterns) {
        if ($varName -eq $pattern.Old -and $variableDefinitions.Keys -contains $pattern.New) {
            $incorrectUsage += @{
                Variable = $varName
                Usages = $usages
                Severity = "Low"
                Issue = "Using deprecated variable name"
                SuggestedFix = $pattern.New
            }
        }
    }
}

# Find missing definitions (used but never defined)
foreach ($varName in $variableUsages.Keys) {
    if (-not $variableDefinitions.ContainsKey($varName)) {
        $missingDefinitions += @{
            Variable = $varName
            Usages = $variableUsages[$varName]
            Severity = "High"
            Issue = "Variable used but never defined"
        }
    }
}

# Find unused definitions (defined but never used)
foreach ($varName in $variableDefinitions.Keys) {
    if (-not $variableUsages.ContainsKey($varName)) {
        $unusedDefinitions += @{
            Variable = $varName
            Definitions = $variableDefinitions[$varName]
            Severity = "Low"
            Issue = "Variable defined but never used"
        }
    }
}

# Helper function for string similarity
function Get-StringSimilarity {
    param([string]$String1, [string]$String2)
    
    if ($String1.Length -eq 0 -and $String2.Length -eq 0) { return 1.0 }
    if ($String1.Length -eq 0 -or $String2.Length -eq 0) { return 0.0 }
    
    $distance = Get-LevenshteinDistance $String1 $String2
    $maxLength = [Math]::Max($String1.Length, $String2.Length)
    return 1.0 - ($distance / $maxLength)
}

function Get-LevenshteinDistance {
    param([string]$String1, [string]$String2)
    
    if ($String1.Length -eq 0) { return $String2.Length }
    if ($String2.Length -eq 0) { return $String1.Length }
    
    $matrix = New-Object 'int[,]' ($String1.Length + 1), ($String2.Length + 1)
    
    for ($i = 0; $i -le $String1.Length; $i++) { $matrix[$i, 0] = $i }
    for ($j = 0; $j -le $String2.Length; $j++) { $matrix[0, $j] = $j }
    
    for ($i = 1; $i -le $String1.Length; $i++) {
        for ($j = 1; $j -le $String2.Length; $j++) {
            $cost = if ($String1[$i-1] -eq $String2[$j-1]) { 0 } else { 1 }
            $matrix[$i, $j] = [Math]::Min([Math]::Min($matrix[$i-1, $j] + 1, $matrix[$i, $j-1] + 1), $matrix[$i-1, $j-1] + $cost)
        }
    }
    
    return $matrix[$String1.Length, $String2.Length]
}

# Report results
Write-Info "`n════════════════════════════════════════════════"
Write-Info "📊 ANALYSIS RESULTS"
Write-Info "════════════════════════════════════════════════"

Write-Success "✅ Found $($variableDefinitions.Keys.Count) unique CSS variable definitions"
Write-Success "✅ Found $($variableUsages.Keys.Count) unique CSS variables in use"

# High severity issues
$highSeverityIssues = @()
$highSeverityIssues += $duplicateDefinitions | Where-Object { $_.Severity -eq "High" }
$highSeverityIssues += $incorrectUsage | Where-Object { $_.Severity -eq "High" }
$highSeverityIssues += $missingDefinitions

if ($highSeverityIssues.Count -gt 0) {
    Write-Error "`n🚨 HIGH SEVERITY ISSUES ($($highSeverityIssues.Count)):"
    
    foreach ($issue in $highSeverityIssues | Select-Object -First 10) {
        Write-Error "   • --$($issue.Variable): $($issue.Issue)"
        if ($issue.SuggestedFix) {
            Write-Warning "     💡 Suggested fix: Use --$($issue.SuggestedFix)"
        }
    }
    if ($highSeverityIssues.Count -gt 10) {
        Write-Error "   ... and $($highSeverityIssues.Count - 10) more high severity issues"
    }
}

# Medium severity issues
$mediumSeverityIssues = @()
$mediumSeverityIssues += $duplicateDefinitions | Where-Object { $_.Severity -eq "Medium" }
$mediumSeverityIssues += $logicalDuplicates | Where-Object { $_.Severity -eq "Medium" }
$mediumSeverityIssues += $incorrectUsage | Where-Object { $_.Severity -eq "Medium" }

if ($mediumSeverityIssues.Count -gt 0) {
    Write-Warning "`n⚠️  MEDIUM SEVERITY ISSUES ($($mediumSeverityIssues.Count)):"
    
    foreach ($issue in $mediumSeverityIssues | Select-Object -First 5) {
        if ($issue.Variables) {
            Write-Warning "   • $($issue.Variables -join ' & '): $($issue.Issue)"
        } else {
            Write-Warning "   • --$($issue.Variable): $($issue.Issue)"
        }
    }
    if ($mediumSeverityIssues.Count -gt 5) {
        Write-Warning "   ... and $($mediumSeverityIssues.Count - 5) more medium severity issues"
    }
}

# Low severity issues
$lowSeverityIssues = $unusedDefinitions + ($incorrectUsage | Where-Object { $_.Severity -eq "Low" })

if ($lowSeverityIssues.Count -gt 0) {
    Write-Info "`n📝 LOW SEVERITY ISSUES ($($lowSeverityIssues.Count)):"
    Write-Info "   • $($unusedDefinitions.Count) unused variable definitions"
    Write-Info "   • $(($incorrectUsage | Where-Object { $_.Severity -eq 'Low' }).Count) deprecated variable usages"
}

# Detailed report
if ($DetailedReport) {
    Write-Info "`n📋 DETAILED FINDINGS:"
    Write-Info "═══════════════════════"
    
    # Show most critical issues first
    foreach ($issue in $highSeverityIssues | Select-Object -First 5) {
        Write-Info "`n🔴 HIGH: --$($issue.Variable)"
        Write-Info "   Issue: $($issue.Issue)"
        
        if ($issue.Definitions) {
            Write-Info "   Definitions:"
            foreach ($def in $issue.Definitions | Select-Object -First 3) {
                Write-Info "     $($def.RelativePath):$($def.Line) = $($def.Value)"
            }
        }
        
        if ($issue.Usages) {
            Write-Info "   Usages:"
            foreach ($usage in $issue.Usages | Select-Object -First 3) {
                Write-Info "     $($usage.RelativePath):$($usage.Line)"
            }
        }
    }
}

# Export to markdown if requested
if ($ExportMarkdown) {
    $markdown = @"
# CSS Variable Analysis Report

Generated: $(Get-Date)

## Summary
- **Total Variables Defined**: $($variableDefinitions.Keys.Count)
- **Total Variables Used**: $($variableUsages.Keys.Count)
- **High Severity Issues**: $($highSeverityIssues.Count)
- **Medium Severity Issues**: $($mediumSeverityIssues.Count)
- **Low Severity Issues**: $($lowSeverityIssues.Count)

## High Severity Issues

"@
    
    foreach ($issue in $highSeverityIssues) {
        $markdown += @"

### --$($issue.Variable)
**Issue**: $($issue.Issue)
$(if ($issue.SuggestedFix) { "**Suggested Fix**: Use --$($issue.SuggestedFix)" })

"@
        if ($issue.Definitions) {
            $markdown += "**Definitions**:`n"
            foreach ($def in $issue.Definitions) {
                $markdown += "- $($def.RelativePath):$($def.Line) = ``$($def.Value)```n"
            }
        }
        if ($issue.Usages) {
            $markdown += "**Usages**:`n"
            foreach ($usage in $issue.Usages) {
                $markdown += "- $($usage.RelativePath):$($usage.Line)`n"
            }
        }
    }
    
    $reportPath = "css-variable-analysis-report.md"
    $markdown | Out-File $reportPath -Encoding UTF8
    Write-Success "`n📄 Detailed report exported to: $reportPath"
}

Write-Info "`n🎯 PRIORITY RECOMMENDATIONS:"
Write-Info "1. Fix HIGH severity variable mismatches immediately (these break functionality)"
Write-Info "2. Resolve duplicate definitions with different values"  
Write-Info "3. Standardize variable naming conventions"
Write-Info "4. Remove unused variable definitions to reduce CSS size"

return @{
    HighSeverityIssues = $highSeverityIssues.Count
    MediumSeverityIssues = $mediumSeverityIssues.Count
    LowSeverityIssues = $lowSeverityIssues.Count
    TotalIssues = $highSeverityIssues.Count + $mediumSeverityIssues.Count + $lowSeverityIssues.Count
}