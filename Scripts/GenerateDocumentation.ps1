#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates AI-optimized documentation from compiled CSS - TRULY GENERIC approach
    
.DESCRIPTION
    Extracts ALL utility classes and CSS variables from compiled CSS without hardcoded patterns.
    Uses pattern recognition to automatically categorize and group utilities.
    
.PARAMETER ProjectPath
    Path to the RR.Blazor project directory
    
.PARAMETER StylesOutputPath
    Output path for the styles JSON documentation
    
.PARAMETER ComponentsOutputPath
    Output path for the components JSON documentation
    
.EXAMPLE
    .\GenerateDocumentation.ps1 -ProjectPath "C:\Projects\MyApp\RR.Blazor"
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

Write-Host "🚀 Generating AI-First Documentation (GENERIC approach)..." -ForegroundColor Cyan

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
# GENERIC CSS EXTRACTION
# ===============================
Write-Host "🎨 Extracting ALL utilities from compiled CSS..." -ForegroundColor Yellow

function Extract-AllFromCompiledCSS {
    param([string]$CssFilePath)
    
    if (-not (Test-Path $CssFilePath)) {
        throw "CSS file not found at $CssFilePath. Run 'dotnet build' first."
    }
    
    $cssContent = Get-Content $CssFilePath -Raw -Encoding UTF8
    
    # Check if CSS is valid
    if ($cssContent -match "^/\* Error:" -or $cssContent -notmatch "\.[a-zA-Z]") {
        throw "CSS compilation failed. Fix build errors first."
    }
    
    Write-Host "  Extracting from compiled CSS: $CssFilePath" -ForegroundColor DarkGray
    
    # Extract ALL CSS variables (--anything)
    $variables = @{}
    $varMatches = [regex]::Matches($cssContent, '--([a-zA-Z0-9_-]+)\s*:', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $varMatches) {
        $varName = $match.Groups[1].Value
        if ($varName.Length -gt 0 -and $varName -notmatch '[-_]$') {
            $variables[$varName] = $true
        }
    }
    
    # Extract ALL CSS classes (.anything)
    $classes = @{}
    $classMatches = [regex]::Matches($cssContent, '\.([a-zA-Z][a-zA-Z0-9_-]*(?:\\:[a-zA-Z0-9_-]+)*)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $classMatches) {
        $fullClassName = $match.Groups[1].Value
        # Clean escape sequences and pseudo-classes
        $baseClassName = $fullClassName -replace '\\:', ':' -replace ':.*$', ''
        
        if ($baseClassName.Length -gt 0 -and $baseClassName -match '^[a-zA-Z]') {
            $classes[$baseClassName] = $true
        }
    }
    
    Write-Host "  Extracted $($classes.Count) classes and $($variables.Count) variables" -ForegroundColor DarkGray
    
    return @{
        "classes" = $classes
        "variables" = $variables
    }
}

# Extract everything from compiled CSS
$cssPath = Join-Path $ProjectPath "wwwroot\css\main.css"
$extracted = Extract-AllFromCompiledCSS -CssFilePath $cssPath

# ===============================
# GENERIC PATTERN RECOGNITION
# ===============================
Write-Host "🔍 Auto-discovering utility patterns..." -ForegroundColor Yellow

