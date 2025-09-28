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
            $prevI = $i - 1
            $prevJ = $j - 1
            $cost = if ($String1.Substring($prevI, 1) -eq $String2.Substring($prevJ, 1)) { 0 } else { 1 }
            $matrix[$i, $j] = [Math]::Min([Math]::Min($matrix[$prevI, $j] + 1, $matrix[$i, $prevJ] + 1), $matrix[$prevI, $prevJ] + $cost)
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
        
        # Look for [Parameter] attributes (including those with additional attributes like AIParameter)
        if ($line -match '\[Parameter') {
            # Look ahead for the property declaration, handle multi-line and complex properties
            $foundProperty = $false
            $searchText = ""
            
            for ($j = $i; $j -lt [Math]::Min($lines.Length, $i + 10); $j++) {
                $currentLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($currentLine)) { continue }
                
                # Skip other attributes that might be on separate lines (like [AIParameter])
                if ($currentLine -match '^\[' -and $j -ne $i) { continue }
                
                $searchText += " " + $currentLine
                
                # Match property declaration - use simple patterns that work
                try {
                    # Try multiple patterns to catch different property declaration styles
                    $matched = $false
                    
                    # Pattern 1: public Type PropertyName { (most common)
                    if ($currentLine -match 'public\s+\w+(?:<[^>]+>)?(?:\[[\],]*\])?(?:\?)?\s+(\w+)\s*\{') {
                        $paramName = $Matches[1]
                        $matched = $true
                    }
                    # Pattern 2: public string PropertyName { (simple types) - most reliable
                    elseif ($currentLine -match 'public\s+string\s+(\w+)') {
                        $paramName = $Matches[1]
                        $matched = $true
                    }
                    # Pattern 2b: other simple types
                    elseif ($currentLine -match 'public\s+(?:int|bool|decimal|float|double|DateTime|Guid)\s+(\w+)') {
                        $paramName = $Matches[1]
                        $matched = $true
                    }
                    # Pattern 3: Generic catch-all for public properties
                    elseif ($currentLine -match 'public\s+\S+\s+(\w+)\s*\{') {
                        $paramName = $Matches[1]
                        $matched = $true
                    }
                    
                    if ($matched -and $paramName) {
                        $paramName = $paramName.Trim()
                        if ($paramName -and $paramName -notmatch '^(get|set|value)$') {
                            $parameters += $paramName
                            $foundProperty = $true
                        }
                        break
                    }
                    # Pattern 2: public Type PropertyName (property name at end of line, { on next line)
                    if ($currentLine -match 'public\s+.*?\s+(\w+)\s*$') {
                        # Look ahead to next line for opening brace
                        $nextLineIndex = $j + 1
                        if ($nextLineIndex -lt $lines.Length) {
                            $nextLine = $lines[$nextLineIndex].Trim()
                            if ($nextLine -match '^\{' -or $nextLine -match '^get\s*=>' -or $nextLine -eq '{') {
                                $paramName = $Matches[1]
                                if ($paramName) {
                                    $paramName = $paramName.Trim()
                                    if ($paramName -and $paramName -notmatch '^(get|set)$') {
                                        $parameters += $paramName
                                        $foundProperty = $true
                                    }
                                }
                                break
                            }
                        }
                    }
                } catch {
                    Write-Warning "Error processing line $j in parameter extraction: $($_.Exception.Message)"
                }
            }
            
            # Debug output for troubleshooting
            if (-not $foundProperty -and $line -match '\[Parameter') {
                Write-Debug "Failed to find property for Parameter attribute at line $i in: $searchText"
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
        if ($line -match '@inherits\s+(\w+)(?:<[^>]+>)?') {
            $baseClassName = $Matches[1]  # Get base class name without generic parameters
            
            # Try to find the base class in our dictionary
            if ($BaseClassParameters.ContainsKey($baseClassName)) {
                $inheritedParameters += $BaseClassParameters[$baseClassName]
                Write-Host "     Inherited $($BaseClassParameters[$baseClassName].Count) parameters from $baseClassName (including full inheritance chain)" -ForegroundColor DarkCyan
            }
            # Also check for base classes that might have "Base" suffix
            elseif ($BaseClassParameters.ContainsKey("${baseClassName}Base")) {
                $inheritedParameters += $BaseClassParameters["${baseClassName}Base"]
                Write-Host "     Inherited $($BaseClassParameters["${baseClassName}Base"].Count) parameters from ${baseClassName}Base (including full inheritance chain)" -ForegroundColor DarkCyan
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
        
        # Look for [Parameter] attributes (handle all variations including AIParameter)
        if ($line -match '\[Parameter') {
            # Collect multi-line parameter definition
            $parameterText = ""
            $propertyFound = $false
            
            for ($j = $i; $j -lt [Math]::Min($lines.Length, $i + 20); $j++) {
                $currentLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($currentLine)) { continue }
                
                $parameterText += " " + $currentLine
                
                # Found the property declaration - handle both simple and complex properties
                if ($currentLine -match 'public\s+[^{]+\{' -or $currentLine -match 'public\s+.*\s+\w+\s*$') {
                    $propertyFound = $true
                    # For complex properties, continue reading until we find the opening brace or end
                    if ($currentLine -notmatch '\{') {
                        # Property declaration continues on next lines
                        continue
                    } else {
                        break
                    }
                }
            }
            
            # Enhanced regex to handle nested generic types and complex property definitions
            # This handles both simple { get; set; } and complex custom implementations
            if ($propertyFound -and $parameterText -match 'public\s+(?:virtual\s+)?(?:override\s+)?(?:static\s+)?[^\s{]+(?:<[^{}>]*(?:<[^{}>]*>[^{}>]*)*>)*(?:\?)?\s+(@?\w+)\s*[\{\s]') {
                $paramName = $Matches[1].Trim()
                if ($paramName -and $paramName -notmatch '^(get|set)$') {
                    $parameters += $paramName
                }
            }
        }
    }
    
    # Combine inherited and component-specific parameters
    $combinedParameters = @()
    $combinedParameters += $inheritedParameters
    $combinedParameters += $parameters
    $allParameters = $combinedParameters | Sort-Object -Unique
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
            if ($paramName -and $requiredParameters -notcontains $paramName) {
                $requiredParameters += $paramName
            }
        }
        
        # Look for required parameter validation patterns
        if ($line -match 'if\s*\(.*\s*(\w+)\s*==\s*null.*throw.*required') {
            $paramName = $Matches[1]
            if ($paramName -and $requiredParameters -notcontains $paramName) {
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
                    if ($paramName -and $requiredParameters -notcontains $paramName) {
                        $requiredParameters += $paramName
                    }
                    break
                }
            }
        }
    }
    
    return $requiredParameters | Sort-Object -Unique
}

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
        if ($parentParameters -and $parentParameters.Count -gt 0) {
            $allParameters += $parentParameters
        }
    }
    
    return $allParameters | Sort-Object -Unique
}

