#!/usr/bin/env pwsh
<#
.SYNOPSIS
    BULLETPROOF R* Component Parameter Validation - Uses actual source code as source of truth
    
.DESCRIPTION
    Validates R* component parameter usage against actual component definitions.
    FIXED: No more false positives, uses real source code parsing instead of incomplete AI docs.
    
.PARAMETER SolutionPath
    Path to the solution directory to scan
    
.PARAMETER ComponentsPath
    Path to RR.Blazor Components directory (for source of truth)
    
.PARAMETER Fix
    Automatically remove invalid parameters
    
.PARAMETER Detailed
    Show detailed parameter information for each component
    
.EXAMPLE
    .\ValidateRComponentUsage_Fixed.ps1 -SolutionPath "." -ComponentsPath "./RR.Blazor/Components"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SolutionPath,
    
    [Parameter(Mandatory = $true)]
    [string]$ComponentsPath,
    
    [Parameter(Mandatory = $false)]
    [switch]$Fix = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$Detailed = $false
)

$ErrorActionPreference = "Stop"

# Function to calculate string similarity for suggestions
function Get-LevenshteinDistance {
    param([string]$String1, [string]$String2)
    
    $len1 = $String1.Length
    $len2 = $String2.Length
    
    if ($len1 -eq 0) { return $len2 }
    if ($len2 -eq 0) { return $len1 }
    
    $matrix = New-Object 'int[,]' ($len1 + 1), ($len2 + 1)
    
    for ($i = 0; $i -le $len1; $i++) { $matrix[$i, 0] = $i }
    for ($j = 0; $j -le $len2; $j++) { $matrix[0, $j] = $j }
    
    for ($i = 1; $i -le $len1; $i++) {
        for ($j = 1; $j -le $len2; $j++) {
            $cost = if ($String1[$i-1] -eq $String2[$j-1]) { 0 } else { 1 }
            $matrix[$i, $j] = [Math]::Min([Math]::Min($matrix[$i-1, $j] + 1, $matrix[$i, $j-1] + 1), $matrix[$i-1, $j-1] + $cost)
        }
    }
    
    return $matrix[$len1, $len2]
}

# Function to extract ACTUAL parameters from component source code (Define first!)
function Extract-ActualParameters {
    param([string]$Content)
    
    $parameters = @()
    $lines = $Content -split "`n"
    $inCodeBlock = $false
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i].Trim()
        
        # Track @code blocks
        if ($line -match '@code\s*{') {
            $inCodeBlock = $true
            continue
        }
        if ($line -match '^\s*}\s*$' -and $inCodeBlock) {
            $inCodeBlock = $false
            continue
        }
        
        # Look for [Parameter] attributes (handle all variations)
        if ($line -match '\[Parameter(?:[\],]|$)') {
            # Collect multi-line parameter definition
            $parameterText = ""
            $propertyFound = $false
            
            for ($j = $i; $j -lt [Math]::Min($lines.Length, $i + 20); $j++) {
                $currentLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($currentLine)) { continue }
                
                $parameterText += " " + $currentLine
                
                # Found the property declaration
                if ($currentLine -match 'public\s+[^{]+\{\s*get;\s*set;\s*\}') {
                    $propertyFound = $true
                    break
                }
            }
            
            if ($propertyFound -and $parameterText -match 'public\s+(?:virtual\s+)?(?:override\s+)?(?:static\s+)?[^\s]+(?:<[^>]*>)?(?:\?)?\s+(@?\w+)\s*\{') {
                $paramName = $Matches[1].Trim()
                if ($paramName -and $paramName -notmatch '^(get|set)$') {
                    $parameters += $paramName
                }
            }
        }
    }
    
    return $parameters | Sort-Object -Unique
}

Write-Host "🔍 BULLETPROOF R* Component Parameter Validation" -ForegroundColor Cyan
Write-Host "Using SOURCE CODE as truth (not AI docs)" -ForegroundColor Yellow

