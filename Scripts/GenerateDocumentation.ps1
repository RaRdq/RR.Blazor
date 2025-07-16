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

# Function to extract CSS classes and variables from compiled CSS
function Extract-CssClasses {
    param([string]$CssFilePath, [string]$ScssDirectory)
    
    $classes = @{}
    $variables = @{}
    
    # First extract variables from SCSS files (CSS doesn't contain variable definitions)
    $scssFiles = Get-ChildItem -Path $ScssDirectory -Filter "*.scss" -Recurse
    Write-Host "  Extracting CSS variables from $($scssFiles.Count) SCSS files" -ForegroundColor DarkGray
    
    foreach ($file in $scssFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Extract CSS variables (--var-name) - comprehensive patterns
        # Pattern 1: Variable definitions (--var-name: value;)
        $varMatches1 = [regex]::Matches($content, '--([a-zA-Z0-9_-]+)\s*:', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $varMatches1) {
            $varName = $match.Groups[1].Value
            if (-not $variables.ContainsKey($varName)) {
                $variables[$varName] = @{
                    "file" = $file.Name
                    "category" = "css-variable-definition"
                }
            }
        }
        
        # Pattern 2: Variable usage (var(--var-name))
        $varMatches2 = [regex]::Matches($content, 'var\(\s*--([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $varMatches2) {
            $varName = $match.Groups[1].Value
            if (-not $variables.ContainsKey($varName)) {
                $variables[$varName] = @{
                    "file" = $file.Name
                    "category" = "css-variable-usage"
                }
            }
        }
        
        # Pattern 3: SCSS variable aliases ($var: var(--var-name))
        $varMatches3 = [regex]::Matches($content, '\$[a-zA-Z0-9_-]+\s*:\s*var\(\s*--([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $varMatches3) {
            $varName = $match.Groups[1].Value
            if (-not $variables.ContainsKey($varName)) {
                $variables[$varName] = @{
                    "file" = $file.Name
                    "category" = "css-variable-alias"
                }
            }
        }
    }
    
    # Use compiled CSS as the definitive source - it has all classes properly generated
    if ((Test-Path $CssFilePath) -and (Get-Item $CssFilePath).Length -gt 1000) {
        Write-Host "  Extracting all classes from compiled CSS: $CssFilePath" -ForegroundColor DarkGray
        $cssContent = Get-Content $CssFilePath -Raw -Encoding UTF8
        
        # Check if CSS is valid (not just error messages) - look for CSS compilation errors, not CSS variables
        if ($cssContent -notmatch "^/\* Error:" -and $cssContent -match "\.[a-zA-Z]") {
            # Extract all CSS class selectors (.class-name, .class-name:hover, etc.)
            $classMatches = [regex]::Matches($cssContent, '\.([a-zA-Z][a-zA-Z0-9_-]*(?:\\:[a-zA-Z0-9_-]+)*)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
            foreach ($match in $classMatches) {
                $fullClassName = $match.Groups[1].Value
                # Clean pseudo-classes and escape sequences for base class name
                $baseClassName = $fullClassName -replace '\\:', ':' -replace ':.*$', ''
                
                if (-not $classes.ContainsKey($baseClassName)) {
                    $classes[$baseClassName] = @{
                        "file" = "compiled-css"
                        "category" = "css-generated"
                    }
                }
            }
            Write-Host "  Extracted $($classes.Count) classes from compiled CSS" -ForegroundColor DarkGray
        } else {
            throw "CSS compilation failed. Build errors must be fixed first. Run 'dotnet build' to see errors."
        }
    } else {
        throw "CSS file not found or too small at $CssFilePath. Run 'dotnet build' first."
    }
    
    return @{
        "classes" = $classes
        "variables" = $variables
    }
}

# SCSS-only extraction - purely based on SCSS source of truth
function Extract-ScssUtilityClasses {
    param([array]$ScssFiles)
    
    $classes = @{}
    
    # Extract classes directly from SCSS files (source of truth)
    foreach ($file in $ScssFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Extract explicit class definitions (.class-name)
        $classMatches = [regex]::Matches($content, '\.([a-zA-Z][a-zA-Z0-9_-]*)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        foreach ($match in $classMatches) {
            $className = $match.Groups[1].Value
            # Skip SCSS interpolation patterns and selectors with pseudo-classes in source
            if ($className -notmatch '#\{' -and $className -notmatch ':' -and -not $classes.ContainsKey($className)) {
                $classes[$className] = @{
                    "file" = $file.Name
                    "category" = "explicit-scss"
                }
            }
        }
        
        # Extract BEM notation classes (.parent { &-child { ... } })
        $classes = Extract-BemClasses -Content $content -File $file -Classes $classes
        
        # Extract @each loop generated classes
        $classes = Extract-AtEachClasses -Content $content -File $file -Classes $classes
        
        # Extract @for loop generated classes
        $classes = Extract-ForLoopClasses -Content $content -File $file -Classes $classes
    }
    
    Write-Host "  Extracted $($classes.Count) classes directly from SCSS source files" -ForegroundColor DarkGray
    return $classes
}

# Extract BEM notation classes from SCSS
function Extract-BemClasses {
    param([string]$Content, [System.IO.FileInfo]$File, [hashtable]$Classes)
    
    # Find BEM parent classes with nested &- children
    # Pattern: .parent { ... &-child { ... } ... }
    $bemMatches = [regex]::Matches($Content, '\.([a-zA-Z][a-zA-Z0-9_-]*)\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    foreach ($match in $bemMatches) {
        $parentClass = $match.Groups[1].Value
        $parentContent = $match.Groups[2].Value
        
        # Find all &-child patterns within this parent
        $childMatches = [regex]::Matches($parentContent, '&-([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        
        foreach ($childMatch in $childMatches) {
            $childName = $childMatch.Groups[1].Value
            $fullClassName = "$parentClass-$childName"
            
            if (-not $Classes.ContainsKey($fullClassName)) {
                $Classes[$fullClassName] = @{
                    "file" = $File.Name
                    "category" = "bem-scss"
                }
            }
        }
        
        # Also add the parent class if not already present
        if (-not $Classes.ContainsKey($parentClass)) {
            $Classes[$parentClass] = @{
                "file" = $File.Name
                "category" = "bem-parent-scss"
            }
        }
    }
    
    return $Classes
}

# Extract classes generated by @for loops in SCSS
function Extract-ForLoopClasses {
    param([string]$Content, [System.IO.FileInfo]$File, [hashtable]$Classes)
    
    # Pattern: @for $i from 1 through 12 { .col-#{$i} { ... } }
    $forMatches = [regex]::Matches($Content, '@for\s+\$(\w+)\s+from\s+(\d+)\s+through\s+(\d+)\s*\{[^}]*\.([a-zA-Z0-9_-]*)-#\{\$\1\}', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    
    foreach ($match in $forMatches) {
        $variable = $match.Groups[1].Value
        $start = [int]$match.Groups[2].Value
        $end = [int]$match.Groups[3].Value
        $prefix = $match.Groups[4].Value
        
        for ($i = $start; $i -le $end; $i++) {
            $generatedClass = "$prefix-$i"
            
            if (-not $Classes.ContainsKey($generatedClass)) {
                $Classes[$generatedClass] = @{
                    "file" = $File.Name
                    "category" = "generated-for-loop"
                }
            }
        }
    }
    
    return $Classes
}

# Extract classes generated by @each loops in SCSS
function Extract-AtEachClasses {
    param([string]$Content, [System.IO.FileInfo]$File, [hashtable]$Classes)
    
    # Pattern 1: @each $name, $value in $map { .prefix-#{$name} }
    $eachMatches1 = [regex]::Matches($Content, '@each\s+\$([^,\s]+)(?:,\s*\$[^{]+)?\s+in\s+\$([^{]+)\s*\{[^}]*\.([a-zA-Z0-9_-]*)-#\{\$\1\}', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $eachMatches1) {
        $variableName = $match.Groups[1].Value.Trim()
        $mapName = $match.Groups[2].Value.Trim()
        $classPrefix = $match.Groups[3].Value.Trim()
        
        # Find the map definition in the same file or imported files
        $mapPattern = "\`$${mapName}\s*:\s*\([^)]+\)"
        $mapMatch = [regex]::Match($Content, $mapPattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
        
        if ($mapMatch.Success) {
            $mapContent = $mapMatch.Value
            # Extract map keys 
            $keyMatches = [regex]::Matches($mapContent, '([''"])([\w-]+)\1\s*:')
            foreach ($keyMatch in $keyMatches) {
                $key = $keyMatch.Groups[2].Value
                $generatedClass = if ($classPrefix) { "$classPrefix-$key" } else { $key }
                
                if (-not $Classes.ContainsKey($generatedClass)) {
                    $Classes[$generatedClass] = @{
                        "file" = $File.Name
                        "category" = "generated-at-each"
                    }
                }
            }
        }
    }
    
    # Pattern 2: @each $item in (list) { .#{$item} }
    $eachMatches2 = [regex]::Matches($Content, '@each\s+\$([^{]+)\s+in\s+\(([^)]+)\)\s*\{[^}]*\.#\{\$\1\}', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $eachMatches2) {
        $variableName = $match.Groups[1].Value.Trim()
        $listContent = $match.Groups[2].Value.Trim()
        
        # Extract list items
        $items = $listContent -split ',' | ForEach-Object { $_.Trim().Trim('"').Trim("'") }
        foreach ($item in $items) {
            if ($item -and $item -ne '') {
                $generatedClass = $item
                
                if (-not $Classes.ContainsKey($generatedClass)) {
                    $Classes[$generatedClass] = @{
                        "file" = $File.Name
                        "category" = "generated-at-each-list"
                    }
                }
            }
        }
    }
    
    # Pattern 3: @each $name, $ratio in $aspect-ratios { .aspect-#{$name} }
    $eachMatches3 = [regex]::Matches($Content, '@each\s+\$([^,\s]+),\s*\$[^{]+\s+in\s+\$([^{]+)\s*\{[^}]*\.([a-zA-Z0-9_-]*)-#\{\$\1\}', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $eachMatches3) {
        $variableName = $match.Groups[1].Value.Trim()
        $mapName = $match.Groups[2].Value.Trim()
        $classPrefix = $match.Groups[3].Value.Trim()
        
        # Find the map definition in the same file
        $mapPattern = "\`$${mapName}\s*:\s*\("
        $mapStart = $Content.IndexOf("`$${mapName}:")
        if ($mapStart -gt -1) {
            # Extract map content manually to handle nested parentheses
            $parenCount = 0
            $mapContent = ""
            $inMap = $false
            
            for ($i = $mapStart; $i -lt $Content.Length; $i++) {
                $char = $Content[$i]
                if ($char -eq '(' -and -not $inMap) {
                    $inMap = $true
                    $parenCount = 1
                    $mapContent += $char
                } elseif ($inMap) {
                    $mapContent += $char
                    if ($char -eq '(') { $parenCount++ }
                    elseif ($char -eq ')') { 
                        $parenCount--
                        if ($parenCount -eq 0) { break }
                    }
                }
            }
            
            # Extract map keys
            if ($mapContent) {
                $keyMatches = [regex]::Matches($mapContent, '([a-zA-Z0-9_-]+)\s*:')
                foreach ($keyMatch in $keyMatches) {
                    $key = $keyMatch.Groups[1].Value
                    $generatedClass = "$classPrefix-$key"
                    
                    if (-not $Classes.ContainsKey($generatedClass)) {
                        $Classes[$generatedClass] = @{
                            "file" = $File.Name
                            "category" = "generated-at-each-map"
                        }
                    }
                }
            }
        }
    }
    
    return $Classes
}

# Extract actual classes and variables from compiled CSS and SCSS files
$stylesPath = Join-Path $ProjectPath "Styles"
$cssPath = Join-Path $ProjectPath "wwwroot\css\main.css"

if (Test-Path $stylesPath) {
    $extractedStyles = Extract-CssClasses -CssFilePath $cssPath -ScssDirectory $stylesPath
    Write-Host "  Extracted $($extractedStyles.classes.Count) classes and $($extractedStyles.variables.Count) variables" -ForegroundColor DarkGray
} else {
    Write-Host "  Warning: Styles directory not found at $stylesPath" -ForegroundColor Yellow
    $extractedStyles = @{ "classes" = @{}; "variables" = @{} }
}

# Function to generate bracket notation patterns from actual classes
function Generate-BracketPatterns {
    param([hashtable]$Classes)
    
    $patterns = @{}
    $standaloneClasses = @{}
    
    foreach ($className in $Classes.Keys) {
        $category = "other"
        $isStandalone = $false
        
        # Handle standalone classes (no prefix-suffix pattern) - detect from actual CSS patterns
        if ($className -notmatch '^[a-z]+-[a-z0-9-]+$' -and $className -match '^[a-z][a-z-]*[a-z]?$') {
            # Categorize standalone classes by common CSS properties they likely represent
            if ($className -match '^(flex|grid|block|inline|hidden|visible|overflow)') {
                $category = "layout"
                $isStandalone = $true
            }
            elseif ($className -match '^(bold|italic|underline|uppercase|lowercase|capitalize)$') {
                $category = "typography"
                $isStandalone = $true
            }
            elseif ($className -match '^(pointer|grab|wait|help|crosshair)$') {
                $category = "interactive"
                $isStandalone = $true
            }
        }
        # Categorize and extract patterns with prefix-suffix structure
        elseif ($className -match '^(p|m|pa|ma|pt|pb|pl|pr|px|py|mt|mb|ml|mr|mx|my|gap)-(.+)$') { 
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
        
        # Collect standalone classes by category
        if ($isStandalone) {
            if (-not $standaloneClasses.ContainsKey($category)) { $standaloneClasses[$category] = @() }
            $standaloneClasses[$category] += $className
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
    
    # Add standalone classes in bracket notation format
    foreach ($category in $standaloneClasses.Keys) {
        if (-not $bracketPatterns.ContainsKey($category)) { $bracketPatterns[$category] = @() }
        $standaloneValues = $standaloneClasses[$category] | Sort-Object -Unique
        # Add standalone classes as a special bracket notation
        $standaloneBracket = "[" + ($standaloneValues -join ', ') + "]"
        $bracketPatterns[$category] = @($standaloneBracket) + $bracketPatterns[$category]
    }
    
    return $bracketPatterns
}

# Generate bracket notation patterns
$bracketPatterns = Generate-BracketPatterns -Classes $extractedStyles.classes

# Group CSS variables by category (only actual --variables)
$categorizedVars = @{}
foreach ($varName in $extractedStyles.variables.Keys) {
    $category = "other"
    
    # Categorize actual CSS variables by their semantic meaning
    if ($varName -match '^(space|spacing)-') { $category = "spacing" }
    elseif ($varName -match '^color-') { $category = "colors" }
    elseif ($varName -match '^(font|text|leading|tracking)-') { $category = "typography" }
    elseif ($varName -match '^(shadow|elevation)-') { $category = "elevation" }
    elseif ($varName -match '^(radius|border-radius)-') { $category = "borders" }
    elseif ($varName -match '^(blur|glass|backdrop)-') { $category = "effects" }
    elseif ($varName -match '^(duration|ease|transition)-') { $category = "animations" }
    elseif ($varName -match '^(icon|size)-') { $category = "sizing" }
    
    if (-not $categorizedVars.ContainsKey($category)) {
        $categorizedVars[$category] = @()
    }
    $categorizedVars[$category] += "--$varName"  # Add -- prefix for proper CSS variable format
}

$stylesDoc = [ordered]@{
    "_ai_instructions" = @{
        "CRITICAL" = "You must use bracket notation for extrapolation. You are required to understand patterns like justify-[start, center, end, between] means justify-start, justify-center, justify-end, justify-between"
        "NAVIGATION" = @{
            "ai_instructions" = "Lines 1-15: You must read these AI instructions and bracket notation rules first"
            "utility_patterns" = "Lines 16-80: You must use these utility classes in bracket notation for extrapolation"
            "css_variables" = "Lines 81+: You must use these CSS custom properties (--variables) for component styling"
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
        "CSS_VARIABLES_VS_UTILITIES" = @{
            "UTILITY_CLASSES" = "Use utility classes (pa-4, text-primary) for standard HTML elements and components"
            "CSS_VARIABLES" = "Use CSS variables (var(--color-primary)) for custom component styling or dynamic values"
            "EXAMPLE" = "Use 'bg-primary' for standard styling, use 'background: var(--color-primary)' for dynamic theming"
        }
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
                      $param.Name -in @('Text', 'Icon', 'Variant', 'Size', 'OnClick', 'Disabled', 'Loading', 'Value', 'Label', 'Title', 'Content', 'Items', 'ChildContent', 'HeaderContent', 'MediaContent', 'FooterContent')
        
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
                      $param.Name -in @('Text', 'Icon', 'Variant', 'Size', 'OnClick', 'Disabled', 'Loading', 'Value', 'Label', 'Title', 'Content', 'Items', 'ChildContent', 'HeaderContent', 'MediaContent', 'FooterContent')
        
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