# Build BASE CLASS parameter dictionary and inheritance chain from .cs files first
Write-Host "Building inheritance tree..." -ForegroundColor Gray

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
                        Write-Host "   $className -> $parentClass" -ForegroundColor DarkBlue
                    }
                }
                
                Write-Host "   Base class ${className}: $($parameters.Count) parameters" -ForegroundColor DarkCyan
                if ($parameters.Count -gt 0) {
                    Write-Host "     Parameters: $($parameters -join ', ')" -ForegroundColor DarkGray
                } elseif ($className -match "Text|Sized|Variant") {
                    # Debug why these aren't being detected
                    Write-Host "     WARNING: No parameters detected for $className - check extraction" -ForegroundColor Yellow
                }
            }
        }
        catch {
            Write-Warning "Failed to read base class file: $($file.FullName) - $($_.Exception.Message)"
        }
    }
    
    # Second pass: resolve full inheritance chains
    Write-Host " Resolving inheritance chains..." -ForegroundColor Yellow
    $resolvedBaseClassParameters = @{}
    foreach ($className in $baseClassParameters.Keys) {
        $allParams = Resolve-InheritanceChain -ClassName $className -BaseClassParameters $baseClassParameters -BaseClassInheritance $baseClassInheritance
        $resolvedBaseClassParameters[$className] = $allParams
        if ($allParams.Count -gt 0) {
            Write-Host "   $className total: $($allParams.Count) parameters (including inherited)" -ForegroundColor Green
            if ($allParams.Count -gt 5) {
                # Show first 5 parameters for readability
                $preview = ($allParams | Select-Object -First 5) -join ', '
                Write-Host "     Parameters: $preview, ..." -ForegroundColor DarkGray
            } else {
                Write-Host "     Parameters: $($allParams -join ', ')" -ForegroundColor DarkGray
            }
        }
    }
    
    $baseClassParameters = $resolvedBaseClassParameters
}

