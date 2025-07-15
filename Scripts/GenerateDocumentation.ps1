#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates 2 separate AI-optimized documentation files for RR.Blazor components and styles
    
.DESCRIPTION
    Creates rr-ai-styles.json (utility classes & CSS variables) and rr-ai-components.json (R* components)
    with AI instructions at the top and concise formatting for AI consumption.
    
.PARAMETER ProjectPath
    Path to the RR.Blazor project directory
    
.PARAMETER StylesOutputPath
    Output path for the styles JSON documentation
    
.PARAMETER ComponentsOutputPath
    Output path for the components JSON documentation
    
.EXAMPLE
    .\GenerateDocumentation.ps1 -ProjectPath "C:\Projects\PayrollAI\RR.Blazor"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory = $false)]
    [string]$StylesOutputPath = "wwwroot/rr-ai-styles.json",
    
    [Parameter(Mandatory = $false)]
    [string]$ComponentsOutputPath = "wwwroot/rr-ai-components.json"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Generating AI-First Documentation (2 files)..." -ForegroundColor Cyan

# Normalize paths
$ProjectPath = Resolve-Path $ProjectPath
$StylesOutputPath = if ([System.IO.Path]::IsPathRooted($StylesOutputPath)) {
    [System.IO.Path]::GetFullPath($StylesOutputPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $ProjectPath $StylesOutputPath))
}

$ComponentsOutputPath = if ([System.IO.Path]::IsPathRooted($ComponentsOutputPath)) {
    [System.IO.Path]::GetFullPath($ComponentsOutputPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $ProjectPath $ComponentsOutputPath))
}

# Create output directories
$StylesOutputDir = Split-Path $StylesOutputPath -Parent
$ComponentsOutputDir = Split-Path $ComponentsOutputPath -Parent
@($StylesOutputDir, $ComponentsOutputDir) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -Path $_ -ItemType Directory -Force | Out-Null
    }
}

# ===============================
# GENERATE STYLES DOCUMENTATION
# ===============================
Write-Host "🎨 Generating styles documentation from SCSS files..." -ForegroundColor Yellow

# Function to extract CSS classes and variables from SCSS files
function Extract-ScssClasses {
    param([string]$ScssDirectory)
    
    $classes = @{}
    $variables = @{}
    
    $scssFiles = Get-ChildItem -Path $ScssDirectory -Filter "*.scss" -Recurse
    Write-Host "  Found $($scssFiles.Count) SCSS files" -ForegroundColor DarkGray
    
    foreach ($file in $scssFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Extract CSS classes (.class-name)
        $classMatches = [regex]::Matches($content, '\.([a-zA-Z0-9_-]+(?:\[[^\]]+\])?)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $classMatches) {
            $className = $match.Groups[1].Value
            if (-not $classes.ContainsKey($className)) {
                $classes[$className] = @{
                    "file" = $file.Name
                    "category" = "extracted"
                }
            }
        }
        
        # Extract CSS variables (--var-name)
        $varMatches = [regex]::Matches($content, '--([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $varMatches) {
            $varName = $match.Groups[1].Value
            if (-not $variables.ContainsKey($varName)) {
                $variables[$varName] = @{
                    "file" = $file.Name
                    "category" = "extracted"
                }
            }
        }
    }
    
    return @{
        "classes" = $classes
        "variables" = $variables
    }
}

# Extract actual classes and variables from SCSS files
$stylesPath = Join-Path $ProjectPath "Styles"
if (Test-Path $stylesPath) {
    $extractedStyles = Extract-ScssClasses -ScssDirectory $stylesPath
    Write-Host "  Extracted $($extractedStyles.classes.Count) classes and $($extractedStyles.variables.Count) variables" -ForegroundColor DarkGray
} else {
    Write-Host "  Warning: Styles directory not found at $stylesPath" -ForegroundColor Yellow
    $extractedStyles = @{ "classes" = @{}; "variables" = @{} }
}

