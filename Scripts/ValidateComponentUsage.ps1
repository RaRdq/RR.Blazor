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

# Function to extract parameters from C# base class files
function Extract-ParametersFromCsFile {
    param([string]$Content)
    
    $parameters = @()
    $lines = $Content -split "`n"
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i].Trim()
        
        # Look for [Parameter] attributes
        if ($line -match '\[Parameter(?:[\],]|$)') {
            # Look ahead for the property declaration
            for ($j = $i + 1; $j -lt [Math]::Min($lines.Length, $i + 10); $j++) {
                $propLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($propLine)) { continue }
                
                # Match property declaration: public Type PropertyName { get; set; }
                if ($propLine -match 'public\s+(?:virtual\s+)?(?:override\s+)?(?:static\s+)?[^\s]+(?:<[^>]*>)?(?:\?)?\s+(\w+)\s*\{') {
                    $paramName = $Matches[1].Trim()
                    if ($paramName -and $paramName -notmatch '^(get|set)$') {
                        $parameters += $paramName
                    }
                    break
                }
            }
        }
    }
    
    return $parameters | Sort-Object -Unique
}

# Function to extract ACTUAL parameters from component source code with inheritance support
function Extract-ActualParameters {
    param(
        [string]$Content,
        [string]$ComponentPath,
        [hashtable]$BaseClassParameters = @{}
    )
    
    if ([string]::IsNullOrEmpty($Content)) {
        Write-Warning "Content is null or empty for component at path: $ComponentPath"
        return @()
    }
    
    $parameters = @()
    $lines = $Content -split "`n"
    $inCodeBlock = $false
    
    # First, check for @inherits directive to find base class
    $inheritedParameters = @()
    foreach ($line in $lines) {
        if ($line -match '@inherits\s+(\w+(?:<[^>]+>)?)') {
            $baseClassName = $Matches[1] -replace '<.*>', ''  # Remove generic type parameters
            if ($BaseClassParameters.ContainsKey($baseClassName)) {
                $inheritedParameters += $BaseClassParameters[$baseClassName]
                Write-Host "    📎 Inherited $($BaseClassParameters[$baseClassName].Count) parameters from $baseClassName (including full inheritance chain)" -ForegroundColor DarkCyan
            }
        }
    }
    
    # Then extract parameters from the component itself
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
    
    # Combine inherited and component-specific parameters
    $allParameters = @($inheritedParameters + $parameters) | Sort-Object -Unique
    return $allParameters
}

# Function to extract REQUIRED parameters from component source code
function Extract-RequiredParameters {
    param([string]$Content)
    
    $requiredParameters = @()
    $lines = $Content -split "`n"
    $inCodeBlock = $false
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i].Trim()
        
        # Track @code blocks
        if ($line -match '@code\\s*{') {
            $inCodeBlock = $true
            continue
        }
        if ($line -match '^\\s*}\\s*$' -and $inCodeBlock) {
            $inCodeBlock = $false
            continue
        }
        
        # Look for ArgumentNullException or required parameter validation
        if ($line -match 'ArgumentNullException.*Parameter.*"([^"]+)"') {
            $paramName = $Matches[1]
            if ($paramName -and $paramName -notin $requiredParameters) {
                $requiredParameters += $paramName
            }
        }
        
        # Look for required parameter validation patterns
        if ($line -match 'if\s*\(.*\s*(\w+)\s*==\s*null.*throw.*required') {
            $paramName = $Matches[1]
            if ($paramName -and $paramName -notin $requiredParameters) {
                $requiredParameters += $paramName
            }
        }
        
        # Look for EditorRequired attribute
        if ($line -match '\[EditorRequired\]') {
            # Find the next property
            for ($j = $i + 1; $j -lt [Math]::Min($lines.Length, $i + 10); $j++) {
                $nextLine = $lines[$j].Trim()
                if ($nextLine -match 'public\s+[^\s]+\s+(\w+)\s*\{') {
                    $paramName = $Matches[1]
                    if ($paramName -and $paramName -notin $requiredParameters) {
                        $requiredParameters += $paramName
                    }
                    break
                }
            }
        }
    }
    
    return $requiredParameters | Sort-Object -Unique
}

