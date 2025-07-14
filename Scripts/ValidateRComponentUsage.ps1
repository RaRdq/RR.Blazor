#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates R* component parameter usage against actual component definitions
    
.DESCRIPTION
    Uses rr-ai-docs.json to get accurate parameter lists for all R* components,
    then scans consuming projects to find parameter violations.
    
.PARAMETER SolutionPath
    Path to the solution directory to scan all projects
    
.PARAMETER AIDocsPath
    Path to the rr-ai-docs.json file
    
.PARAMETER Fix
    Automatically fix violations by removing invalid parameters
    
.EXAMPLE
    ./ValidateRComponentUsage.ps1 -SolutionPath "." -AIDocsPath "./RR.Blazor/wwwroot/rr-ai-docs.json"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SolutionPath,
    
    [Parameter(Mandatory = $true)]
    [string]$AIDocsPath,
    
    [Parameter(Mandatory = $false)]
    [switch]$Fix = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Validating R* Component Usage..." -ForegroundColor Cyan

# Load AI docs to get actual component parameters
if (-not (Test-Path $AIDocsPath)) {
    throw "AI docs not found at: $AIDocsPath"
}

$aiDocs = Get-Content $AIDocsPath -Raw | ConvertFrom-Json
$componentParameters = @{}

# Build component parameter lookup table
foreach ($componentName in $aiDocs.components.PSObject.Properties.Name) {
    if ($componentName -match '^R[A-Z]') {
        $component = $aiDocs.components.$componentName
        $validParams = @($component.parameters.PSObject.Properties.Name)
        $componentParameters[$componentName] = $validParams
        
        Write-Host "  📄 $componentName has $($validParams.Count) parameters: $($validParams -join ', ')" -ForegroundColor DarkGray
    }
}

Write-Host "📂 Scanning solution for R* component usage..." -ForegroundColor Yellow

# Find all .razor files in solution, excluding RR.Blazor itself to avoid false positives
$razorFiles = Get-ChildItem -Path $SolutionPath -Filter "*.razor" -Recurse | Where-Object { 
    $_.FullName -notlike "*RR.Blazor*" 
}
$violations = @()

foreach ($file in $razorFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($SolutionPath, '').TrimStart('\', '/')
    
    # Find all R* component usages (including multiline)
    $componentPattern = '(?s)<(R[A-Z]\w*)\s+([^>]*?)/?>'
    $componentMatches = [regex]::Matches($content, $componentPattern)
    
    foreach ($match in $componentMatches) {
        $componentName = $match.Groups[1].Value
        $attributesSection = $match.Groups[2].Value
        
        # Skip if we don't have parameter info for this component
        if (-not $componentParameters.ContainsKey($componentName)) {
            continue
        }
        
        $validParams = $componentParameters[$componentName]
        
        # Extract all parameters from the attributes section
        $paramPattern = '(\w+)\s*=\s*"[^"]*"'
        $paramMatches = [regex]::Matches($attributesSection, $paramPattern)
        
        foreach ($paramMatch in $paramMatches) {
            $paramName = $paramMatch.Groups[1].Value
            
            # Skip common attributes that aren't component parameters
            if ($paramName -in @('class', 'style', 'id', 'onclick', 'onchange', 'bind', 'ref')) {
                continue
            }
            
            # Check if this parameter is valid for this component
            if ($paramName -notin $validParams) {
                $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                
                $violation = @{
                    File = $relativePath
                    FullPath = $file.FullName
                    LineNumber = $lineNumber
                    Component = $componentName
                    InvalidParameter = $paramName
                    ValidParameters = $validParams -join ', '
                    MatchedText = $match.Value
                }
                
                $violations += $violation
                
                Write-Host "  ❌ ${relativePath}:${lineNumber} - $componentName does not support parameter '$paramName'" -ForegroundColor Red
                Write-Host "     Valid parameters: $($validParams -join ', ')" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host "`n📊 Validation Results:" -ForegroundColor Cyan
Write-Host "  Total violations found: $($violations.Count)" -ForegroundColor White

if ($violations.Count -eq 0) {
    Write-Host "  ✅ No parameter violations found!" -ForegroundColor Green
    return @{ Success = $true; Violations = @() }
}

# Group violations by component type
$violationsByComponent = $violations | Group-Object Component

foreach ($group in $violationsByComponent) {
    $componentName = $group.Name
    $count = $group.Count
    $invalidParams = ($group.Group | Select-Object -ExpandProperty InvalidParameter | Sort-Object -Unique) -join ', '
    
    Write-Host "  🔴 ${componentName}: $count violations" -ForegroundColor Red
    Write-Host "     Invalid parameters used: $invalidParams" -ForegroundColor Yellow
}

if ($Fix) {
    Write-Host "`n🔧 Fixing violations..." -ForegroundColor Yellow
    
    $fixedFiles = @{}
    
    foreach ($violation in $violations) {
        $filePath = $violation.FullPath
        
        if (-not $fixedFiles.ContainsKey($filePath)) {
            $fixedFiles[$filePath] = Get-Content $filePath -Raw -Encoding UTF8
        }
        
        $content = $fixedFiles[$filePath]
        
        # Remove the invalid parameter from the component usage
        $paramToRemove = $violation.InvalidParameter
        $componentName = $violation.Component
        
        # Pattern to match the specific parameter and remove it
        $removeParamPattern = "(\s+$paramToRemove\s*=\s*`"[^`"]*`")"
        $content = $content -replace $removeParamPattern, ''
        
        # Also handle case where parameter might be on its own line
        $removeParamLinePattern = "(?m)^\s*$paramToRemove\s*=\s*`"[^`"]*`"\s*$"
        $content = $content -replace $removeParamLinePattern, ''
        
        $fixedFiles[$filePath] = $content
        
        Write-Host "  ✏️ Removed $paramToRemove from $componentName in $($violation.File)" -ForegroundColor Green
    }
    
    # Write fixed files back
    foreach ($filePath in $fixedFiles.Keys) {
        Set-Content -Path $filePath -Value $fixedFiles[$filePath] -Encoding UTF8 -NoNewline
    }
    
    Write-Host "  ✅ Fixed $($fixedFiles.Count) files" -ForegroundColor Green
}

return @{
    Success = $violations.Count -eq 0
    Violations = $violations
    ViolationCount = $violations.Count
}