# Function to generate bracket notation patterns from actual classes
function Generate-BracketPatterns {
    param([hashtable]$Classes)
    
    $patterns = @{}
    
    foreach ($className in $Classes.Keys) {
        $category = "other"
        
        # Categorize and extract patterns
        if ($className -match '^(p|m|pa|ma|pt|pb|pl|pr|px|py|mt|mb|ml|mr|mx|my|gap)-(.+)$') { 
            $category = "spacing"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(d|flex|justify|align|grid|col|row)-(.+)$') { 
            $category = "layout"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(text|font|leading|tracking)-(.+)$') { 
            $category = "typography"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(bg|border|rounded)-(.+)$') { 
            $category = "appearance"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(w|h|min|max)-(.+)$') { 
            $category = "sizing"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(elevation|shadow|glass|backdrop)-(.+)$') { 
            $category = "effects"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(cursor|user-select|pointer)-(.+)$') { 
            $category = "interactive"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
        elseif ($className -match '^(animate|transition|duration|ease)-(.+)$') { 
            $category = "animations"
            $prefix = $Matches[1]
            $value = $Matches[2]
            if (-not $patterns.ContainsKey($category)) { $patterns[$category] = @{} }
            if (-not $patterns[$category].ContainsKey($prefix)) { $patterns[$category][$prefix] = @() }
            $patterns[$category][$prefix] += $value
        }
    }
    
    # Convert to bracket notation
    $bracketPatterns = @{}
    foreach ($category in $patterns.Keys) {
        $bracketPatterns[$category] = @()
        foreach ($prefix in $patterns[$category].Keys) {
            $values = $patterns[$category][$prefix] | Sort-Object -Unique
            $bracketNotation = "$prefix-[$($values -join ', ')]"
            $bracketPatterns[$category] += $bracketNotation
        }
    }
    
    return $bracketPatterns
}

# Generate bracket notation patterns
$bracketPatterns = Generate-BracketPatterns -Classes $extractedStyles.classes

# Group variables by category with bracket notation
$categorizedVars = @{}
foreach ($varName in $extractedStyles.variables.Keys) {
    $category = "other"
    
    if ($varName -match '^(space|spacing)') { $category = "spacing" }
    elseif ($varName -match '^(color|text|bg|background|border)') { $category = "colors" }
    elseif ($varName -match '^(font|text|leading|tracking)') { $category = "typography" }
    elseif ($varName -match '^(shadow|elevation)') { $category = "elevation" }
    elseif ($varName -match '^(radius|border-radius)') { $category = "borders" }
    
    if (-not $categorizedVars.ContainsKey($category)) {
        $categorizedVars[$category] = @()
    }
    $categorizedVars[$category] += $varName
}

$stylesDoc = [ordered]@{
    "_ai_instructions" = @{
        "CRITICAL" = "You must use bracket notation for extrapolation. You are required to understand patterns like justify-[start, center, end, between] means justify-start, justify-center, justify-end, justify-between"
        "NAVIGATION" = @{
            "ai_instructions" = "Lines 1-15: You must read these AI instructions and bracket notation rules first"
            "utility_patterns" = "Lines 16-80: You must use these utility classes in bracket notation for extrapolation"
            "css_variables" = "Lines 81-120: You must use these CSS variables grouped by category"
        }
        "BRACKET_NOTATION" = @{
            "FORMAT" = "You must understand that prefix-[value1, value2, value3, ...] means prefix-value1, prefix-value2, prefix-value3, etc."
            "EXAMPLES" = @(
                "You must know that p-[0, 1, 2, 4, 8] means p-0, p-1, p-2, p-4, p-8",
                "You must know that text-[xs, sm, base, lg, xl] means text-xs, text-sm, text-base, text-lg, text-xl",
                "You must know that justify-[start, center, end, between] means justify-start, justify-center, justify-end, justify-between"
            )
        }
        "USAGE_DIRECTIVE" = "You must extrapolate from these patterns to generate the exact classes needed. You are not allowed to invent classes that don't exist in these patterns."
        "EXTRACTION_INFO" = @{
            "total_classes" = $extractedStyles.classes.Count
            "total_variables" = $extractedStyles.variables.Count
            "source_directory" = "RR.Blazor/Styles/"
        }
    }
    
    "utility_patterns" = $bracketPatterns
    "css_variables" = $categorizedVars
}

# Generate styles JSON
$stylesJson = $stylesDoc | ConvertTo-Json -Depth 10 -Compress:$false
$stylesJson = $stylesJson -replace '\\u003c', '<' -replace '\\u003e', '>'
$stylesJson | Out-File -FilePath $StylesOutputPath -Encoding UTF8 -Force

Write-Host "✅ Styles documentation generated: $StylesOutputPath" -ForegroundColor Green

# PARAMETER EXTRACTION FUNCTION
function Extract-ComponentParameters {
    param(
        [string]$Content,
        [string]$ComponentName
    )
    
    $parameters = New-Object System.Collections.ArrayList
    $lines = $Content -split "`n"
    
    # Track @code block state
    $inCodeBlock = $false
    $codeBlockDepth = 0
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i].Trim()
        
        # Track @code blocks
        if ($line -match '@code\s*{') {
            $inCodeBlock = $true
            $codeBlockDepth = 1
            continue
        }
        
        if ($inCodeBlock) {
            # Count braces to handle nested blocks
            $openBraces = ($line -split '\{').Count - 1
            $closeBraces = ($line -split '\}').Count - 1
            $codeBlockDepth = $codeBlockDepth + ($openBraces - $closeBraces)
            
            if ($codeBlockDepth -le 0) {
                $inCodeBlock = $false
                $codeBlockDepth = 0
                continue
            }
        }
        
        # Look for [Parameter] attributes
        if ($line -match '\[Parameter(?:[\],]|$)') {
            # Collect parameter definition lines
            $fullParameterText = ""
            $propertyFound = $false
            
            # Look for XML documentation above
            $description = ""
            $aiHint = ""
            for ($k = $i - 1; $k -ge [Math]::Max(0, $i - 10); $k--) {
                $docLine = $lines[$k].Trim()
                if ($docLine -match '^$') { continue }
                if ($docLine -match '///\s*<summary>(.*?)</summary>') {
                    $description = $Matches[1].Trim()
                    break
                }
                if ($docLine -match '///\s*<ai-hint>(.*?)</ai-hint>') {
                    $aiHint = $Matches[1].Trim()
                    continue
                }
                if ($docLine -match '///\s*(.+)') {
                    $description = $Matches[1].Trim()
                    break
                }
            }
            
            # Collect parameter definition lines
            for ($j = $i; $j -lt [Math]::Min($lines.Length, $i + 20); $j++) {
                $currentLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($currentLine)) { continue }
                
                $fullParameterText = $fullParameterText + " " + $currentLine
                
                # Check if we found the property declaration
                if ($currentLine -match 'public\s+[^{]+\{\s*get;\s*set;\s*\}') {
                    $propertyFound = $true
                    break
                }
            }
            
            if (-not $propertyFound) { continue }
            
            # Extract parameter info
            if ($fullParameterText -match 'public\s+(?:virtual\s+)?(?:override\s+)?(?:static\s+)?([^\s]+(?:<[^>]*>)?(?:\?)?\s*)\s+(@?\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+))?;?') {
                $paramType = $Matches[1].Trim()
                $paramName = $Matches[2].Trim()
                $defaultValue = if ($Matches[3]) { $Matches[3].Trim() } else { $null }
                
                # Extract AIParameter hint
                if ($fullParameterText -match '\[AIParameter\(\s*["]([^"]*)["]') {
                    $aiHint = $Matches[1]
                }
                elseif ($fullParameterText -match '\[AIParameter\(\s*Hint\s*=\s*["]([^"]*)["]') {
                    $aiHint = $Matches[1]
                }
                
                # Extract suggested values
                $suggestedValues = @()
                if ($fullParameterText -match 'SuggestedValues\s*=\s*new\[\]\s*\{\s*([^}]+)\}') {
                    $valuesText = $Matches[1]
                    $suggestedValues = @($valuesText -split ',' | ForEach-Object { $_.Trim().Trim('"') })
                }
                
                # Check if required
                $isRequired = $false
                if ($fullParameterText -match 'Required\s*=\s*true') {
                    $isRequired = $true
                }
                
                $param = [PSCustomObject]@{
                    Name = $paramName
                    Type = $paramType
                    Description = $description
                    AIHint = $aiHint
                    SuggestedValues = $suggestedValues
                    IsRequired = $isRequired
                    DefaultValue = $defaultValue
                }
                
                $parameters.Add($param) | Out-Null
            }
        }
    }
    
    return $parameters
}

