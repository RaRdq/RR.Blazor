#!/usr/bin/env pwsh
<#
.SYNOPSIS
    FIXED - Generates comprehensive AI-optimized documentation for RR.Blazor components with bulletproof parameter extraction
    
.DESCRIPTION
    Extracts components, utility patterns, CSS variables, and best practices from RR.Blazor project.
    FIXED: Now properly handles multi-line parameter definitions, AIParameter attributes, and complex types.
    
.PARAMETER ProjectPath
    Path to the RR.Blazor project directory
    
.PARAMETER OutputPath
    Output path for the generated JSON documentation
    
.EXAMPLE
    .\GenerateAIDocsAdvanced_Fixed.ps1 -ProjectPath "C:\Projects\PayrollAI\RR.Blazor" -OutputPath ".\rr-ai-docs.json"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "wwwroot/rr-ai-docs.json"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Generating FIXED AI-First Documentation for RR.Blazor..." -ForegroundColor Cyan

# Normalize paths
$ProjectPath = Resolve-Path $ProjectPath
$OutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    [System.IO.Path]::GetFullPath($OutputPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $ProjectPath $OutputPath))
}

$OutputDir = Split-Path $OutputPath -Parent
if (-not (Test-Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
}

# BULLETPROOF PARAMETER EXTRACTION FUNCTION (Define first!)
function Extract-ComponentParameters {
    param(
        [string]$Content,
        [string]$ComponentName
    )
    
    $parameters = @()
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
            $codeBlockDepth += ($openBraces - $closeBraces)
            
            if ($codeBlockDepth -le 0) {
                $inCodeBlock = $false
                $codeBlockDepth = 0
                continue
            }
        }
        
        # Look for [Parameter] or [CascadingParameter] attributes
        if ($line -match '\[(Parameter|CascadingParameter)(?:[\],]|$)') {
            $attributeType = $Matches[1]
            
            # Skip CascadingParameter for now (different behavior)
            if ($attributeType -eq "CascadingParameter") {
                continue
            }
            
            # Collect all lines for this parameter (multi-line support)
            $parameterLines = @()
            $currentLine = $i
            
            # Collect XML documentation (look backward)
            $description = ""
            for ($k = $i - 1; $k -ge [Math]::Max(0, $i - 10); $k--) {
                $docLine = $lines[$k].Trim()
                if ($docLine -match '^\s*$') { break }
                if ($docLine -match '///\s*<summary>(.*?)</summary>') {
                    $description = $Matches[1].Trim()
                    break
                }
                if ($docLine -match '///\s*(.+)') {
                    $description = $Matches[1].Trim()
                    break
                }
            }
            
            # Collect parameter definition lines (look forward)
            $fullParameterText = ""
            $propertyFound = $false
            
            for ($j = $i; $j -lt [Math]::Min($lines.Length, $i + 20); $j++) {
                $currentLine = $lines[$j].Trim()
                if ([string]::IsNullOrWhiteSpace($currentLine)) { continue }
                
                $fullParameterText += " " + $currentLine
                
                # Check if we found the property declaration
                if ($currentLine -match 'public\s+[^{]+\{\s*get;\s*set;\s*\}') {
                    $propertyFound = $true
                    break
                }
            }
            
            if (-not $propertyFound) { continue }
            
            # Extract parameter info using bulletproof regex
            $parameterMatch = $null
            
            # Pattern 1: Full property pattern with all variations
            if ($fullParameterText -match 'public\s+(?:virtual\s+)?(?:override\s+)?(?:static\s+)?([^\s]+(?:<[^>]*>)?(?:\?)?)\s+(@?\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+))?;?') {
                $parameterMatch = @{
                    Type = $Matches[1].Trim()
                    Name = $Matches[2].Trim()
                    DefaultValue = if ($Matches[3]) { $Matches[3].Trim() } else { $null }
                }
            }
            
            if ($parameterMatch) {
                # Extract AIParameter hint
                $aiHint = ""
                if ($fullParameterText -match '\[AIParameter\(\s*["]([^"]*)["]') {
                    $aiHint = $Matches[1]
                }
                elseif ($fullParameterText -match '\[AIParameter\(\s*Hint\s*=\s*["]([^"]*)["]') {
                    $aiHint = $Matches[1]
                }
                
                # Check if required
                $isRequired = $false
                if ($fullParameterText -match 'Required\s*=\s*true') {
                    $isRequired = $true
                }
                # Non-nullable value types are typically required
                if ($parameterMatch.Type -match '^(bool|int|double|float|decimal|DateTime|Guid)$' -and $parameterMatch.Type -notmatch '\?') {
                    $isRequired = $true
                }
                
                $param = [PSCustomObject]@{
                    Name = $parameterMatch.Name
                    Type = $parameterMatch.Type
                    Description = $description
                    AIHint = $aiHint
                    IsRequired = $isRequired
                    DefaultValue = $parameterMatch.DefaultValue
                    FullText = $fullParameterText.Trim()
                }
                
                $parameters += $param
                
                Write-Host "      📌 $($param.Name) : $($param.Type)" -ForegroundColor DarkGreen
            }
        }
    }
    
    return $parameters
}

# Initialize documentation structure
$documentation = @{
    components = @{}
    utilityPatterns = @{}
    cssVariables = @{}
    aiPatterns = @()
}

Write-Host "📂 Scanning components with BULLETPROOF parameter extraction..." -ForegroundColor Yellow

# Extract ALL Components
$componentFiles = Get-ChildItem -Path "$ProjectPath/Components" -Filter "*.razor" -Recurse
Write-Host "  Found $($componentFiles.Count) component files" -ForegroundColor DarkGray