Write-Host "🔍 BULLETPROOF R* Component Parameter Validation" -ForegroundColor Cyan
Write-Host "Using SOURCE CODE as truth (.razor + .cs inheritance)" -ForegroundColor Yellow

# Function to resolve inheritance chain recursively
function Resolve-InheritanceChain {
    param(
        [string]$ClassName,
        [hashtable]$BaseClassParameters,
        [hashtable]$BaseClassInheritance,
        [System.Collections.Generic.HashSet[string]]$VisitedClasses = [System.Collections.Generic.HashSet[string]]::new()
    )
    
    # Prevent infinite recursion
    if ($VisitedClasses.Contains($ClassName)) {
        return @()
    }
    $VisitedClasses.Add($ClassName) | Out-Null
    
    $allParameters = @()
    
    # Add parameters from current class
    if ($BaseClassParameters.ContainsKey($ClassName)) {
        $allParameters += $BaseClassParameters[$ClassName]
    }
    
    # Add parameters from parent class
    if ($BaseClassInheritance.ContainsKey($ClassName)) {
        $parentClass = $BaseClassInheritance[$ClassName]
        $parentParameters = Resolve-InheritanceChain -ClassName $parentClass -BaseClassParameters $BaseClassParameters -BaseClassInheritance $BaseClassInheritance -VisitedClasses $VisitedClasses
        $allParameters += $parentParameters
    }
    
    return $allParameters | Sort-Object -Unique
}

# Build BASE CLASS parameter dictionary and inheritance chain from .cs files first
Write-Host "📂 Building base class parameter dictionary with inheritance..." -ForegroundColor Yellow

$baseClassParameters = @{}
$baseClassInheritance = @{}
# Search for all *Base.cs files in any subdirectory, not just /Base folder
$baseClassFiles = Get-ChildItem -Path $ComponentsPath -Filter "*Base.cs" -Recurse

if ($baseClassFiles) {
    
    # First pass: collect parameters and inheritance info
    foreach ($file in $baseClassFiles) {
        $className = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        try {
            $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
            if (-not [string]::IsNullOrEmpty($content)) {
                # Extract parameters
                $parameters = Extract-ParametersFromCsFile -Content $content
                if ($parameters.Count -gt 0) {
                    $baseClassParameters[$className] = $parameters
                }
                
                # Extract inheritance information
                if ($content -match "class\s+$className(?:<[^>]+>)?\s*:\s*(\w+)(?:<[^>]+>)?") {
                    $parentClass = $Matches[1]
                    # Skip Microsoft base classes
                    if ($parentClass -notmatch "^(ComponentBase|Object|Enum)$") {
                        $baseClassInheritance[$className] = $parentClass
                        Write-Host "  🔗 $className -> $parentClass" -ForegroundColor DarkBlue
                    }
                }
                
                Write-Host "  📄 Base class ${className}: $($parameters.Count) parameters" -ForegroundColor DarkCyan
            }
        }
        catch {
            Write-Warning "Failed to read base class file: $($file.FullName) - $($_.Exception.Message)"
        }
    }
    
    # Second pass: resolve full inheritance chains
    $resolvedBaseClassParameters = @{}
    foreach ($className in $baseClassParameters.Keys) {
        $allParams = Resolve-InheritanceChain -ClassName $className -BaseClassParameters $baseClassParameters -BaseClassInheritance $baseClassInheritance
        $resolvedBaseClassParameters[$className] = $allParams
        Write-Host "  ✅ $className total: $($allParams.Count) parameters (including inherited)" -ForegroundColor Green
    }
    
    $baseClassParameters = $resolvedBaseClassParameters
}

# Build SOURCE OF TRUTH parameter dictionary from actual .razor files with inheritance
Write-Host "📂 Building component parameter dictionary with inheritance..." -ForegroundColor Yellow