function Auto-DiscoverPatterns {
    param([hashtable]$Classes)
    
    $patterns = @{}
    $standalone = @{}
    
    $classNames = @($Classes.Keys)
    foreach ($className in $classNames) {
        # Handle fractional patterns first (e.g., gap-0-5, w-1-5, min-w-0-5)
        if ($className -match '^([a-zA-Z-]+)-(\d+-\d+)$') {
            $prefix = $Matches[1]
            $suffix = $Matches[2]
            
            if (-not $patterns.ContainsKey($prefix)) { $patterns[$prefix] = @() }
            $patterns[$prefix] += $suffix
        }
        # Handle responsive patterns (e.g., sm:gap-4, md:w-full)
        elseif ($className -match '^([a-zA-Z]+):([a-zA-Z-]+)$') {
            $breakpoint = $Matches[1]
            $utility = $Matches[2]
            $responsivePrefix = "$breakpoint`:$utility"
            
            # Extract the base utility pattern
            if ($utility -match '^([a-zA-Z]+)-(.+)$') {
                $utilityPrefix = $Matches[1]
                $utilitySuffix = $Matches[2]
                $fullPrefix = "$breakpoint`:$utilityPrefix"
                
                if (-not $patterns.ContainsKey($fullPrefix)) { $patterns[$fullPrefix] = @() }
                $patterns[$fullPrefix] += $utilitySuffix
            }
        }
        else {
            # Split by dash to find prefix-suffix patterns
            $parts = $className -split '-'
            
            if ($parts.Count -eq 1) {
                # Standalone class (no dashes)
                $category = "standalone"
                if (-not $standalone.ContainsKey($category)) { $standalone[$category] = @() }
                $standalone[$category] += $className
            }
            elseif ($parts.Count -eq 2) {
                # Standard prefix-suffix pattern
                $prefix = $parts[0]
                $suffix = $parts[1]
                
                if (-not $patterns.ContainsKey($prefix)) { $patterns[$prefix] = @() }
                $patterns[$prefix] += $suffix
            }
            elseif ($parts.Count -ge 3) {
                # Multi-part classes: try different combinations
                # Try prefix-rest (e.g., "text-shadow-lg" -> "text-shadow": ["lg"])
                $prefix = ($parts[0..($parts.Count-2)] -join '-')
                $suffix = $parts[-1]
                
                if (-not $patterns.ContainsKey($prefix)) { $patterns[$prefix] = @() }
                $patterns[$prefix] += $suffix
            }
        }
    }
    
    # Convert to bracket notation
    $bracketPatterns = @{}
    $prefixes = @($patterns.Keys)
    foreach ($prefix in $prefixes) {
        $values = $patterns[$prefix] | Sort-Object -Unique
        $bracketPatterns[$prefix] = "$prefix-[" + ($values -join ', ') + "]"
    }
    
    # Add standalone classes
    $categories = @($standalone.Keys)
    foreach ($category in $categories) {
        $values = $standalone[$category] | Sort-Object -Unique
        $bracketPatterns[$category] = "[" + ($values -join ', ') + "]"
    }
    
    Write-Host "  Auto-discovered $($bracketPatterns.Count) utility patterns" -ForegroundColor DarkGray
    
    return $bracketPatterns
}

# Auto-discover all patterns
$utilityPatterns = Auto-DiscoverPatterns -Classes $extracted.classes

# ===============================
# GENERIC VARIABLE GROUPING
# ===============================
Write-Host "🏷️ Auto-grouping CSS variables..." -ForegroundColor Yellow

function Auto-GroupVariables {
    param([hashtable]$Variables)
    
    $groups = @{}
    
    $varNames = @($Variables.Keys)
    foreach ($varName in $varNames) {
        # Auto-detect category from variable name
        $category = "other"
        
        # Use first word as category
        if ($varName -match '^([a-z]+)-') {
            $category = $Matches[1]
        }
        
        if (-not $groups.ContainsKey($category)) {
            $groups[$category] = @()
        }
        $groups[$category] += "--$varName"
    }
    
    # Sort variables within each category
    $categories = @($groups.Keys)
    foreach ($category in $categories) {
        $groups[$category] = $groups[$category] | Sort-Object -Unique
    }
    
    Write-Host "  Auto-grouped variables into $($groups.Count) categories" -ForegroundColor DarkGray
    
    return $groups
}

# Auto-group all variables
$variableGroups = Auto-GroupVariables -Variables $extracted.variables