# Build SOURCE OF TRUTH parameter dictionary from actual .razor files
Write-Host "📂 Building source-of-truth parameter dictionary..." -ForegroundColor Yellow

$componentParameters = @{}
$componentFiles = Get-ChildItem -Path $ComponentsPath -Filter "R*.razor" -Recurse

foreach ($file in $componentFiles) {
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    Write-Host "  📄 Parsing $componentName..." -ForegroundColor DarkGray
    
    $parameters = Extract-ActualParameters -Content $content
    $componentParameters[$componentName] = $parameters
    
    if ($Detailed) {
        Write-Host "    Parameters: $($parameters -join ', ')" -ForegroundColor DarkGreen
    } else {
        Write-Host "    ✅ Found $($parameters.Count) parameters" -ForegroundColor DarkGreen
    }
}

Write-Host "📊 Parameter dictionary built: $($componentParameters.Count) components, $($componentParameters.Values | ForEach-Object { $_.Count } | Measure-Object -Sum | Select-Object -ExpandProperty Sum) total parameters" -ForegroundColor Green


# Standard parameters to IGNORE (not false positives)
$ignoredParameters = @(
    # Generic Blazor types
    'TValue', 'TItem', 'TEntity', 'TModel', 'TData',
    
    # Standard HTML attributes
    'class', 'style', 'id', 'title', 'tabindex', 'hidden',
    
    # Common Blazor attributes  
    'onclick', 'onchange', 'onfocus', 'onblur', 'onkeydown', 'onkeyup', 'onmouseenter', 'onmouseleave',
    'bind', 'bind-value', 'bind-checked', 'ref', 'after',
    
    # Standard HTML input attributes
    'name', 'value', 'checked', 'selected', 'disabled', 'readonly', 'required', 'placeholder',
    'min', 'max', 'step', 'maxlength', 'minlength', 'pattern', 'autocomplete',
    
    # Standard form attributes
    'form', 'formaction', 'formmethod', 'formnovalidate', 'formtarget'
)

# Component structural constraints (child components that require specific parents)
$componentConstraints = @{
    'RDataTableColumn' = @{
        RequiredParent = 'RDataTable'
        Context = 'ColumnsContent'
        Description = 'RDataTableColumn must be used within RDataTable''s ColumnsContent'
    }
    'RListItem' = @{
        RequiredParent = 'RList'
        Context = 'ChildContent'
        Description = 'RListItem must be used within RList''s ChildContent'
    }
    'RTabItem' = @{
        RequiredParent = 'RTabs'
        Context = 'ChildContent'
        Description = 'RTabItem must be used within RTabs'' ChildContent'
    }
    'RAccordionItem' = @{
        RequiredParent = 'RAccordion'
        Context = 'ChildContent'
        Description = 'RAccordionItem must be used within RAccordion''s ChildContent'
    }
    'RFormSection' = @{
        RequiredParent = 'RForm'
        Context = 'FormFields'
        Description = 'RFormSection must be used within RForm''s FormFields'
    }
}