$componentParameters = @{}
$componentRequiredParams = @{}
$componentFiles = Get-ChildItem -Path $ComponentsPath -Filter "R*.razor" -Recurse

foreach ($file in $componentFiles) {
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        
        Write-Host "  📄 Parsing $componentName..." -ForegroundColor DarkGray
        
        if ([string]::IsNullOrEmpty($content)) {
            Write-Warning "Content is empty for: $($file.FullName)"
            continue
        }
        
        $parameters = Extract-ActualParameters -Content $content -ComponentPath $file.FullName -BaseClassParameters $baseClassParameters
        $requiredParams = Extract-RequiredParameters -Content $content
        
        $componentParameters[$componentName] = $parameters
        $componentRequiredParams[$componentName] = $requiredParams
        
        if ($Detailed) {
            Write-Host "    Parameters: $($parameters -join ', ')" -ForegroundColor DarkGreen
            if ($requiredParams.Count -gt 0) {
                Write-Host "    Required: $($requiredParams -join ', ')" -ForegroundColor Yellow
            }
        } else {
            Write-Host "    ✅ Found $($parameters.Count) parameters ($($requiredParams.Count) required)" -ForegroundColor DarkGreen
        }
    }
    catch {
        Write-Warning "Failed to process component: $($file.FullName) - $($_.Exception.Message)"
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
    'RDataTableColumnGeneric' = @{
        RequiredParent = 'RDataTableGeneric'
        Context = 'ColumnsContent'
        Description = 'RDataTableColumnGeneric must be used within RDataTableGeneric''s ColumnsContent'
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
        RequiredParent = @('RForm', 'RFormGeneric')
        Context = 'FormFields'
        Description = 'RFormSection must be used within RForm''s or RFormGeneric''s FormFields'
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
        $requiredParents = $constraint.RequiredParent
        
        # Ensure it's an array
        if ($requiredParents -is [string]) {
            $requiredParents = @($requiredParents)
        }
        
        # Find all instances of child component (using word boundaries to prevent substring matches)
        $childPattern = "(?s)<$childComponent\b[^>]*(?:/>|>.*?</$childComponent>)"
        $childMatches = [regex]::Matches($Content, $childPattern)
        
        foreach ($childMatch in $childMatches) {
            $childStart = $childMatch.Index
            $childEnd = $childMatch.Index + $childMatch.Length
            
            $foundValidParent = $false
            
            # Check each possible parent type
            foreach ($requiredParent in $requiredParents) {
                # Look for parent component that contains this child (using word boundaries to prevent substring matches)
                $parentPattern = "(?s)<$requiredParent\b[^>]*>.*?</$requiredParent>"
                $parentMatches = [regex]::Matches($Content, $parentPattern)
                
                foreach ($parentMatch in $parentMatches) {
                    $parentStart = $parentMatch.Index
                    $parentEnd = $parentMatch.Index + $parentMatch.Length
                    
                    # Check if child is within parent boundaries
                    if ($childStart -ge $parentStart -and $childEnd -le $parentEnd) {
                        $foundValidParent = $true
                        break
                    }
                }
                
                if ($foundValidParent) {
                    break
                }
            }
            
            if (-not $foundValidParent) {
                $lineNumber = ($Content.Substring(0, $childStart) -split "`n").Count
                
                # Determine severity for structural violations
                $structuralSeverity = "ERROR"
                $structuralSeverityIcon = "❌"
                $structuralSeverityColor = "Red"
                
                if ($FilePath -like "*.md") {
                    $structuralSeverity = "WARNING"
                    $structuralSeverityIcon = "⚠️"
                    $structuralSeverityColor = "Yellow"
                } elseif ($FilePath -like "*.html") {
                    $structuralSeverity = "WARNING"
                    $structuralSeverityIcon = "⚠️"
                    $structuralSeverityColor = "Yellow"
                }
                
                $constraintViolations += @{
                    File = $FilePath
                    LineNumber = $lineNumber
                    Component = $childComponent
                    RequiredParent = $requiredParent
                    Context = $constraint.Context
                    Description = $constraint.Description
                    MatchedText = $childMatch.Value
                    Severity = $structuralSeverity
                    SeverityIcon = $structuralSeverityIcon
                    SeverityColor = $structuralSeverityColor
                }
            }
        }
    }
    
    return $constraintViolations
}