# Build SOURCE OF TRUTH parameter dictionary from actual .razor files with inheritance
Write-Host "Parsing component parameters..." -ForegroundColor Gray

$componentParameters = @{}
$componentRequiredParams = @{}

# Get both .razor and .cs files for R* components
$razorFiles = Get-ChildItem -Path $ComponentsPath -Filter "R*.razor" -Recurse
$csFiles = Get-ChildItem -Path $ComponentsPath -Filter "R*.cs" -Recurse | Where-Object { 
    $_.Name -notlike "*Base.cs" -and $_.Name -notlike "*Models.cs" -and $_.Name -notlike "*Enums.cs"
}

# Use ArrayList to avoid array concatenation issues
$componentFileList = New-Object System.Collections.ArrayList
if ($razorFiles) { $componentFileList.AddRange($razorFiles) }
if ($csFiles) { $componentFileList.AddRange($csFiles) }

foreach ($file in $componentFileList) {
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        
        Write-Host "   Parsing $componentName..." -ForegroundColor DarkGray
        
        # Debug mode for RChoice specifically
        $debugComponent = ($componentName -eq "RChoice")
        
        if ([string]::IsNullOrEmpty($content)) {
            Write-Warning "Content is empty for: $($file.FullName)"
            continue
        }
        
        # Use different extraction method based on file type
        if ($file.Extension -eq ".cs") {
            $parameters = Extract-ParametersFromCsFile -Content $content
            # For .cs files, also check if there are inherited base class parameters
            foreach ($line in ($content -split "`n")) {
                if ($line -match "class\s+$componentName(?:<[^>]+>)?\s*:\s*(\w+)(?:<[^>]+>)?") {
                    $parentClass = $Matches[1]
                    if ($baseClassParameters.ContainsKey($parentClass)) {
                        $inheritedParams = $baseClassParameters[$parentClass]
                        if ($inheritedParams -and $inheritedParams.Count -gt 0) {
                            $parameters += $inheritedParams
                            Write-Host "     Inherited $($inheritedParams.Count) parameters from $parentClass (including full inheritance chain)" -ForegroundColor DarkCyan
                        }
                    }
                }
            }
            $parameters = $parameters | Sort-Object -Unique
        } else {
            $parameters = Extract-ActualParameters -Content $content -ComponentPath $file.FullName -BaseClassParameters $baseClassParameters
        }
        $requiredParams = Extract-RequiredParameters -Content $content
        
        # Handle duplicate component names by merging parameters
        if ($componentParameters.ContainsKey($componentName)) {
            # Merge with existing parameters (union)
            $existingParams = $componentParameters[$componentName]
            $combinedParams = @()
            $combinedParams += $existingParams
            $combinedParams += $parameters
            $mergedParams = $combinedParams | Sort-Object -Unique
            $componentParameters[$componentName] = $mergedParams
            
            $existingReqParams = $componentRequiredParams[$componentName]
            $combinedReqParams = @()
            $combinedReqParams += $existingReqParams
            $combinedReqParams += $requiredParams
            $mergedReqParams = $combinedReqParams | Sort-Object -Unique
            $componentRequiredParams[$componentName] = $mergedReqParams
            
            Write-Host "     Merged with existing $componentName ($($existingParams.Count) + $($parameters.Count) = $($mergedParams.Count) parameters)" -ForegroundColor Cyan
        } else {
            $componentParameters[$componentName] = $parameters
            $componentRequiredParams[$componentName] = $requiredParams
        }
        
        if ($Detailed -or $debugComponent) {
            Write-Host "    Parameters: $($parameters -join ', ')" -ForegroundColor DarkGreen
            if ($requiredParams.Count -gt 0) {
                Write-Host "    Required: $($requiredParams -join ', ')" -ForegroundColor Yellow
            }
        } else {
            Write-Host "     Found $($parameters.Count) parameters ($($requiredParams.Count) required)" -ForegroundColor DarkGreen
        }
    }
    catch {
        Write-Warning "Failed to process component: $($file.FullName) - $($_.Exception.Message)"
    }
}