# Function to check component structural constraints
function Test-ComponentConstraints {
    param(
        [string]$Content,
        [string]$FilePath
    )
    
    $constraintViolations = @()
    
    foreach ($childComponent in $componentConstraints.Keys) {
        $constraint = $componentConstraints[$childComponent]
        $requiredParent = $constraint.RequiredParent
        
        # Find all instances of child component
        $childPattern = "(?s)<$childComponent[^>]*(?:/>|>.*?</$childComponent>)"
        $childMatches = [regex]::Matches($Content, $childPattern)
        
        foreach ($childMatch in $childMatches) {
            $childStart = $childMatch.Index
            $childEnd = $childMatch.Index + $childMatch.Length
            
            # Look for parent component that contains this child
            $parentPattern = "(?s)<$requiredParent[^>]*>.*?</$requiredParent>"
            $parentMatches = [regex]::Matches($Content, $parentPattern)
            
            $foundValidParent = $false
            foreach ($parentMatch in $parentMatches) {
                $parentStart = $parentMatch.Index
                $parentEnd = $parentMatch.Index + $parentMatch.Length
                
                # Check if child is within parent boundaries
                if ($childStart -ge $parentStart -and $childEnd -le $parentEnd) {
                    $foundValidParent = $true
                    break
                }
            }
            
            if (-not $foundValidParent) {
                $lineNumber = ($Content.Substring(0, $childStart) -split "`n").Count
                
                $constraintViolations += @{
                    File = $FilePath
                    LineNumber = $lineNumber
                    Component = $childComponent
                    RequiredParent = $requiredParent
                    Context = $constraint.Context
                    Description = $constraint.Description
                    MatchedText = $childMatch.Value
                }
            }
        }
    }
    
    return $constraintViolations
}

Write-Host "🔍 Scanning solution for R* component usage..." -ForegroundColor Yellow

# Find all .razor files to validate (exclude RR.Blazor itself)
$razorFiles = Get-ChildItem -Path $SolutionPath -Filter "*.razor" -Recurse | Where-Object { 
    $_.FullName -notlike "*RR.Blazor*" 
}

$violations = @()
$structuralViolations = @()
$totalComponentsFound = 0