Write-Host "🔍 Scanning solution for R* component usage..." -ForegroundColor Yellow

# Find all files that might contain R* component usage (razor, md, html, etc.)
$filesToScan = @()

# Get .razor files (primary target)
$razorFiles = Get-ChildItem -Path $SolutionPath -Filter "*.razor" -Recurse | Where-Object { 
    $_.FullName -notlike "*RR.Blazor*" -and 
    $_.FullName -notlike "*_Backup*" -and
    $_.FullName -notlike "*backup*" -and
    $_.FullName -notlike "*\.git*" -and
    $_.FullName -notlike "*\bin\*" -and
    $_.FullName -notlike "*\obj\*" -and
    $_.FullName -notlike "*\node_modules\*"
}

# Get .md files (documentation files that might have examples)
$mdFiles = Get-ChildItem -Path $SolutionPath -Filter "*.md" -Recurse | Where-Object { 
    $_.FullName -notlike "*\.git*" -and
    $_.FullName -notlike "*\bin\*" -and
    $_.FullName -notlike "*\obj\*" -and
    $_.FullName -notlike "*\node_modules\*"
}

# Get .html files (might contain component examples)
$htmlFiles = Get-ChildItem -Path $SolutionPath -Filter "*.html" -Recurse | Where-Object { 
    $_.FullName -notlike "*\.git*" -and
    $_.FullName -notlike "*\bin\*" -and
    $_.FullName -notlike "*\obj\*" -and
    $_.FullName -notlike "*\node_modules\*"
}

$filesToScan = $razorFiles + $mdFiles + $htmlFiles

$violations = @()
$structuralViolations = @()
$totalComponentsFound = 0

foreach ($file in $filesToScan) {
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        
        # Handle null content gracefully
        if ([string]::IsNullOrEmpty($content)) {
            Write-Warning "File content is null or empty: $($file.FullName)"
            continue
        }
        
        $relativePath = $file.FullName.Replace($SolutionPath, '').TrimStart('\', '/')
    }
    catch {
        Write-Warning "Failed to read file: $($file.FullName) - $($_.Exception.Message)"
        continue
    }
    
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
        
        # Extract parameters using Blazor-aware approach
        # Match actual parameter assignments: ParamName="value" or ParamName={expr} or @bind-ParamName
        $parameterPattern = '(?:^|\s)((?:@bind-)?[A-Z]\w*)(?=\s*=)'
        $paramMatches = [regex]::Matches(" $attributesSection ", $parameterPattern)
        
        
        foreach ($paramMatch in $paramMatches) {
            $paramName = $paramMatch.Groups[1].Value
            
            # Handle Blazor @bind-Parameter syntax
            $actualParamName = $paramName
            if ($paramName.StartsWith("@bind-")) {
                $actualParamName = $paramName.Substring(6)  # Remove "@bind-" prefix
                # For @bind-Parameter, check if Parameter or ParameterChanged exists
                $bindingValid = ($actualParamName -in $validParams) -or ("$($actualParamName)Changed" -in $validParams)
                if ($bindingValid) {
                    continue  # @bind-Parameter is valid
                }
            }
            
            # Skip ignored parameters (no false positives)
            if ($actualParamName -in $ignoredParameters) {
                continue
            }
            
            # Check if parameter is valid for this component
            if ($actualParamName -notin $validParams) {
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
                
                # Determine severity based on file type
                $severity = "ERROR"
                $severityIcon = "❌"
                $severityColor = "Red"
                
                if ($file.Extension -eq ".md") {
                    $severity = "WARNING"
                    $severityIcon = "⚠️"
                    $severityColor = "Yellow"
                } elseif ($file.Extension -eq ".html") {
                    $severity = "WARNING" 
                    $severityIcon = "⚠️"
                    $severityColor = "Yellow"
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
                    Severity = $severity
                    SeverityIcon = $severityIcon
                    SeverityColor = $severityColor
                }
                
                $violations += $violation
                
                $suggestionText = if ($suggestion) { " (Did you mean '$suggestion'?)" } else { "" }
                Write-Host "  $severityIcon ${relativePath}:${lineNumber} - [$severity] $componentName does not support parameter '$paramName'$suggestionText" -ForegroundColor $severityColor
                if ($Detailed) {
                    Write-Host "     Valid parameters: $($validParams -join ', ')" -ForegroundColor Gray
                }
            }
        }
    }
}