# ===============================
# GENERATE STYLES DOCUMENTATION
# ===============================
$stylesDoc = [ordered]@{
    "_ai_instructions" = @{
        "CRITICAL" = "Use bracket notation for extrapolation. pattern-[value1, value2] means pattern-value1, pattern-value2"
        "EXTRACTION_METHOD" = "Fully generic extraction from compiled CSS - no hardcoded patterns"
        "NAVIGATION" = @{
            "ai_instructions" = "Lines 1-15: Read AI instructions and bracket notation rules"
            "utility_patterns" = "Lines 16-80: Auto-discovered utility patterns in bracket notation"
            "css_variables" = "Lines 81+: Auto-grouped CSS custom properties"
        }
        "BRACKET_NOTATION" = @{
            "FORMAT" = "prefix-[value1, value2, value3] means prefix-value1, prefix-value2, prefix-value3"
            "EXAMPLES" = @(
                "p-[0, 1, 2, 4, 8] means p-0, p-1, p-2, p-4, p-8",
                "text-[xs, sm, base, lg, xl] means text-xs, text-sm, text-base, text-lg, text-xl",
                "opacity-[0, 25, 50, 75, 100] means opacity-0, opacity-25, opacity-50, opacity-75, opacity-100"
            )
        }
        "USAGE_DIRECTIVE" = "Extrapolate from patterns to generate exact classes. Do not invent classes."
        "CSS_VARIABLES_VS_UTILITIES" = @{
            "UTILITY_CLASSES" = "Use utility classes (pa-4, opacity-50) for standard styling"
            "CSS_VARIABLES" = "Use CSS variables (var(--color-primary)) for custom component styling"
            "EXAMPLE" = "Use 'opacity-50' for standard styling, 'opacity: var(--opacity-50)' for custom components"
        }
        "EXTRACTION_INFO" = @{
            "total_classes" = $extracted.classes.Count
            "total_variables" = $extracted.variables.Count
            "total_patterns" = $utilityPatterns.Count
            "extraction_method" = "Generic CSS parsing"
        }
    }
    
    "utility_patterns" = $utilityPatterns
    "css_variables" = $variableGroups
}

# Generate styles JSON
$stylesJson = $stylesDoc | ConvertTo-Json -Depth 10 -Compress:$false
$stylesJson = $stylesJson -replace '\\u003c', '<' -replace '\\u003e', '>'
$stylesJson | Out-File -FilePath $StylesOutputPath -Encoding UTF8 -Force

Write-Host "✅ Styles documentation generated: $StylesOutputPath" -ForegroundColor Green

# ===============================
# COMPONENTS DOCUMENTATION (unchanged)
# ===============================
Write-Host "📂 Generating components documentation..." -ForegroundColor Yellow