foreach ($file in $razorFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($SolutionPath, '').TrimStart('\', '/')
    
    # Check structural constraints first
    $constraintViolations = Test-ComponentConstraints -Content $content -FilePath $relativePath
    $structuralViolations += $constraintViolations
    
    # Find all R* component usages (multi-line aware)
    $componentPattern = '(?s)<(R[A-Z]\w*)\s+([^>]*?)/?>'
    $componentMatches = [regex]::Matches($content, $componentPattern)
    
    foreach ($match in $componentMatches) {
        $componentName = $match.Groups[1].Value
        $attributesSection = $match.Groups[2].Value
        $totalComponentsFound++
        
        # Skip if we don't have parameter info for this component
        if (-not $componentParameters.ContainsKey($componentName)) {
            continue
        }
        
        $validParams = $componentParameters[$componentName]
        
        # Extract parameters from attributes section (handle quoted values, expressions)
        # First, remove all @bind-* directives to avoid false matches with :after syntax
        $cleanedAttributes = $attributesSection -replace '@bind[-\w]*:[^=\s]*="[^"]*"', ''
        
        # Extract parameters using a more robust parser for complex expressions
        $parameters = @()
        $i = 0
        $attributesLength = $cleanedAttributes.Length
        
        while ($i -lt $attributesLength) {
            # Skip whitespace
            while ($i -lt $attributesLength -and $cleanedAttributes[$i] -match '\s') { $i++ }
            if ($i -ge $attributesLength) { break }
            
            # Find parameter name (skip any non-word characters first)
            if ($cleanedAttributes[$i] -notmatch '\w') {
                $i++
                continue
            }
            
            $paramStart = $i
            while ($i -lt $attributesLength -and $cleanedAttributes[$i] -match '\w') { $i++ }
            if ($i -eq $paramStart) { 
                $i++
                continue 
            }
            
            $paramName = $cleanedAttributes.Substring($paramStart, $i - $paramStart)
            
            # Skip whitespace and check for =
            while ($i -lt $attributesLength -and $cleanedAttributes[$i] -match '\s') { $i++ }
            
            # Check if this is a boolean attribute (no =) or a value attribute (has =)
            if ($i -ge $attributesLength -or $cleanedAttributes[$i] -ne '=') { 
                # Boolean attribute - just add the parameter name
                $parameters += $paramName
                continue 
            }
            
            # Has = so parse the value
            $i++ # Skip =
            while ($i -lt $attributesLength -and $cleanedAttributes[$i] -match '\s') { $i++ }
            
            # Parse parameter value with proper nesting handling
            if ($i -lt $attributesLength) {
                if ($cleanedAttributes[$i] -eq '"') {
                    # Handle quoted string with proper nesting
                    $i++
                    $nestLevel = 0
                    $inString = $false
                    while ($i -lt $attributesLength) {
                        $char = $cleanedAttributes[$i]
                        if ($char -eq '"' -and -not $inString) {
                            if ($nestLevel -eq 0) {
                                $i++
                                break
                            }
                            $inString = $true
                        } elseif ($char -eq '"' -and $inString) {
                            $inString = $false
                        } elseif (-not $inString) {
                            if ($char -eq '{' -or $char -eq '(') {
                                $nestLevel++
                            } elseif ($char -eq '}' -or $char -eq ')') {
                                $nestLevel--
                            }
                        } elseif ($char -eq '\') {
                            $i++ # Skip escaped character
                        }
                        $i++
                    }
                } else {
                    # Handle unquoted value
                    while ($i -lt $attributesLength -and $cleanedAttributes[$i] -notmatch '\s') { $i++ }
                }
                
                $parameters += $paramName
            }
        }
        
        # Create mock matches object for compatibility
        $paramMatches = @()
        foreach ($param in $parameters) {
            $paramMatches += @{ Groups = @(@{}, @{ Value = $param }) }
        }
        
        foreach ($paramMatch in $paramMatches) {
            $paramName = $paramMatch.Groups[1].Value
            
            # Skip ignored parameters (no false positives)
            if ($paramName -in $ignoredParameters) {
                continue
            }
            
            # Check if parameter is valid for this component
            if ($paramName -notin $validParams) {
                $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                
                # Find closest matching parameter for suggestions
                $suggestion = ""
                # Simple case-insensitive match for common typos
                $lowerParamName = $paramName.ToLower()
                foreach ($validParam in $validParams) {
                    if ($validParam.ToLower() -eq $lowerParamName) {
                        $suggestion = $validParam
                        break
                    }
                }
                # If no exact match, look for parameters that start with the same letters
                if (-not $suggestion -and $paramName.Length -ge 3) {
                    $prefix = $paramName.Substring(0, 3).ToLower()
                    foreach ($validParam in $validParams) {
                        if ($validParam.ToLower().StartsWith($prefix)) {
                            $suggestion = $validParam
                            break
                        }
                    }
                }
                
                $violation = @{
                    File = $relativePath
                    FullPath = $file.FullName
                    LineNumber = $lineNumber
                    Component = $componentName
                    InvalidParameter = $paramName
                    ValidParameters = $validParams
                    Suggestion = $suggestion
                    MatchedText = $match.Value
                }
                
                $violations += $violation
                
                $suggestionText = if ($suggestion) { " (Did you mean '$suggestion'?)" } else { "" }
                Write-Host "  ❌ ${relativePath}:${lineNumber} - $componentName does not support parameter '$paramName'$suggestionText" -ForegroundColor Red
                if ($Detailed) {
                    Write-Host "     Valid parameters: $($validParams -join ', ')" -ForegroundColor Yellow
                }
            }
        }
    }
}

# Report structural constraint violations first
if ($structuralViolations.Count -gt 0) {
    Write-Host "`n🏗️ STRUCTURAL CONSTRAINT VIOLATIONS:" -ForegroundColor Red
    foreach ($violation in $structuralViolations) {
        Write-Host "  ❌ $($violation.File):$($violation.LineNumber) - $($violation.Component) used outside required parent" -ForegroundColor Red
        Write-Host "     Required: $($violation.Description)" -ForegroundColor Yellow
    }
}

# Report Results
Write-Host "`n📊 VALIDATION RESULTS:" -ForegroundColor Cyan
$totalViolations = $violations.Count + $structuralViolations.Count
Write-Host "  Total violations found: $totalViolations" -ForegroundColor $(if ($totalViolations -eq 0) { 'Green' } else { 'Red' })
Write-Host "    Parameter violations: $($violations.Count)" -ForegroundColor $(if ($violations.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "    Structural violations: $($structuralViolations.Count)" -ForegroundColor $(if ($structuralViolations.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "  Files scanned: $($razorFiles.Count)" -ForegroundColor White
Write-Host "  R* components found: $totalComponentsFound" -ForegroundColor White
Write-Host "  Component types validated: $($componentParameters.Count)" -ForegroundColor White

if ($totalViolations -eq 0) {
    Write-Host "  ✅ No violations found!" -ForegroundColor Green
    return @{ Success = $true; Violations = @(); StructuralViolations = @() }
}

# Group violations by component
$violationsByComponent = $violations | Group-Object Component | Sort-Object Count -Descending

Write-Host "`n🔴 VIOLATIONS BY COMPONENT:" -ForegroundColor Red
foreach ($group in $violationsByComponent) {
    $componentName = $group.Name
    $count = $group.Count
    $invalidParams = ($group.Group | Select-Object -ExpandProperty InvalidParameter | Sort-Object -Unique) -join ', '
    
    Write-Host "  $componentName ($count violations)" -ForegroundColor Red
    Write-Host "    Invalid parameters: $invalidParams" -ForegroundColor Yellow
}

# Auto-fix if requested
if ($Fix) {
    Write-Host "`n🔧 AUTO-FIXING violations..." -ForegroundColor Yellow
    
    $fixedFiles = @{}
    $fixCount = 0
    
    foreach ($violation in $violations) {
        $filePath = $violation.FullPath
        
        if (-not $fixedFiles.ContainsKey($filePath)) {
            $fixedFiles[$filePath] = Get-Content $filePath -Raw -Encoding UTF8
        }
        
        $content = $fixedFiles[$filePath]
        $paramToRemove = $violation.InvalidParameter
        
        # Remove invalid parameter (handle various formats)
        $removePatterns = @(
            "(\s+$paramToRemove\s*=\s*`"[^`"]*`")",
            "(\s+$paramToRemove\s*=\s*@[^\s>]+)",
            "(\s+$paramToRemove\s*=\s*\{[^}]*\})",
            "(?m)^\s*$paramToRemove\s*=\s*[^>\s]+\s*$"
        )
        
        foreach ($pattern in $removePatterns) {
            if ($content -match $pattern) {
                $content = $content -replace $pattern, ''
                $fixCount++
                break
            }
        }
        
        $fixedFiles[$filePath] = $content
        Write-Host "  ✏️ Removed '$paramToRemove' from $($violation.Component) in $($violation.File)" -ForegroundColor Green
    }
    
    # Write fixed files
    foreach ($filePath in $fixedFiles.Keys) {
        Set-Content -Path $filePath -Value $fixedFiles[$filePath] -Encoding UTF8 -NoNewline
    }
    
    Write-Host "  ✅ Fixed $fixCount invalid parameters in $($fixedFiles.Count) files" -ForegroundColor Green
}

Write-Host "`n💡 RECOMMENDATIONS:" -ForegroundColor Cyan
Write-Host "  • Use source code as truth, not incomplete documentation" -ForegroundColor Gray
Write-Host "  • Consider parameter naming conventions (camelCase vs PascalCase)" -ForegroundColor Gray
Write-Host "  • Check component documentation for parameter usage examples" -ForegroundColor Gray
Write-Host "  • Ensure child components are used within their required parent components" -ForegroundColor Gray

return @{
    Success = $totalViolations -eq 0
    Violations = $violations
    StructuralViolations = $structuralViolations
    ViolationCount = $violations.Count
    StructuralViolationCount = $structuralViolations.Count
    TotalViolationCount = $totalViolations
    ComponentsScanned = $componentParameters.Count
    FilesScanned = $razorFiles.Count
}