Write-Host "Parameter dictionary: $($componentParameters.Count) components, $($componentParameters.Values | ForEach-Object { $_.Count } | Measure-Object -Sum | Select-Object -ExpandProperty Sum) total parameters" -ForegroundColor Gray

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
        RequiredParent = 'RTableGeneric'
        Context = 'ColumnsContent'
        Description = 'RDataTableColumnGeneric must be used within RTableGeneric''s ColumnsContent'
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
                $structuralSeverityIcon = "[ERROR]"
                $structuralSeverityColor = "Red"
                
                if ($FilePath -like "*.md") {
                    $structuralSeverity = "WARNING"
                    $structuralSeverityIcon = "[WARNING]"
                    $structuralSeverityColor = "Yellow"
                } elseif ($FilePath -like "*.html") {
                    $structuralSeverity = "WARNING"
                    $structuralSeverityIcon = "[WARNING]"
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

# Find all files that might contain R* component usage (razor, md, html, etc.)
# Use ArrayLists to avoid array concatenation issues
$razorFilesList = New-Object System.Collections.ArrayList
$mdFilesList = New-Object System.Collections.ArrayList
$htmlFilesList = New-Object System.Collections.ArrayList

# Get .razor files (primary target)
Get-ChildItem -Path $SolutionPath -Filter "*.razor" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.FullName -notlike "*RR.Blazor*" -and 
        $_.FullName -notlike "*_Backup*" -and
        $_.FullName -notlike "*backup*" -and
        $_.FullName -notlike "*\.git*" -and
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*" -and
        $_.FullName -notlike "*\node_modules\*") {
        [void]$razorFilesList.Add($_)
    }
}
Write-Host "  Found $($razorFilesList.Count) .razor files" -ForegroundColor Gray

# Get .md files (documentation files that might have examples)
Write-Host "  Searching for .md files..." -ForegroundColor Gray
Get-ChildItem -Path $SolutionPath -Filter "*.md" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.FullName -notlike "*\.git*" -and
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*" -and
        $_.FullName -notlike "*\node_modules\*") {
        [void]$mdFilesList.Add($_)
    }
}
Write-Host "  Found $($mdFilesList.Count) .md files" -ForegroundColor Gray

# Get .html files (might contain component examples)
Write-Host "  Searching for .html files..." -ForegroundColor Gray
Get-ChildItem -Path $SolutionPath -Filter "*.html" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.FullName -notlike "*\.git*" -and
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*" -and
        $_.FullName -notlike "*\node_modules\*") {
        [void]$htmlFilesList.Add($_)
    }
}
Write-Host "  Found $($htmlFilesList.Count) .html files" -ForegroundColor Gray

# Combine all files
$filesToScan = New-Object System.Collections.ArrayList
$filesToScan.AddRange($razorFilesList)
$filesToScan.AddRange($mdFilesList)
$filesToScan.AddRange($htmlFilesList)

Write-Host "  Total files to scan: $($filesToScan.Count)" -ForegroundColor Gray

$violations = New-Object System.Collections.ArrayList
$structuralViolations = New-Object System.Collections.ArrayList
$totalComponentsFound = 0