# ===============================
# GENERATE COMPONENTS DOCUMENTATION
# ===============================
Write-Host "📂 Generating components documentation..." -ForegroundColor Yellow

$componentFiles = Get-ChildItem -Path "$ProjectPath/Components" -Filter "R*.razor" -Recurse
Write-Host "  Found $($componentFiles.Count) R* component files" -ForegroundColor DarkGray

$components = @{}

foreach ($file in $componentFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
    # Extract purpose from @** blocks
    $purpose = ""
    if ($content -match '(?s)@\*\*\s*(.*?)\*\*@') {
        $aiBlock = $Matches[1]
        if ($aiBlock -match '(?s)<summary>(.*?)</summary>') { 
            $purpose = $Matches[1].Trim() -replace '\s+', ' ' 
        }
        elseif ($aiBlock -match '(?s)<ai-prompt>(.*?)</ai-prompt>') { 
            $purpose = $Matches[1].Trim() 
        }
    }
    
    # Fallback to XML comments
    if ([string]::IsNullOrEmpty($purpose)) {
        if ($content -match '(?s)///\s*<summary>(.*?)</summary>') {
            $purpose = $Matches[1].Trim() -replace '\s+', ' '
        }
    }
    
    # Extract essential parameters
    $parameters = Extract-ComponentParameters -Content $content -ComponentName $componentName
    $essentialParams = @()
    
    foreach ($param in $parameters) {
        # Include only essential parameters
        $isEssential = $param.AIHint -or $param.IsRequired -or 
                      $param.Name -in @('Text', 'Icon', 'Variant', 'Size', 'OnClick', 'Disabled', 'Loading', 'Value', 'Label', 'Title', 'Content', 'Items', 'ChildContent')
        
        if ($isEssential) {
            $paramDesc = $param.Name + ": " + $param.Type
            if ($param.AIHint) {
                $paramDesc += " - " + $param.AIHint
            }
            if ($param.SuggestedValues.Count -gt 0) {
                $paramDesc += " [" + ($param.SuggestedValues -join ', ') + "]"
            }
            $essentialParams += $paramDesc
        }
    }
    
    # Create structured format according to requirements
    $componentInfo = [ordered]@{
        "Purpose" = if ([string]::IsNullOrEmpty($purpose)) { "UI component" } else { $purpose }
        "Parameters" = [ordered]@{}
    }
    
    # Add essential parameters with proper formatting
    foreach ($param in $parameters) {
        $isEssential = $param.AIHint -or $param.IsRequired -or 
                      $param.Name -in @('Text', 'Icon', 'Variant', 'Size', 'OnClick', 'Disabled', 'Loading', 'Value', 'Label', 'Title', 'Content', 'Items', 'ChildContent')
        
        if ($isEssential) {
            $paramValue = $param.Type
            
            # Add enum values in bracket notation if available
            if ($param.SuggestedValues.Count -gt 0) {
                $paramValue += "[" + ($param.SuggestedValues -join ', ') + "]"
            }
            
            # Add AI hint
            if ($param.AIHint) {
                $paramValue += " - " + $param.AIHint
            }
            
            $componentInfo.Parameters[$param.Name] = $paramValue
        }
    }
    
    $components[$componentName] = $componentInfo
}

