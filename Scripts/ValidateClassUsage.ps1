#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates CSS class usage in Blazor Razor files against RR.Blazor styles

.DESCRIPTION
    This script analyzes Razor files to ensure all CSS classes exist in RR.Blazor styles.
    It can target specific projects or auto-detect client projects while excluding server modules.

.PARAMETER SolutionPath
    Path to the solution root directory (default: current directory)

.PARAMETER ProjectPaths
    Specific project paths to validate. If empty, auto-detects client projects.
    Supports both relative and absolute paths.

.PARAMETER StylesPath
    Path to the RR.Blazor styles directory (default: "RR.Blazor/Styles")

.PARAMETER StylesDocPath
    Path to the RR.Blazor styles documentation JSON (default: "RR.Blazor/wwwroot/rr-ai-styles.json")

.PARAMETER ShowInlineStyles
    Show warnings for inline style attributes (default: true)

.PARAMETER Verbose
    Enable verbose output for debugging

.EXAMPLE
    # Auto-detect client projects (excludes Server and RR.* except RR.Blazor)
    .\ValidateClassUsage.ps1

.EXAMPLE
    # Validate specific projects only
    .\ValidateClassUsage.ps1 -ProjectPaths @("MyApp.Client", "MyApp.Shared.Client")

.EXAMPLE
    # Validate with custom solution path
    .\ValidateClassUsage.ps1 -SolutionPath "C:\MyProject" -ProjectPaths @("MyApp.Client")
#>

# Class Usage Validation Script
# Validates that all CSS classes used in Razor files exist in RR.Blazor styles
# Shows warnings for inline styles and non-existent classes

