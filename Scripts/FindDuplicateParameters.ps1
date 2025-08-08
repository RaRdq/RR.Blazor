#!/usr/bin/env pwsh

# Find Duplicate Parameters and Inheritance Issues
# ================================================

param(
    [string]$ComponentsPath = "./RR.Blazor/Components",
    [switch]$Fix
)

Write-Host "🔍 SCANNING FOR DUPLICATE PARAMETERS AND INHERITANCE ISSUES" -ForegroundColor Cyan
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host ""

$duplicates = @()
$missingInheritance = @()
$logicAmbiguity = @()

# Define base class parameters for conflict detection
$baseClassParams = @{
    'RComponentBase' = @('Class', 'Style', 'Disabled', 'ChildContent', 'Density', 'FullWidth', 'Elevation', 'AdditionalAttributes')
    'RInputBase' = @('Label', 'Placeholder', 'HelpText', 'FieldName', 'Required', 'ReadOnly', 'Loading', 'HasError', 'ErrorMessage', 'MaxLength', 'Immediate', 'ImmediateDebounce', 'OnInput', 'OnChange', 'OnTextInput', 'OnTextChanged', 'OnFocus', 'OnBlur', 'OnKeyPress', 'OnKeyDown', 'OnStartIconClick', 'OnEndIconClick', 'Variant', 'Size', 'StartIcon', 'EndIcon')
    'RInteractiveComponentBase' = @('OnClick', 'OnDoubleClick', 'OnMouseEnter', 'OnMouseLeave', 'OnKeyDown', 'OnKeyUp', 'OnKeyPress')
    'RVariantComponentBase' = @('Variant', 'Size')
}

# Get all .razor files
$razorFiles = Get-ChildItem -Path $ComponentsPath -Filter "R*.razor" -Recurse

Write-Host "📂 Analyzing $($razorFiles.Count) R* components..." -ForegroundColor DarkGray
Write-Host ""