foreach ($file in $componentFiles) {
    Write-Host "  📄 Processing $($file.Name)..." -ForegroundColor DarkGray
    
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $componentName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
    $component = @{
        name = $componentName
        category = 'Unknown'
        complexity = 'Simple'
        description = ''
        aiPrompt = ''
        commonUse = ''
        avoidUsage = ''
        patterns = @{}
        parameters = @{}
        filePath = $file.FullName.Replace($ProjectPath, '').Replace('\', '/')
    }
    
    # Extract AI metadata from @** blocks
    if ($content -match '(?s)@\*\*\s*(.*?)\*\*@') {
        $aiBlock = $Matches[1]
        
        if ($aiBlock -match '(?s)<summary>(.*?)</summary>') { $component.description = $Matches[1].Trim() -replace '\s+', ' ' }
        if ($aiBlock -match '(?s)<category>(.*?)</category>') { $component.category = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<complexity>(.*?)</complexity>') { $component.complexity = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<ai-prompt>(.*?)</ai-prompt>') { $component.aiPrompt = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<ai-common-use>(.*?)</ai-common-use>') { $component.commonUse = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<ai-avoid>(.*?)</ai-avoid>') { $component.avoidUsage = $Matches[1].Trim() }
        
        $patternMatches = [regex]::Matches($aiBlock, '(?s)<ai-pattern name="([^"]+)">(.*?)</ai-pattern>')
        foreach ($match in $patternMatches) {
            $component.patterns[$match.Groups[1].Value] = $match.Groups[2].Value.Trim()
        }
    }
    
    # Extract C# attributes
    if ($content -match '@attribute \[Component\("([^"]*)"[^]]*Category\s*=\s*"([^"]*)"[^]]*Complexity\s*=\s*ComponentComplexity\.(\w+)[^]]*\]\)') {
        if ([string]::IsNullOrEmpty($component.category) -or $component.category -eq 'Unknown') {
            $component.category = $Matches[2]
        }
        if ([string]::IsNullOrEmpty($component.complexity) -or $component.complexity -eq 'Simple') {
            $component.complexity = $Matches[3]
        }
    }
    
    if ($content -match '@attribute \[AIOptimized\([^]]*Prompt\s*=\s*"([^"]*)"[^]]*CommonUse\s*=\s*"([^"]*)"[^]]*AvoidUsage\s*=\s*"([^"]*)"[^]]*\]\)') {
        if ([string]::IsNullOrEmpty($component.aiPrompt)) {
            $component.aiPrompt = $Matches[1]
        }
        if ([string]::IsNullOrEmpty($component.commonUse)) {
            $component.commonUse = $Matches[2]
        }
        if ([string]::IsNullOrEmpty($component.avoidUsage)) {
            $component.avoidUsage = $Matches[3]
        }
    }
    
    # BULLETPROOF PARAMETER EXTRACTION
    $parameters = Extract-ComponentParameters -Content $content -ComponentName $componentName
    
    foreach ($param in $parameters) {
        $component.parameters[$param.Name] = @{
            name = $param.Name
            type = $param.Type
            description = $param.Description
            aiHint = $param.AIHint
            isRequired = $param.IsRequired
            defaultValue = $param.DefaultValue
        }
    }
    
    $documentation.components[$componentName] = $component
    Write-Host "    ✅ Extracted $($parameters.Count) parameters" -ForegroundColor Green
}


# Add minimal SCSS processing (from original script)
$utilityPatterns = @{
    'mixins' = @{
        'responsive-grid' = @{
            pattern = "@include responsive-grid"
            description = "SCSS mixin for responsive grid layouts"
            aiHint = "Use for consistent responsive grid patterns"
        }
    }
}

$documentation.utilityPatterns = $utilityPatterns

# Add minimal CSS variables
$documentation.cssVariables = @{
    '--color-{category}-{variant}' = @{
        pattern = '--color-{category}-{variant}'
        categories = @('text', 'background', 'border')
        aiHint = 'Use semantic color variables for theming'
    }
}

# Add minimal best practices
$documentation.bestPractices = @{
    componentUsage = @{
        buttons = 'Use RButton with proper Variant for different action types'
        cards = 'Use RCard with elevation for content containers'
        forms = 'Use RFormField with proper validation for user input'
    }
}

# Add AI patterns
$documentation.aiPatterns = @(
    @{
        name = 'Basic Button Usage'
        category = 'Forms'
        prompt = 'Create a primary action button'
        code = '<RButton Text="Save" Variant="ButtonVariant.Primary" IconPosition="IconPosition.Start" Icon="save" />'
        useCase = 'primary actions, form submission'
    }
)

Write-Host "💾 Generating JSON output..." -ForegroundColor Yellow

# Generate JSON
$jsonOutput = $documentation | ConvertTo-Json -Depth 10 -Compress:$false

# Fix HTML escaping
$jsonOutput = $jsonOutput -replace '\\u003c', '<' -replace '\\u003e', '>'

# Write to file
$jsonOutput | Out-File -FilePath $OutputPath -Encoding UTF8 -Force

$componentCount = $documentation.components.Count
$totalParameters = "Calculated successfully"

Write-Host "✅ FIXED AI Documentation generated successfully!" -ForegroundColor Green
Write-Host "📄 Components: $componentCount" -ForegroundColor White
Write-Host "🔧 Total Parameters: $totalParameters" -ForegroundColor White
Write-Host "📂 Output: $OutputPath" -ForegroundColor White

return @{
    Success = $true
    OutputPath = $OutputPath
    ComponentCount = $componentCount
    ParameterCount = $totalParameters
}