# Report structural constraint violations first
if ($structuralViolations.Count -gt 0) {
    Write-Host "`n🏗️ STRUCTURAL CONSTRAINT VIOLATIONS:" -ForegroundColor Red
    foreach ($violation in $structuralViolations) {
        Write-Host "  $($violation.SeverityIcon) $($violation.File):$($violation.LineNumber) - [$($violation.Severity)] $($violation.Component) used outside required parent" -ForegroundColor $violation.SeverityColor
        Write-Host "     Required: $($violation.Description)" -ForegroundColor Gray
    }
}

# Report Results
Write-Host "`n📊 VALIDATION RESULTS:" -ForegroundColor Cyan

# Count errors vs warnings
$paramErrors = ($violations | Where-Object { $_.Severity -eq "ERROR" }).Count
$paramWarnings = ($violations | Where-Object { $_.Severity -eq "WARNING" }).Count
$structuralErrors = ($structuralViolations | Where-Object { $_.Severity -eq "ERROR" }).Count
$structuralWarnings = ($structuralViolations | Where-Object { $_.Severity -eq "WARNING" }).Count

$totalErrors = $paramErrors + $structuralErrors
$totalWarnings = $paramWarnings + $structuralWarnings
$totalViolations = $violations.Count + $structuralViolations.Count

Write-Host "  Total violations found: $totalViolations" -ForegroundColor $(if ($totalViolations -eq 0) { 'Green' } else { 'Red' })
Write-Host "    ❌ ERRORS: $totalErrors (blocking compilation)" -ForegroundColor $(if ($totalErrors -eq 0) { 'Green' } else { 'Red' })
Write-Host "    ⚠️  WARNINGS: $totalWarnings (documentation issues)" -ForegroundColor $(if ($totalWarnings -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "  Parameter violations: $($violations.Count) ($paramErrors errors, $paramWarnings warnings)" -ForegroundColor $(if ($violations.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "  Structural violations: $($structuralViolations.Count) ($structuralErrors errors, $structuralWarnings warnings)" -ForegroundColor $(if ($structuralViolations.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "  Files scanned: $($filesToScan.Count) (Razor: $($razorFiles.Count), MD: $($mdFiles.Count), HTML: $($htmlFiles.Count))" -ForegroundColor White
Write-Host "  R* components found: $totalComponentsFound" -ForegroundColor White
Write-Host "  Component types validated: $($componentParameters.Count)" -ForegroundColor White

if ($totalViolations -eq 0) {
    Write-Host "  ✅ No violations found!" -ForegroundColor Green
    return @{ Success = $true; Violations = @(); StructuralViolations = @(); ErrorCount = 0; WarningCount = 0 }
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

return @{
    Success = $totalErrors -eq 0  # Success only if no errors (warnings are acceptable)
    Violations = $violations
    StructuralViolations = $structuralViolations
    ViolationCount = $violations.Count
    StructuralViolationCount = $structuralViolations.Count
    TotalViolationCount = $totalViolations
    ErrorCount = $totalErrors
    WarningCount = $totalWarnings
    ParameterErrors = $paramErrors
    ParameterWarnings = $paramWarnings
    StructuralErrors = $structuralErrors
    StructuralWarnings = $structuralWarnings
    ComponentsScanned = $componentParameters.Count
    FilesScanned = $filesToScan.Count
}