# [Component extraction code remains the same as it's already generic]
function Extract-ComponentParameters {
    param([string]$Content, [string]$ComponentName)
    
    $parameters = New-Object System.Collections.ArrayList
    $lines = $Content -split "`n"
    
    $inCodeBlock = $false
    $codeBlockDepth = 0
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i].Trim()
        
        if ($line -match '@code\s*{') {
            $inCodeBlock = $true
            $codeBlockDepth = 1
            continue
        }
        
        if ($inCodeBlock) {
            $openBraces = ($line -split '\{').Count - 1
            $closeBraces = ($line -split '\}').Count - 1
            $codeBlockDepth = $codeBlockDepth + ($openBraces - $closeBraces)
            
            if ($codeBlockDepth -le 0) {
                $inCodeBlock = $false
                $codeBlockDepth = 0
                continue
            }
        }
        
        if ($line -match '\[Parameter(?:[],]|$)') {
            $fullParameterText = ""
            $propertyFound = $false
            
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
            
            for ($j = $i; $j -lt [Math]::Min($lines.Length, $i + 20); $j++) {
                $currentLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($currentLine)) { continue }
                
                $fullParameterText = $fullParameterText + " " + $currentLine
                
                if ($currentLine -match 'public\s+[^{]+\{\s*get;\s*set;\s*\}') {
                    $propertyFound = $true
                    break
                }
            }
            
            if (-not $propertyFound) { continue }
            
            if ($fullParameterText -match 'public\s+(?:virtual\s+)?(?:override\s+)?(?:static\s+)?([^\s]+(?:<[^>]*>)?(?:\?)?\s*)\s+(@?\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+))?;?') {
                $paramType = $Matches[1].Trim()
                $paramName = $Matches[2].Trim()
                $defaultValue = if ($Matches[3]) { $Matches[3].Trim() } else { $null }
                
                if ($fullParameterText -match '\[AIParameter\(\s*["]([^"]*)["]') {
                    $aiHint = $Matches[1]
                }
                elseif ($fullParameterText -match '\[AIParameter\(\s*Hint\s*=\s*["]([^"]*)["]') {
                    $aiHint = $Matches[1]
                }
                
                $suggestedValues = @()
                if ($fullParameterText -match 'SuggestedValues\s*=\s*new\[\]\s*\{\s*([^}]+)\}') {
                    $valuesText = $Matches[1]
                    $suggestedValues = @($valuesText -split ',' | ForEach-Object { $_.Trim().Trim('"') })
                }
                
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

$componentFiles = Get-ChildItem -Path "$ProjectPath/Components" -Filter "R*.razor" -Recurse
Write-Host "  Found $($componentFiles.Count) R* component files" -ForegroundColor DarkGray

$components = @{}

foreach ($file in $componentFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
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
    
    if ([string]::IsNullOrEmpty($purpose)) {
        if ($content -match '(?s)///\s*<summary>(.*?)</summary>') {
            $purpose = $Matches[1].Trim() -replace '\s+', ' '
        }
    }
    
    $parameters = Extract-ComponentParameters -Content $content -ComponentName $componentName
    
    $componentInfo = [ordered]@{
        "Purpose" = if ([string]::IsNullOrEmpty($purpose)) { "UI component" } else { $purpose }
        "Parameters" = [ordered]@{}
    }
    
    foreach ($param in $parameters) {
        $isEssential = $param.AIHint -or $param.IsRequired -or 
                      $param.Name -in @('Text', 'Icon', 'Variant', 'Size', 'OnClick', 'Disabled', 'Loading', 'Value', 'Label', 'Title', 'Content', 'Items', 'ChildContent', 'HeaderContent', 'MediaContent', 'FooterContent')
        
        if ($isEssential) {
            $paramValue = $param.Type
            
            if ($param.SuggestedValues.Count -gt 0) {
                $paramValue += "[" + ($param.SuggestedValues -join ', ') + "]"
            }
            
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
        "CRITICAL" = "Use exact component format for UI generation"
        "NAVIGATION" = @{
            "ai_instructions" = "Lines 1-15: Read AI instructions first"
            "components" = "Lines 16+: Components with structured format and essential parameters"
        }
        "USAGE_DIRECTIVE" = "Use <RComponentName Parameter1='value' Parameter2='value' /> in Blazor markup"
        "COMPONENT_FORMAT" = "Each component has Purpose and Parameters with Type, enum values [brackets], and AI hints"
        "ESSENTIAL_ONLY" = "Only essential parameters with AI hints shown. Standard Blazor parameters available but not documented."
        "EXTRACTION_INFO" = @{
            "total_components" = $components.Count
            "extraction_method" = "Generic component parsing"
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

Write-Host "🎉 GENERIC Documentation generation completed!" -ForegroundColor Cyan
Write-Host "📊 Statistics:" -ForegroundColor White
Write-Host "   R* Components: $componentCount" -ForegroundColor White
Write-Host "   Essential Parameters: $totalParameters" -ForegroundColor White
Write-Host "   Discovered Patterns: $($utilityPatterns.Count)" -ForegroundColor White
Write-Host "   Extracted Classes: $($extracted.classes.Count)" -ForegroundColor White
Write-Host "   Extracted Variables: $($extracted.variables.Count)" -ForegroundColor White
Write-Host "   Styles Output: $StylesOutputPath" -ForegroundColor White
Write-Host "   Components Output: $ComponentsOutputPath" -ForegroundColor White

return @{
    Success = $true
    StylesOutputPath = $StylesOutputPath
    ComponentsOutputPath = $ComponentsOutputPath
    ComponentCount = $componentCount
    ParameterCount = $totalParameters
    ExtractedClasses = $extracted.classes.Count
    ExtractedVariables = $extracted.variables.Count
}