param(
    [string]$SolutionPath = ".",
    [string[]]$ProjectPaths = @(),
    [string]$StylesPath = "RR.Blazor/Styles",
    [string]$StylesDocPath = "RR.Blazor/wwwroot/rr-ai-styles.json",
    [switch]$ShowInlineStyles = $true,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🎯 Project Paths: $(if ($ProjectPaths.Count -gt 0) { $ProjectPaths -join ', ' } else { 'Auto-detect client projects' })" -ForegroundColor Yellow
Write-Host "📄 Styles Doc: $StylesDocPath" -ForegroundColor Yellow

# Load available classes from AI documentation
$availableClasses = @{}
if (Test-Path $StylesDocPath) {
    $stylesDoc = Get-Content $StylesDocPath | ConvertFrom-Json
    
    # Function to expand bracket notation patterns
    function ExpandBracketNotation($pattern) {
        # Handle prefix-[values] pattern (e.g., "pa-[0, 1, 2, 4]")
        if ($pattern -match '^([a-zA-Z-]+)-\[([^\]]+)\]$') {
            $prefix = $matches[1]
            $values = $matches[2] -split ',\s*'
            foreach ($value in $values) {
                "$prefix-$($value.Trim())"
            }
        }
        # Handle plain bracket pattern (e.g., "[block, flex, grid, hidden]")
        elseif ($pattern -match '^\[([^\]]+)\]$') {
            $values = $matches[1] -split ',\s*'
            foreach ($value in $values) {
                $value.Trim()
            }
        } 
        else {
            # Return as-is if not bracket notation
            $pattern
        }
    }
    
    if ($stylesDoc.PSObject.Properties.Name -contains "utility_patterns") {
        foreach ($category in $stylesDoc.utility_patterns.PSObject.Properties) {
            foreach ($pattern in $category.Value) {
                $expandedClasses = ExpandBracketNotation($pattern)
                foreach ($class in $expandedClasses) {
                    $availableClasses[$class] = "utility"
                }
            }
        }
    }
    
    Write-Host "📚 Loaded $($availableClasses.Count) utility classes from AI documentation (single source of truth)" -ForegroundColor Green
    
    if ($Verbose) {
        Write-Host "Sample classes loaded:" -ForegroundColor Gray
        $availableClasses.Keys | Sort-Object | Select-Object -First 20 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "❌ ERROR: Styles documentation not found at $StylesDocPath" -ForegroundColor Red
    Write-Host "   This file is the single source of truth for all CSS classes" -ForegroundColor Red
    Write-Host "   Run: pwsh ./RR.Blazor/Scripts/GenerateDocumentation.ps1 -ProjectPath ./RR.Blazor" -ForegroundColor Yellow
}

# Single source of truth: rr-ai-styles.json only (no SCSS fallback)
$allClasses = @{}
$availableClasses.Keys | ForEach-Object { $allClasses[$_] = "utility" }

# Find all code files (.razor, .cs, .js, .ts, .ch) based on project paths or auto-detection
if ($ProjectPaths.Count -gt 0) {
    # Use specific project paths provided by user
    $razorFiles = @()
    foreach ($projectPath in $ProjectPaths) {
        $fullProjectPath = if ([System.IO.Path]::IsPathRooted($projectPath)) {
            $projectPath
        } else {
            Join-Path $SolutionPath $projectPath
        }
        
        if (Test-Path $fullProjectPath) {
            $projectCodeFiles = Get-ChildItem -Path $fullProjectPath -Recurse -Include "*.razor", "*.cs", "*.js", "*.ts", "*.ch" | Where-Object { 
                $_.FullName -notlike "*node_modules*" -and 
                $_.FullName -notlike "*bin*" -and 
                $_.FullName -notlike "*obj*" -and 
                $_.FullName -notlike "*_backup*" -and 
                $_.FullName -notlike "*_Backup*"
            }
            $razorFiles += $projectCodeFiles
            Write-Host "  Found $($projectCodeFiles.Count) code files in: $projectPath" -ForegroundColor Gray
        } else {
            Write-Host "  ⚠️ Project path not found: $projectPath" -ForegroundColor Yellow
        }
    }
} else {
    # Auto-detect client projects - exclude server and RR modules except RR.Blazor
    $razorFiles = Get-ChildItem -Path $SolutionPath -Recurse -Include "*.razor", "*.cs", "*.js", "*.ts", "*.ch" | Where-Object { 
        $_.FullName -notlike "*node_modules*" -and 
        $_.FullName -notlike "*bin*" -and 
        $_.FullName -notlike "*obj*" -and 
        $_.FullName -notlike "*_backup*" -and 
        $_.FullName -notlike "*_Backup*" -and
        $_.FullName -notlike "*Server*" -and
        $_.FullName -notlike "*RR.Auth*" -and
        $_.FullName -notlike "*RR.AI*" -and
        $_.FullName -notlike "*RR.Core*" -and
        $_.FullName -notlike "*RR.MailService*" -and
        $_.FullName -notlike "*RR.Storage*" -and
        ($_.FullName -like "*RR.Blazor*" -or $_.FullName -notlike "*RR.*")
    }
}

Write-Host "🔍 Found $($razorFiles.Count) code files to analyze" -ForegroundColor Green

$issues = @()
$inlineStyleCount = 0
$totalClassUsages = 0

# Function to check if a class is valid
function IsValidClass($class) {
    # Skip empty classes
    if ([string]::IsNullOrWhiteSpace($class)) { return $true }
    
    # Skip Razor syntax
    if ($class -match '@|\$|\{|\}|\(|\)|==|!=|<=|>=|<|>|\?|\:|\||&') { return $true }
    
    # Skip pure numbers or variables
    if ($class -match '^\d+$') { return $true }
    
    # Skip classes that are clearly Razor variables or expressions
    if ($class -match '^[A-Z][a-zA-Z0-9]*$' -and $class -ne 'Class') { return $true }
    
    # Must be a valid CSS class name
    if ($class -notmatch '^[a-zA-Z][a-zA-Z0-9-_]*$') { return $true }
    
    # Check against AI documentation classes (single source of truth)
    $cleanClass = $class -replace '^(sm|md|lg|xl|xxl):', ''
    
    if ($allClasses.ContainsKey($cleanClass)) {
        return $true
    }
    
    # Pattern-based validation for common utility classes that should exist
    $commonPatterns = @(
        '^min-w-\d+$',          # min-w-200, min-w-80, etc.
        '^max-w-\d+$',          # max-w-400, etc.
        '^min-h-\d+$',          # min-h-100, min-h-200, etc.
        '^max-h-\d+$',          # max-h-300, max-h-500, etc.
        '^h-\d+$',              # h-10, h-60, h-400, etc.
        '^w-\d+$',              # w-150, etc.
        '^bg-opacity-\d+$',     # bg-opacity-50, bg-opacity-75, etc.
        '^progress-\d+$',       # progress-65, progress-85, etc.
        '^bg-(light|dark|elevated|secondary|danger|surface-secondary|white-\d+|bg-gradient-primary)$'  # Common background variants
    )
    
    foreach ($pattern in $commonPatterns) {
        if ($cleanClass -match $pattern) {
            return $true
        }
    }
    
    return $false
}

# Function to extract classes from class attribute
function ExtractClasses($classString) {
    $validClasses = @()
    
    # Handle conditional classes like @(condition ? "class1" : "class2")
    $conditionalMatches = [regex]::Matches($classString, '@\([^)]+\?\s*["'']([^"'']*?)["''][^)]*\)')
    foreach ($match in $conditionalMatches) {
        $classes = $match.Groups[1].Value -split '\s+' | Where-Object { $_ -ne '' }
        $validClasses += $classes
    }
    
    # Handle simple quoted classes
    $quotedMatches = [regex]::Matches($classString, '["'']([^"'']*?)["'']')
    foreach ($match in $quotedMatches) {
        $classes = $match.Groups[1].Value -split '\s+' | Where-Object { $_ -ne '' }
        $validClasses += $classes
    }
    
    # Handle space-separated classes (fallback)
    if ($validClasses.Count -eq 0) {
        $validClasses = $classString -split '\s+' | Where-Object { $_ -ne '' }
    }
    
    return $validClasses | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
}

foreach ($file in $razorFiles) {
    if ($Verbose) { Write-Host "📄 Analyzing: $($file.Name)" -ForegroundColor Gray }
    
    $content = Get-Content $file.FullName -Raw
    $relativePath = $file.FullName.Replace($PWD.Path, "").TrimStart('\', '/')
    
    # Check if this is a test file (be more lenient with validation)
    $isTestFile = $relativePath -match '(Test_|Tests?/|\.Test\.)'
    
    # Check for inline styles
    if ($ShowInlineStyles) {
        $inlineStyleMatches = [regex]::Matches($content, 'style\s*=\s*["'']([^"'']*?)["'']')
        foreach ($match in $inlineStyleMatches) {
            $inlineStyleCount++
            $issues += [PSCustomObject]@{
                File = $relativePath
                Line = ($content.Substring(0, $match.Index) -split "`n").Length
                Type = "InlineStyle"
                Issue = "Inline styles discouraged"
                Content = $match.Groups[1].Value
                Suggestion = "Use utility classes instead"
            }
        }
    }
    
    # Extract class usages with better Razor syntax handling
    $classMatches = [regex]::Matches($content, 'class\s*=\s*["'']([^"'']*?)["'']')
    foreach ($match in $classMatches) {
        $classString = $match.Groups[1].Value
        $classes = ExtractClasses($classString)
        
        foreach ($class in $classes) {
            $totalClassUsages++
            
            if (-not (IsValidClass($class))) {
                # Skip missing class warnings for test files unless it's a critical issue
                if (-not $isTestFile -or $class -match '^[A-Z]') {
                    $issues += [PSCustomObject]@{
                        File = $relativePath
                        Line = ($content.Substring(0, $match.Index) -split "`n").Length
                        Type = "MissingClass"
                        Issue = "Class not found in RR.Blazor AI documentation"
                        Content = $class
                        Suggestion = "Check spelling or add class to RR.Blazor system"
                    }
                }
            }
        }
    }
    
    # Extract class strings from C# code blocks and expressions
    # Pattern 1: String literals containing CSS classes in @code blocks
    $codeBlockPattern = '(?s)@code\s*\{.*?\}'
    $codeBlockMatches = [regex]::Matches($content, $codeBlockPattern)
    
    foreach ($codeBlockMatch in $codeBlockMatches) {
        $codeContent = $codeBlockMatch.Value
        
        # Look for string literals that likely contain CSS classes
        $stringLiteralPattern = '"([^"]*(?:\s+[a-z][a-z0-9-]*){1,}[^"]*)"'
        $stringMatches = [regex]::Matches($codeContent, $stringLiteralPattern)
        
        foreach ($stringMatch in $stringMatches) {
            $stringValue = $stringMatch.Groups[1].Value
            
            # Skip strings that are clearly not CSS classes
            if ($stringValue -match '^\s*$|^[A-Z]|@|{|}|\(|\)|==|!=|<=|>=|<|>|\?|:|\||&|\\|/|mailto:|http:|https:|javascript:|Error|Failed|Invalid|Processing|Loading|Completed|Successfully') {
                continue
            }
            
            # Skip strings that contain common non-CSS words
            $nonCssPatterns = @(
                'data', 'user', 'error', 'message', 'failed', 'success', 'loading', 'complete',
                'api', 'endpoint', 'query', 'response', 'request', 'method', 'function',
                'database', 'table', 'column', 'field', 'record', 'entity',
                'input', 'output', 'result', 'value', 'parameter', 'argument',
                'the', 'and', 'or', 'with', 'from', 'to', 'in', 'on', 'at', 'by',
                'debug', 'info', 'warn', 'trace', 'log', 'console'
            )
            
            $containsNonCssWords = $false
            foreach ($pattern in $nonCssPatterns) {
                if ($stringValue -match "\\b$pattern\\b") {
                    $containsNonCssWords = $true
                    break
                }
            }
            
            if ($containsNonCssWords) {
                continue
            }
            
            # Extract potential CSS classes from the string
            $potentialClasses = $stringValue -split '\s+' | Where-Object { 
                $_ -ne '' -and 
                $_ -match '^[a-z][a-z0-9-]*$' -and
                $_ -notmatch '^\d+$' -and
                $_.Length -le 50 -and  # Reasonable max length for CSS classes
                $_ -notmatch '^(mailto|http|https|ftp|file|javascript)' # Protocol prefixes
            }
            
            foreach ($class in $potentialClasses) {
                $totalClassUsages++
                
                if (-not (IsValidClass($class))) {
                    $lineNumber = ($content.Substring(0, $codeBlockMatch.Index + $stringMatch.Index) -split "`n").Length
                    $issues += [PSCustomObject]@{
                        File = $relativePath
                        Line = $lineNumber
                        Type = "MissingClass"
                        Issue = "Class not found in RR.Blazor AI documentation (found in C# code)"
                        Content = $class
                        Suggestion = "Check spelling or add class to RR.Blazor system"
                    }
                }
            }
        }
    }
    
    # Pattern 2: String literals in C# expressions (outside @code blocks)
    $csharpExpressionPattern = '@\([^)]+\)|@[a-zA-Z_][a-zA-Z0-9_]*\([^)]*\)|@\{[^}]*\}'
    $expressionMatches = [regex]::Matches($content, $csharpExpressionPattern)
    
    foreach ($expressionMatch in $expressionMatches) {
        $expressionContent = $expressionMatch.Value
        
        # Look for string literals within expressions
        $stringLiteralPattern = '"([^"]*(?:\s+[a-z][a-z0-9-]*){1,}[^"]*)"'
        $stringMatches = [regex]::Matches($expressionContent, $stringLiteralPattern)
        
        foreach ($stringMatch in $stringMatches) {
            $stringValue = $stringMatch.Groups[1].Value
            
            # Skip strings that are clearly not CSS classes
            if ($stringValue -match '^\s*$|^[A-Z]|@|{|}|\(|\)|==|!=|<=|>=|<|>|\?|:|\||&|\\|/|mailto:|http:|https:|javascript:|Error|Failed|Invalid|Processing|Loading|Completed|Successfully') {
                continue
            }
            
            # Skip strings that contain common non-CSS words
            $nonCssPatterns = @(
                'data', 'user', 'error', 'message', 'failed', 'success', 'loading', 'complete',
                'api', 'endpoint', 'query', 'response', 'request', 'method', 'function',
                'database', 'table', 'column', 'field', 'record', 'entity',
                'input', 'output', 'result', 'value', 'parameter', 'argument',
                'the', 'and', 'or', 'with', 'from', 'to', 'in', 'on', 'at', 'by',
                'debug', 'info', 'warn', 'trace', 'log', 'console'
            )
            
            $containsNonCssWords = $false
            foreach ($pattern in $nonCssPatterns) {
                if ($stringValue -match "\\b$pattern\\b") {
                    $containsNonCssWords = $true
                    break
                }
            }
            
            if ($containsNonCssWords) {
                continue
            }
            
            # Extract potential CSS classes from the string
            $potentialClasses = $stringValue -split '\s+' | Where-Object { 
                $_ -ne '' -and 
                $_ -match '^[a-z][a-z0-9-]*$' -and
                $_ -notmatch '^\d+$' -and
                $_.Length -le 50 -and  # Reasonable max length for CSS classes
                $_ -notmatch '^(mailto|http|https|ftp|file|javascript)' # Protocol prefixes
            }
            
            foreach ($class in $potentialClasses) {
                $totalClassUsages++
                
                if (-not (IsValidClass($class))) {
                    $lineNumber = ($content.Substring(0, $expressionMatch.Index + $stringMatch.Index) -split "`n").Length
                    $issues += [PSCustomObject]@{
                        File = $relativePath
                        Line = $lineNumber
                        Type = "MissingClass"
                        Issue = "Class not found in RR.Blazor AI documentation (found in C# expression)"
                        Content = $class
                        Suggestion = "Check spelling or add class to RR.Blazor system"
                    }
                }
            }
        }
    }
    
    # Check for old BEM patterns that should be updated
    $bemMatches = [regex]::Matches($content, 'class\s*=\s*["''][^"'']*__[^"'']*["'']')
    foreach ($match in $bemMatches) {
        $issues += [PSCustomObject]@{
            File = $relativePath
            Line = ($content.Substring(0, $match.Index) -split "`n").Length
            Type = "BEMPattern"
            Issue = "BEM notation found, should use functional naming"
            Content = $match.Groups[0].Value
            Suggestion = "Convert to functional naming (e.g., card__primary → card-primary)"
        }
    }
    
    # Check for deprecated utility patterns
    $deprecatedPatterns = @{
        '\belevation-(\d+)\b' = 'Use shadow-$1'
    }
    
    foreach ($pattern in $deprecatedPatterns.Keys) {
        $deprecatedMatches = [regex]::Matches($content, $pattern)
        foreach ($match in $deprecatedMatches) {
            $issues += [PSCustomObject]@{
                File = $relativePath
                Line = ($content.Substring(0, $match.Index) -split "`n").Length
                Type = "DeprecatedPattern"
                Issue = "Deprecated utility pattern"
                Content = $match.Groups[0].Value
                Suggestion = $deprecatedPatterns[$pattern]
            }
        }
    }
}

# Generate report
Write-Host "  Total issues found: $($issues.Count)" -ForegroundColor White

Write-Host "  Total Razor files: $($razorFiles.Count)" -ForegroundColor White
Write-Host "  Total class usages: $totalClassUsages" -ForegroundColor White

if ($ShowInlineStyles) {
    Write-Host "  Inline styles found: $inlineStyleCount" -ForegroundColor White
}

# Group issues by type
$issuesByType = $issues | Group-Object Type
foreach ($group in $issuesByType) {
    Write-Host "  $($group.Name): $($group.Count)" -ForegroundColor Yellow
}

# Show detailed issues (limit to 50 for readability)
if ($issues.Count -gt 0) {
    Write-Host "`n🔍 Detailed Issues (first 50):" -ForegroundColor Red
    
    $displayIssues = $issues | Sort-Object Type, File, Line | Select-Object -First 50
    foreach ($issue in $displayIssues) {
        $typeColor = switch ($issue.Type) {
            "InlineStyle" { "Yellow" }
            "MissingClass" { "Red" }
            "BEMPattern" { "Magenta" }
            "DeprecatedPattern" { "Cyan" }
            default { "White" }
        }
        
        Write-Host "  [$($issue.Type)]" -ForegroundColor $typeColor -NoNewline
        Write-Host " $($issue.File):$($issue.Line)" -ForegroundColor Gray
        Write-Host "    Issue: $($issue.Issue)" -ForegroundColor White
        Write-Host "    Found: $($issue.Content)" -ForegroundColor Gray
        Write-Host "    Suggestion: $($issue.Suggestion)" -ForegroundColor Green
        Write-Host ""
    }
    
    if ($issues.Count -gt 50) {
        Write-Host "  ... and $($issues.Count - 50) more issues in the JSON report." -ForegroundColor Gray
    }
}

# Generate JSON report for build integration
$report = @{
    Timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    TotalFiles = $razorFiles.Count
    TotalClassUsages = $totalClassUsages
    TotalIssues = $issues.Count
    InlineStyleCount = $inlineStyleCount
    IssuesByType = ($issuesByType | ForEach-Object { @{ Type = $_.Name; Count = $_.Count } })
    Issues = $issues
}

$reportPath = "class-validation-report.json"
$report | ConvertTo-Json -Depth 10 | Set-Content $reportPath
Write-Host "📄 JSON report saved to: $reportPath" -ForegroundColor Cyan

# Exit with appropriate code for build integration
if ($issues.Count -gt 0) {
    Write-Host "⚠️ Validation completed with issues. Review the report above." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✅ All class usages are valid!" -ForegroundColor Green
    exit 0
}