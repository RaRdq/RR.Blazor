#!/usr/bin/env pwsh
<#
.SYNOPSIS
Detects CSS variable mismatches between SCSS files and C# Razor components

.DESCRIPTION
Analyzes SCSS files for CSS custom property usage (var(--variable-name)) and compares
with CSS custom properties set in C# Razor components to identify potential mismatches
that could cause styling/animation failures.

.EXAMPLE
./DetectCSSVariableMismatches.ps1
#>

param(
    [string]$ProjectPath = ".",
    [switch]$DetailedReport,
    [switch]$ExportJson
)

# Color output functions
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }

Write-Info "🔍 CSS Variable Mismatch Detective Starting..."
Write-Info "═══════════════════════════════════════════════"

# Initialize collections
$scssVariables = @{}
$razorVariables = @{}
$potentialMismatches = @()
$suspiciousPatterns = @()

# Find all SCSS files
$scssFiles = Get-ChildItem -Path $ProjectPath -Recurse -Include "*.scss" -ErrorAction SilentlyContinue

Write-Info "📋 Analyzing $($scssFiles.Count) SCSS files..."

foreach ($scssFile in $scssFiles) {
    try {
        $content = Get-Content $scssFile.FullName -Raw -ErrorAction Continue
        
        # Extract CSS variables used in var() functions
        $varMatches = [regex]::Matches($content, 'var\(\s*--([^,\)]+)(?:,\s*([^)]+))?\s*\)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $varMatches) {
            $varName = $match.Groups[1].Value.Trim()
            $defaultValue = if ($match.Groups[2].Success) { $match.Groups[2].Value.Trim() } else { $null }
            
            if (-not $scssVariables.ContainsKey($varName)) {
                $scssVariables[$varName] = @()
            }
            
            $scssVariables[$varName] += @{
                File = $scssFile.FullName
                Line = ($content.Substring(0, $match.Index) -split "`n").Count
                Context = $match.Value
                DefaultValue = $defaultValue
            }
        }
    }
    catch {
        Write-Warning "⚠️  Could not process $($scssFile.Name): $($_.Exception.Message)"
    }
}

# Find all Razor files
$razorFiles = Get-ChildItem -Path $ProjectPath -Recurse -Include "*.razor" -ErrorAction SilentlyContinue

Write-Info "📋 Analyzing $($razorFiles.Count) Razor files..."

foreach ($razorFile in $razorFiles) {
    try {
        $content = Get-Content $razorFile.FullName -Raw -ErrorAction Continue
        
        # Extract CSS variables being set in style attributes or C# code
        # Pattern 1: style="--variable: value"  
        $styleMatches = [regex]::Matches($content, 'style\s*=\s*["'']([^"'']*--([^:;]+):\s*([^;"'']+))["'']', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $styleMatches) {
            $varName = $match.Groups[2].Value.Trim()
            $varValue = $match.Groups[3].Value.Trim()
            
            if (-not $razorVariables.ContainsKey($varName)) {
                $razorVariables[$varName] = @()
            }
            
            $razorVariables[$varName] += @{
                File = $razorFile.FullName
                Line = ($content.Substring(0, $match.Index) -split "`n").Count
                Context = $match.Groups[1].Value
                SetMethod = "StyleAttribute"
                Value = $varValue
            }
        }
        
        # Pattern 2: C# string interpolation setting CSS variables
        $csharpMatches = [regex]::Matches($content, '\$\s*"[^"]*--([^:;"]+):\s*\{([^}]+)\}', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $csharpMatches) {
            $varName = $match.Groups[1].Value.Trim()
            $csharpExpression = $match.Groups[2].Value.Trim()
            
            if (-not $razorVariables.ContainsKey($varName)) {
                $razorVariables[$varName] = @()
            }
            
            $razorVariables[$varName] += @{
                File = $razorFile.FullName
                Line = ($content.Substring(0, $match.Index) -split "`n").Count
                Context = $match.Value
                SetMethod = "CSharpInterpolation"
                Value = $csharpExpression
            }
        }
        
        # Pattern 3: styles.Add() method calls
        $addMatches = [regex]::Matches($content, 'styles\.Add\s*\(\s*["'']--([^"'']+)["'']', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $addMatches) {
            $varName = $match.Groups[1].Value.Trim()
            
            if (-not $razorVariables.ContainsKey($varName)) {
                $razorVariables[$varName] = @()
            }
            
            $razorVariables[$varName] += @{
                File = $razorFile.FullName
                Line = ($content.Substring(0, $match.Index) -split "`n").Count
                Context = $match.Value
                SetMethod = "StylesAdd"
                Value = "Dynamic"
            }
        }
        
    }
    catch {
        Write-Warning "⚠️  Could not process $($razorFile.Name): $($_.Exception.Message)"
    }
}

# Helper function for Levenshtein distance
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

Write-Info "🔍 Analyzing variable usage patterns..."

# Find potential mismatches
foreach ($scssVar in $scssVariables.Keys) {
    # Direct mismatch - used in SCSS but never set in Razor
    if (-not $razorVariables.ContainsKey($scssVar)) {
        # Look for similar names that might be typos
        $similarVars = $razorVariables.Keys | Where-Object {
            $_ -like "*$($scssVar.Split('-')[-1])*" -or 
            $scssVar -like "*$($_.Split('-')[-1])*" -or
            (Get-LevenshteinDistance $_ $scssVar) -le 2
        }
        
        $potentialMismatches += @{
            Type = "UnusedInRazor"
            ScssVariable = $scssVar
            ScssUsages = $scssVariables[$scssVar]
            SimilarRazorVars = $similarVars
            Severity = if ($similarVars) { "High" } else { "Medium" }
            Description = "SCSS uses --$scssVar but no Razor component sets this variable"
        }
    }
}