$componentsDoc = [ordered]@{
    "_ai_instructions" = @{
        "CRITICAL" = "You must use this exact component format for UI generation"
        "NAVIGATION" = @{
            "ai_instructions" = "Lines 1-15: You must read these AI instructions first"
            "components" = "Lines 16+: Components with structured format and essential parameters"
        }
        "USAGE_DIRECTIVE" = "You must use <RComponentName Parameter1='value' Parameter2='value' /> in Blazor markup"
        "COMPONENT_FORMAT" = "Each component has Purpose and Parameters with Type, enum values [brackets], and AI hints"
        "ESSENTIAL_ONLY" = "You are only shown essential parameters with AI hints. Standard Blazor parameters (@bind-*, :after, etc.) are available but not documented."
        "EXTRACTION_INFO" = @{
            "total_components" = $components.Count
            "source_directory" = "RR.Blazor/Components/"
        }
    }
    
    "components" = $components
}

# Generate components JSON
$componentsJson = $componentsDoc | ConvertTo-Json -Depth 10 -Compress:$false
$componentsJson = $componentsJson -replace '\\u003c', '<' -replace '\\u003e', '>'
$componentsJson | Out-File -FilePath $ComponentsOutputPath -Encoding UTF8 -Force

Write-Host "✅ Components documentation generated: $ComponentsOutputPath" -ForegroundColor Green

# Final summary
$componentCount = $components.Count
$totalParameters = ($components.Values | ForEach-Object { $_.parameters.Count } | Measure-Object -Sum).Sum

Write-Host "🎉 Documentation generation completed!" -ForegroundColor Cyan
Write-Host "📊 Statistics:" -ForegroundColor White
Write-Host "  • R* Components: $componentCount" -ForegroundColor White
Write-Host "  • Essential Parameters: $totalParameters" -ForegroundColor White
Write-Host "  • Extracted Classes: $($extractedStyles.classes.Count)" -ForegroundColor White
Write-Host "  • Extracted Variables: $($extractedStyles.variables.Count)" -ForegroundColor White
Write-Host "  • Styles Output: $StylesOutputPath" -ForegroundColor White
Write-Host "  • Components Output: $ComponentsOutputPath" -ForegroundColor White

return @{
    Success = $true
    StylesOutputPath = $StylesOutputPath
    ComponentsOutputPath = $ComponentsOutputPath
    ComponentCount = $componentCount
    ParameterCount = $totalParameters
    ExtractedClasses = $extractedStyles.classes.Count
    ExtractedVariables = $extractedStyles.variables.Count
}