foreach ($file in $razorFiles) {
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    # Skip if content is empty
    if ([string]::IsNullOrWhiteSpace($content)) {
        continue
    }
    
    # Check inheritance
    $inheritance = @()
    if ($content -match '@inherits\s+(\w+)') {
        $inheritance += $Matches[1]
    }
    
    # Find all parameters in this component
    $componentParams = @()
    if ($content -match '@code\s*\{([\s\S]*)\}[\s\S]*$') {
        $codeSection = $Matches[1]
        
        # Extract parameters using regex
        $paramMatches = [regex]::Matches($codeSection, '\[Parameter\][^\[]*?public\s+[^{]+?\s+(\w+)\s*\{')
        foreach ($match in $paramMatches) {
            $paramName = $match.Groups[1].Value
            if ($paramName -and $paramName -notmatch '^(get|set)$') {
                $componentParams += $paramName
            }
        }
    }
    
    # Check for conflicts with base classes
    foreach ($baseClass in $inheritance) {
        if ($baseClassParams.ContainsKey($baseClass)) {
            $baseParams = $baseClassParams[$baseClass]
            
            # If inheriting from RComponentBase, also include its parameters
            if ($baseClass -ne 'RComponentBase' -and $baseClassParams.ContainsKey('RComponentBase')) {
                $baseParams += $baseClassParams['RComponentBase']
            }
            
            foreach ($param in $componentParams) {
                if ($param -in $baseParams) {
                    $duplicates += [PSCustomObject]@{
                        File = $file.FullName.Replace((Get-Location).Path, '').TrimStart('\', '/')
                        Component = $componentName
                        Parameter = $param
                        BaseClass = $baseClass
                        Severity = 'ERROR'
                        Issue = "Duplicate parameter '$param' - already provided by $baseClass"
                    }
                }
            }
        }
    }
    
    # Check for missing inheritance patterns
    $needsRComponentBase = $false
    $hasBaseClasses = $inheritance.Count -gt 0
    
    # Check if component declares base parameters without inheritance
    if (-not $hasBaseClasses) {
        foreach ($param in $componentParams) {
            if ($param -in $baseClassParams['RComponentBase']) {
                $needsRComponentBase = $true
                break
            }
        }
        
        if ($needsRComponentBase) {
            $missingInheritance += [PSCustomObject]@{
                File = $file.FullName.Replace((Get-Location).Path, '').TrimStart('\', '/')
                Component = $componentName
                MissingBase = 'RComponentBase'
                Parameters = ($componentParams | Where-Object { $_ -in $baseClassParams['RComponentBase'] }) -join ', '
                Severity = 'WARNING'
                Issue = "Should inherit from RComponentBase - declares: $($componentParams | Where-Object { $_ -in $baseClassParams['RComponentBase'] } | Select-Object -First 3 | Join-String -Separator ', ')..."
            }
        }
    }
    
    # Check for logic ambiguity (specific cases)
    if ($componentName -eq 'RTextInput' -and 'Name' -in $componentParams -and 'FieldName' -in $componentParams) {
        $logicAmbiguity += [PSCustomObject]@{
            File = $file.FullName.Replace((Get-Location).Path, '').TrimStart('\', '/')
            Component = $componentName
            Ambiguity = 'Name vs FieldName'
            Severity = 'WARNING'
            Issue = "Has both 'Name' and 'FieldName' parameters - logic ambiguity"
        }
    }
}

# Report findings
Write-Host "🚨 DUPLICATE PARAMETER CONFLICTS:" -ForegroundColor Red
Write-Host "====================================" -ForegroundColor Red

if ($duplicates.Count -eq 0) {
    Write-Host "✅ No duplicate parameter conflicts found" -ForegroundColor Green
} else {
    $duplicates | Sort-Object Component, Parameter | ForEach-Object {
        Write-Host "  ❌ $($_.Component) ($($_.File))" -ForegroundColor Red
        Write-Host "     Parameter: '$($_.Parameter)' conflicts with $($_.BaseClass)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "⚠️  MISSING INHERITANCE:" -ForegroundColor Yellow
Write-Host "=========================" -ForegroundColor Yellow

if ($missingInheritance.Count -eq 0) {
    Write-Host "✅ No missing inheritance issues found" -ForegroundColor Green
} else {
    $missingInheritance | Sort-Object Component | ForEach-Object {
        Write-Host "  ⚠️  $($_.Component) ($($_.File))" -ForegroundColor Yellow
        Write-Host "     Should inherit from: $($_.MissingBase)" -ForegroundColor Gray
        Write-Host "     Declaring: $($_.Parameters)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "🤔 LOGIC AMBIGUITY:" -ForegroundColor Magenta
Write-Host "===================" -ForegroundColor Magenta

if ($logicAmbiguity.Count -eq 0) {
    Write-Host "✅ No logic ambiguity issues found" -ForegroundColor Green
} else {
    $logicAmbiguity | Sort-Object Component | ForEach-Object {
        Write-Host "  🤔 $($_.Component) ($($_.File))" -ForegroundColor Magenta
        Write-Host "     Issue: $($_.Issue)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "📊 SUMMARY:" -ForegroundColor Cyan
Write-Host "===========" -ForegroundColor Cyan
Write-Host "  Duplicate Parameters: $($duplicates.Count)" -ForegroundColor $(if ($duplicates.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "  Missing Inheritance: $($missingInheritance.Count)" -ForegroundColor $(if ($missingInheritance.Count -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "  Logic Ambiguity: $($logicAmbiguity.Count)" -ForegroundColor $(if ($logicAmbiguity.Count -eq 0) { 'Green' } else { 'Magenta' })

$totalIssues = $duplicates.Count + $missingInheritance.Count + $logicAmbiguity.Count
Write-Host "  Total Issues: $totalIssues" -ForegroundColor $(if ($totalIssues -eq 0) { 'Green' } else { 'Red' })

if ($Fix -and $totalIssues -gt 0) {
    Write-Host ""
    Write-Host "🔧 FIXING ISSUES..." -ForegroundColor Blue
    Write-Host "===================" -ForegroundColor Blue
    
    # TODO: Implement fixes
    Write-Host "⚠️  Fix functionality not implemented yet - manual fixes required" -ForegroundColor Yellow
}

Write-Host ""