$processedFiles = 0
foreach ($file in $filesToScan) {
    $processedFiles++
    if ($processedFiles % 50 -eq 0) {
        Write-Host "  Processing file $processedFiles of $($filesToScan.Count)..." -ForegroundColor Gray
    }
    
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        
        # Handle null content gracefully
        if ([string]::IsNullOrEmpty($content)) {
            Write-Warning "File content is null or empty: $($file.FullName)"
            continue
        }
        
        $relativePath = $file.FullName.Replace($SolutionPath, '')
        $relativePath = $relativePath.TrimStart('\').TrimStart('/')
    }
    catch {
        Write-Warning "Failed to read file: $($file.FullName) - $($_.Exception.Message)"
        continue
    }
    
    # Check structural constraints first
    $constraintViolations = Test-ComponentConstraints -Content $content -FilePath $relativePath
    if ($constraintViolations) {
        $structuralViolations.AddRange($constraintViolations)
    }
    
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
        
        $validParams = @($componentParameters[$componentName])
        
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
                $bindingValid = ($validParams -contains $actualParamName) -or ($validParams -contains "$($actualParamName)Changed")
                if ($bindingValid) {
                    continue  # @bind-Parameter is valid
                }
            }
            
            # Skip ignored parameters (no false positives)
            if ($ignoredParameters -contains $actualParamName) {
                continue
            }
            
            # Check if parameter is valid for this component
            # SPECIAL HANDLING: Components using RAttributeForwarder can accept additional HTML attributes
            $usesAttributeForwarder = $false
            
            # Dynamically check if component uses RForwardingComponentBase or has AdditionalAttributes
            if ($validParams -contains 'AdditionalAttributes' -or 
                ($validParams | Where-Object { $_ -match 'Class|Style|Disabled|ChildContent' }).Count -ge 3) {
                $usesAttributeForwarder = $true
            }
            
            # Check if it's a standard HTML attribute that RAttributeForwarder would accept
            $htmlAttributes = @(
                'id', 'class', 'style', 'title', 'lang', 'dir', 'accesskey', 'contenteditable', 'contextmenu', 
                'draggable', 'dropzone', 'hidden', 'spellcheck', 'translate', 'role', 'tabindex',
                # ARIA attributes
                'aria-label', 'aria-labelledby', 'aria-describedby', 'aria-hidden', 'aria-expanded', 
                'aria-selected', 'aria-checked', 'aria-disabled', 'aria-required', 'aria-invalid',
                # Data attributes (pattern)
                'data-.*',
                # Event attributes
                'onclick', 'ondblclick', 'onmousedown', 'onmouseup', 'onmouseover', 'onmouseout', 
                'onmousemove', 'onkeydown', 'onkeyup', 'onkeypress', 'onfocus', 'onblur', 'onchange',
                'onsubmit', 'onreset', 'onload', 'onunload'
            )
            
            $isHtmlAttribute = $false
            foreach ($htmlAttr in $htmlAttributes) {
                if ($htmlAttr.EndsWith('.*')) {
                    # Pattern matching for data-* attributes
                    $pattern = $htmlAttr.Replace('.*', '')
                    if ($actualParamName.ToLower().StartsWith($pattern.ToLower())) {
                        $isHtmlAttribute = $true
                        break
                    }
                } elseif ($actualParamName.ToLower() -eq $htmlAttr.ToLower()) {
                    $isHtmlAttribute = $true
                    break
                }
            }
            
            # If component uses attribute forwarding and it's a standard HTML attribute, allow it
            if ($usesAttributeForwarder -and $isHtmlAttribute) {
                continue  # Skip validation - this is valid
            }
            
            if ($validParams -notcontains $actualParamName) {
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
                # If no exact match, use Levenshtein distance to find best match
                if (-not $suggestion -and $paramName.Length -ge 3) {
                    $bestMatch = ""
                    $bestDistance = [int]::MaxValue
                    
                    foreach ($validParam in $validParams) {
                        $distance = Get-LevenshteinDistance -String1 $paramName.ToLower() -String2 $validParam.ToLower()
                        # Only suggest if distance is reasonable (not more than half the length)
                        $maxDistance = [Math]::Max(1, [Math]::Floor($paramName.Length / 2))
                        if ($distance -lt $bestDistance -and $distance -le $maxDistance) {
                            $bestDistance = $distance
                            $bestMatch = $validParam
                        }
                    }
                    
                    $suggestion = $bestMatch
                }
                
                # Determine severity based on file type
                $severity = "ERROR"
                $severityIcon = "[ERROR]"
                $severityColor = "Red"
                
                if ($file.Extension -eq ".md") {
                    $severity = "WARNING"
                    $severityIcon = "[WARNING]"
                    $severityColor = "Yellow"
                } elseif ($file.Extension -eq ".html") {
                    $severity = "WARNING" 
                    $severityIcon = "[WARNING]"
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
                
                [void]$violations.Add($violation)
                
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
    Write-Host "`n STRUCTURAL CONSTRAINT VIOLATIONS:" -ForegroundColor Red
    foreach ($violation in $structuralViolations) {
        Write-Host "  $($violation.SeverityIcon) $($violation.File):$($violation.LineNumber) - [$($violation.Severity)] $($violation.Component) used outside required parent" -ForegroundColor $violation.SeverityColor
        Write-Host "     Required: $($violation.Description)" -ForegroundColor Gray
    }
}

# Report Results
Write-Host "`n VALIDATION RESULTS:" -ForegroundColor Cyan

# Count errors vs warnings
$paramErrors = 0
$paramWarnings = 0
foreach ($v in $violations) {
    if ($v.Severity -eq "ERROR") { $paramErrors++ }
    else { $paramWarnings++ }
}
$structuralErrors = 0
$structuralWarnings = 0
foreach ($v in $structuralViolations) {
    if ($v.Severity -eq "ERROR") { $structuralErrors++ }
    else { $structuralWarnings++ }
}

$totalErrors = $paramErrors + $structuralErrors
$totalWarnings = $paramWarnings + $structuralWarnings
$totalViolations = $violations.Count + $structuralViolations.Count

Write-Host "  Total violations found: $totalViolations" -ForegroundColor $(if ($totalViolations -eq 0) { 'Green' } else { 'Red' })
Write-Host "    [ERROR] ERRORS: $totalErrors (blocking compilation)" -ForegroundColor $(if ($totalErrors -eq 0) { 'Green' } else { 'Red' })
Write-Host "    [WARNING]  WARNINGS: $totalWarnings (documentation issues)" -ForegroundColor $(if ($totalWarnings -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "  Parameter violations: $($violations.Count) ($paramErrors errors, $paramWarnings warnings)" -ForegroundColor $(if ($violations.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "  Structural violations: $($structuralViolations.Count) ($structuralErrors errors, $structuralWarnings warnings)" -ForegroundColor $(if ($structuralViolations.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host "  Files scanned: $($filesToScan.Count) (Razor: $($razorFilesList.Count), MD: $($mdFilesList.Count), HTML: $($htmlFilesList.Count))" -ForegroundColor White
Write-Host "  R* components found: $totalComponentsFound" -ForegroundColor White
Write-Host "  Component types validated: $($componentParameters.Count)" -ForegroundColor White

if ($totalViolations -eq 0) {
    Write-Host "Component parameters: 0 violations in $($componentsScanned) components" -ForegroundColor Gray
    return @{ Success = $true; Violations = @(); StructuralViolations = @(); ErrorCount = 0; WarningCount = 0 }
}

# Group violations by component
$violationsByComponent = @()
if ($violations.Count -gt 0) {
    $violationsByComponent = $violations | Group-Object Component | Sort-Object Count -Descending
}

Write-Host "`n VIOLATIONS BY COMPONENT:" -ForegroundColor Red
foreach ($group in $violationsByComponent) {
    $componentName = $group.Name
    $count = $group.Count
    $invalidParams = ($group.Group | Select-Object -ExpandProperty InvalidParameter | Sort-Object -Unique) -join ', '
    
    Write-Host "  $componentName ($count violations)" -ForegroundColor Red
    Write-Host "    Invalid parameters: $invalidParams" -ForegroundColor Yellow
}

# Auto-fix if requested
if ($Fix) {
    Write-Host "`n AUTO-FIXING violations..." -ForegroundColor Yellow
    
    $fixedFiles = @{}
    $fixCount = 0
    
    foreach ($violation in $violations) {
        # Skip warnings in MD and HTML files
        if ($violation.Severity -ne "ERROR") {
            continue
        }
        
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
        Write-Host "   Removed '$paramToRemove' from $($violation.Component) in $($violation.File)" -ForegroundColor Green
    }
    
    # Write fixed files
    foreach ($filePath in $fixedFiles.Keys) {
        Set-Content -Path $filePath -Value $fixedFiles[$filePath] -Encoding UTF8 -NoNewline
    }
    
    Write-Host "   Fixed $fixCount invalid parameters in $($fixedFiles.Count) files" -ForegroundColor Green
}

return @{
    Success = $totalErrors -eq 0  # Success only if no errors (warnings are acceptable)
    Violations = @($violations)
    StructuralViolations = @($structuralViolations)
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