foreach ($razorVar in $razorVariables.Keys) {
    # Reverse mismatch - set in Razor but never used in SCSS
    if (-not $scssVariables.ContainsKey($razorVar)) {
        # Look for similar names
        $similarVars = $scssVariables.Keys | Where-Object {
            $_ -like "*$($razorVar.Split('-')[-1])*" -or 
            $razorVar -like "*$($_.Split('-')[-1])*" -or
            (Get-LevenshteinDistance $_ $razorVar) -le 2
        }
        
        $potentialMismatches += @{
            Type = "UnusedInScss"
            RazorVariable = $razorVar
            RazorUsages = $razorVariables[$razorVar]
            SimilarScssVars = $similarVars
            Severity = if ($similarVars) { "High" } else { "Low" }
            Description = "Razor sets --$razorVar but no SCSS file uses this variable"
        }
    }
}

# Detect suspicious patterns
$suspiciousPatterns += $scssVariables.Keys | Where-Object { 
    $_ -match "(value|width|height|size|color)" -and 
    ($razorVariables.Keys -contains ($_ -replace "value", "width") -or 
     $razorVariables.Keys -contains ($_ -replace "width", "value") -or
     $razorVariables.Keys -contains ($_ -replace "height", "size"))
} | ForEach-Object {
    @{
        Pattern = "SuspiciousNaming"
        Variable = $_
        Description = "Variable name suggests possible naming confusion with width/value/size variants"
        Severity = "Medium"
    }
}

# Report results
Write-Info "════════════════════════════════════════════════"
Write-Info "📊 ANALYSIS COMPLETE"
Write-Info "════════════════════════════════════════════════"

Write-Success "✅ Found $($scssVariables.Keys.Count) unique CSS variables in SCSS files"
Write-Success "✅ Found $($razorVariables.Keys.Count) unique CSS variables in Razor files"

$highSeverityIssues = $potentialMismatches | Where-Object { $_.Severity -eq "High" }
$mediumSeverityIssues = $potentialMismatches | Where-Object { $_.Severity -eq "Medium" }

if ($highSeverityIssues) {
    Write-Error "🚨 HIGH SEVERITY MISMATCHES ($($highSeverityIssues.Count)):"
    foreach ($issue in $highSeverityIssues) {
        Write-Error "   • $($issue.Description)"
        if ($issue.SimilarRazorVars) {
            Write-Warning "     Similar Razor vars: $($issue.SimilarRazorVars -join ', ')"
        }
        if ($issue.SimilarScssVars) {
            Write-Warning "     Similar SCSS vars: $($issue.SimilarScssVars -join ', ')"
        }
    }
}

if ($mediumSeverityIssues) {
    Write-Warning "⚠️  MEDIUM SEVERITY ISSUES ($($mediumSeverityIssues.Count)):"
    foreach ($issue in $mediumSeverityIssues | Select-Object -First 5) {
        Write-Warning "   • $($issue.Description)"
    }
    if ($mediumSeverityIssues.Count -gt 5) {
        Write-Warning "   ... and $($mediumSeverityIssues.Count - 5) more"
    }
}

if ($suspiciousPatterns) {
    Write-Warning "🔍 SUSPICIOUS PATTERNS ($($suspiciousPatterns.Count)):"
    foreach ($pattern in $suspiciousPatterns | Select-Object -First 3) {
        Write-Warning "   • --$($pattern.Variable): $($pattern.Description)"
    }
}

if ($DetailedReport) {
    Write-Info "`n📋 DETAILED FINDINGS:"
    Write-Info "═══════════════════════"
    
    foreach ($mismatch in $potentialMismatches | Where-Object { $_.Severity -eq "High" }) {
        Write-Info "`nIssue: $($mismatch.Description)"
        Write-Info "Severity: $($mismatch.Severity)"
        
        if ($mismatch.ScssUsages) {
            Write-Info "SCSS Usages:"
            foreach ($usage in $mismatch.ScssUsages | Select-Object -First 3) {
                Write-Info "  $($usage.File -replace '.*\\', ''):$($usage.Line) - $($usage.Context)"
            }
        }
        
        if ($mismatch.RazorUsages) {
            Write-Info "Razor Usages:"
            foreach ($usage in $mismatch.RazorUsages | Select-Object -First 3) {
                Write-Info "  $($usage.File -replace '.*\\', ''):$($usage.Line) - $($usage.Context)"
            }
        }
    }
}

# Export results if requested
if ($ExportJson) {
    $results = @{
        Summary = @{
            ScssVariableCount = $scssVariables.Keys.Count
            RazorVariableCount = $razorVariables.Keys.Count
            HighSeverityIssues = $highSeverityIssues.Count
            MediumSeverityIssues = $mediumSeverityIssues.Count
            SuspiciousPatterns = $suspiciousPatterns.Count
        }
        Mismatches = $potentialMismatches
        SuspiciousPatterns = $suspiciousPatterns
        ScssVariables = $scssVariables
        RazorVariables = $razorVariables
    }
    
    $jsonPath = Join-Path $ProjectPath "css-variable-analysis.json"
    $results | ConvertTo-Json -Depth 10 | Out-File $jsonPath
    Write-Success "📄 Results exported to: $jsonPath"
}

Write-Info "`n🎯 RECOMMENDATIONS:"
Write-Info "• Focus on HIGH severity mismatches first"
Write-Info "• Check components with dynamic styling (progress bars, sliders, etc.)"
Write-Info "• Verify variable names match between SCSS var() and C# property setting"
Write-Info "• Consider standardizing CSS variable naming conventions"

return @{
    HighSeverityIssues = $highSeverityIssues.Count
    MediumSeverityIssues = $mediumSeverityIssues.Count
    TotalIssues = $potentialMismatches.Count
}