#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates comprehensive AI-optimized documentation for RR.Blazor components
    
.DESCRIPTION
    Extracts components, utility patterns, CSS variables, and best practices
    from RR.Blazor project for AI consumption. Creates lightweight but complete
    documentation that AI agents can use to quickly understand the system.
    
.PARAMETER ProjectPath
    Path to the RR.Blazor project directory
    
.PARAMETER OutputPath
    Output path for the generated JSON documentation
    
.PARAMETER IncludeExamples
    Include usage examples in the output (default: false for AI optimization)
    
.EXAMPLE
    .\GenerateAIDocsAdvanced.ps1 -ProjectPath "C:\Projects\PayrollAI\RR.Blazor" -OutputPath ".\rr-ai-docs.json"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "wwwroot/rr-ai-docs.json",
    
    [Parameter(Mandatory = $false)]
    [bool]$IncludeExamples = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Generating AI-First Documentation for RR.Blazor..." -ForegroundColor Cyan

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

# Initialize documentation structure (AI-optimized, no waste)
$documentation = @{
    components = @{}
    utilityPatterns = @{}
    cssVariables = @{}
    aiPatterns = @()
}

Write-Host "📂 Scanning components..." -ForegroundColor Yellow

# Extract ALL Components (not just R* prefixed)
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
    
    # Extract AI metadata from @** blocks (multiline with dotall)
    if ($content -match '(?s)@\*\*\s*(.*?)\*\*@') {
        $aiBlock = $Matches[1]
        
        # Extract tags (multiline support)
        if ($aiBlock -match '(?s)<summary>(.*?)</summary>') { $component.description = $Matches[1].Trim() -replace '\s+', ' ' }
        if ($aiBlock -match '(?s)<category>(.*?)</category>') { $component.category = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<complexity>(.*?)</complexity>') { $component.complexity = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<ai-prompt>(.*?)</ai-prompt>') { $component.aiPrompt = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<ai-common-use>(.*?)</ai-common-use>') { $component.commonUse = $Matches[1].Trim() }
        if ($aiBlock -match '(?s)<ai-avoid>(.*?)</ai-avoid>') { $component.avoidUsage = $Matches[1].Trim() }
        
        # Extract AI patterns (multiline support)
        $patternMatches = [regex]::Matches($aiBlock, '(?s)<ai-pattern name="([^"]+)">(.*?)</ai-pattern>')
        foreach ($match in $patternMatches) {
            $component.patterns[$match.Groups[1].Value] = $match.Groups[2].Value.Trim()
        }
    }
    
    # Extract C# attributes for additional metadata
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
    
    # Extract parameters from entire component content (not just @code blocks)
    # Updated pattern to match parameters anywhere in the file
    $parameterPattern = '(?s)(?:\/\/\/\s*<summary>(.*?)<\/summary>\s*)?\[Parameter\](?:[^\[]*\[AIParameter\([^\]]*\))?\s*public\s+(\w+(?:\?)?)\s+(\w+)\s*\{[^}]*\}'
    $paramMatches = [regex]::Matches($content, $parameterPattern)
    
    foreach ($paramMatch in $paramMatches) {
        $paramDescription = if ($paramMatch.Groups[1].Success) { $paramMatch.Groups[1].Value.Trim() } else { '' }
        $paramType = $paramMatch.Groups[2].Value
        $paramName = $paramMatch.Groups[3].Value
        
        $parameter = @{
            name = $paramName
            type = $paramType
            description = $paramDescription
            aiHint = ''
            isRequired = $false
        }
        
        # Extract AIParameter hint if present (improved pattern)
        if ($paramMatch.Value -match '\[AIParameter\(\s*"([^"]*)"') {
            $parameter.aiHint = $Matches[1]
        }
        elseif ($paramMatch.Value -match '\[AIParameter\(\s*@"([^"]*)"') {
            $parameter.aiHint = $Matches[1]
        }
        
        # Check if parameter is marked as required
        if ($paramMatch.Value -match 'Required\s*=\s*true' -or $paramType -notmatch '\?$') {
            $parameter.isRequired = $true
        }
        
        $component.parameters[$paramName] = $parameter
    }
    
    $documentation.components[$componentName] = $component
}

Write-Host "🎨 Extracting SCSS patterns..." -ForegroundColor Yellow

# Extract Utility Patterns from SCSS - DYNAMIC SCANNING
$scssFiles = Get-ChildItem -Path "$ProjectPath/Styles" -Filter "*.scss" -Recurse
Write-Host "  Found $($scssFiles.Count) SCSS files" -ForegroundColor DarkGray

$utilityPatterns = @{}
$extractedCssVariables = @{}
$extractedUtilityClasses = @()

foreach ($scssFile in $scssFiles) {
    $scssContent = Get-Content $scssFile.FullName -Raw -Encoding UTF8
    
    # Extract CSS variables (--variable-name)
    $cssVariableMatches = [regex]::Matches($scssContent, '--([a-zA-Z0-9-]+):\s*([^;]+);')
    foreach ($match in $cssVariableMatches) {
        $varName = $match.Groups[1].Value
        $varValue = $match.Groups[2].Value.Trim()
        $extractedCssVariables["--$varName"] = $varValue
    }
    
    # Extract ALL class selectors from SCSS - comprehensive patterns
    # Start with a simpler, more inclusive pattern
    $allClassMatches = [regex]::Matches($scssContent, '\.([a-zA-Z0-9_\\-]+(?:[\\\\]?[:@\\-][a-zA-Z0-9_\\-]+)*)')
    foreach ($match in $allClassMatches) {
        $className = $match.Groups[1].Value
        # Clean up escaped characters
        $className = $className -replace '\\:', ':'
        $className = $className -replace '\\\\', ''
        
        # Filter out component names and BEM elements
        if ($className -notmatch '^[A-Z]' -and $className -notmatch '__' -and $className.Length -gt 1) {
            $extractedUtilityClasses += $className
        }
    }
    
    # Extract mixins (@mixin name)
    $mixinMatches = [regex]::Matches($scssContent, '@mixin\s+([a-zA-Z0-9-]+)')
    foreach ($match in $mixinMatches) {
        $mixinName = $match.Groups[1].Value
        if (-not $utilityPatterns.ContainsKey('mixins')) {
            $utilityPatterns['mixins'] = @{}
        }
        $utilityPatterns['mixins'][$mixinName] = @{
            pattern = "@include $mixinName"
            description = "SCSS mixin for reusable styles"
            aiHint = "Use @include $mixinName for consistent styling"
        }
    }
}

# If compiled CSS exists, extract utilities from there too for completeness
$compiledCssPath = Join-Path $ProjectPath "wwwroot/css/main.css"
if (Test-Path $compiledCssPath) {
    Write-Host "  📄 Analyzing compiled CSS for additional utilities..." -ForegroundColor DarkGray
    $compiledCss = Get-Content $compiledCssPath -Raw -Encoding UTF8
    
    # Extract all class selectors from compiled CSS
    $compiledClassMatches = [regex]::Matches($compiledCss, '^\s*\.([a-zA-Z0-9_\\-]+(?:[:@\\-][a-zA-Z0-9_\\-]+)*)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $compiledClassMatches) {
        $className = $match.Groups[1].Value
        # Clean up and filter
        if ($className -notmatch '^[A-Z]' -and $className -notmatch '__' -and $className.Length -gt 1) {
            $extractedUtilityClasses += $className
        }
    }
}

# Remove duplicates first
$extractedUtilityClasses = $extractedUtilityClasses | Sort-Object -Unique

Write-Host "  📊 Total unique utility classes found: $($extractedUtilityClasses.Count)" -ForegroundColor DarkGray

# Group utility classes by patterns - comprehensive categories based on actual usage
$spacingClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(p|pt|pr|pb|pl|px|py|m|mt|mr|mb|ml|mx|my|gap|gap-x|gap-y|space-x|space-y)(-n)?-' -or
    $_ -match '^(pa|ma)(-n)?-' -or
    $_ -match '^(section-spacing|card-spacing|content-spacing)'
}
$layoutClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(d-|flex|justify|align|items|grid|container|columns|aspect|place|inset|static|fixed|absolute|relative|sticky)' -or
    $_ -match '^(center|max-w|max-h|min-w|min-h|w-|h-|size-|object-|overflow|resize)'
}
$effectClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(elevation|glass|backdrop|shadow|blur|brightness|contrast|grayscale|hue-rotate|invert|saturate|sepia|mix-blend)' -or
    $_ -match '^(drop-shadow|scale|rotate|translate|transform|transition|animate|spin|shimmer|pulse)'
}
$typographyClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(text|font|leading|tracking|whitespace|break|truncate|line-clamp|uppercase|lowercase|capitalize|normal-case)'
}
$borderClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(border|rounded|ring|outline|divide)'
}
$backgroundClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(bg-|background)'
}
$stateClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(hover:|focus:|active:|disabled:|is-|sr-only|not-sr-only|invisible|visible|hidden|cursor|select|list)'
}
$colorClasses = $extractedUtilityClasses | Where-Object { 
    $_ -match '^(text-|bg-|border-|ring-|from-|via-|to-)' -and $_ -match '(primary|secondary|success|warning|error|info|light|dark|transparent|current)'
}

# Create dynamic utility patterns based on discovered classes
if ($spacingClasses.Count -gt 0) {
    # Group spacing utilities by type for better organization
    $paddingUtils = $spacingClasses | Where-Object { $_ -match '^p[tlrbxy]?-' }
    $marginUtils = $spacingClasses | Where-Object { $_ -match '^m[tlrbxy]?(-n)?-' }
    $gapUtils = $spacingClasses | Where-Object { $_ -match '^(gap|space)-' }
    
    $utilityPatterns['spacing'] = @{
        total = $spacingClasses.Count
        padding = @{
            count = $paddingUtils.Count
            examples = ($paddingUtils | Select-Object -First 5) -join ', '
            pattern = 'p{direction}-{size} where direction: t|r|b|l|x|y or empty'
        }
        margin = @{
            count = $marginUtils.Count
            examples = ($marginUtils | Select-Object -First 5) -join ', '
            pattern = 'm{direction}(-n)?-{size} where n = negative'
        }
        gap = @{
            count = $gapUtils.Count
            examples = ($gapUtils | Select-Object -First 5) -join ', '
            pattern = 'gap(-{axis})?-{size} or space-{axis}-{size}'
        }
        aiHint = "Use padding (p-*), margin (m-*), gap-* for consistent spacing"
    }
}

if ($layoutClasses.Count -gt 0) {
    $utilityPatterns['layout'] = @{
        discovered = $layoutClasses | Sort-Object -Unique
        pattern = ($layoutClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "Layout utilities discovered from SCSS files"
        aiHint = "Use discovered layout classes for responsive designs"
    }
}

if ($effectClasses.Count -gt 0) {
    $utilityPatterns['effects'] = @{
        discovered = $effectClasses | Sort-Object -Unique
        pattern = ($effectClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "Visual effect utilities discovered from SCSS files"
        aiHint = "Use discovered effect classes for modern UI"
    }
}

if ($typographyClasses.Count -gt 0) {
    $utilityPatterns['typography'] = @{
        discovered = $typographyClasses | Sort-Object -Unique
        pattern = ($typographyClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "Typography utilities discovered from SCSS files"
        aiHint = "Use discovered typography classes for consistent text styling"
    }
}

if ($borderClasses.Count -gt 0) {
    $utilityPatterns['borders'] = @{
        discovered = $borderClasses | Sort-Object -Unique
        pattern = ($borderClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "Border utilities discovered from SCSS files"
        aiHint = "Use discovered border classes for consistent styling"
    }
}

if ($backgroundClasses.Count -gt 0) {
    $utilityPatterns['backgrounds'] = @{
        discovered = $backgroundClasses | Sort-Object -Unique
        pattern = ($backgroundClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "Background utilities discovered from SCSS files"
        aiHint = "Use discovered background classes for surfaces and containers"
    }
}

if ($stateClasses.Count -gt 0) {
    $utilityPatterns['states'] = @{
        discovered = $stateClasses | Sort-Object -Unique
        pattern = ($stateClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "State and interaction utilities discovered from SCSS files"
        aiHint = "Use discovered state classes for interactive elements"
    }
}

if ($colorClasses.Count -gt 0) {
    $utilityPatterns['colors'] = @{
        discovered = $colorClasses | Sort-Object -Unique
        pattern = ($colorClasses | Sort-Object -Unique | Select-Object -First 10) -join ', '
        description = "Semantic color utilities discovered from SCSS files"
        aiHint = "Use discovered color classes for consistent theming"
    }
}

Write-Host "  📊 CSS variables found: $($extractedCssVariables.Count)" -ForegroundColor DarkGray

# Extract CSS Variables as concise patterns (AI-optimized from discovered variables)
$cssVariablePatterns = @{}

# Group discovered CSS variables by patterns
$colorVars = @($extractedCssVariables.Keys | Where-Object { $_ -match '--color-' })
$spacingVars = @($extractedCssVariables.Keys | Where-Object { $_ -match '--(space|gap|margin|padding)-' })
$fontVars = @($extractedCssVariables.Keys | Where-Object { $_ -match '--font-' })
$shadowVars = @($extractedCssVariables.Keys | Where-Object { $_ -match '--shadow-' })
$radiusVars = @($extractedCssVariables.Keys | Where-Object { $_ -match '--radius-' })

# Create dynamic CSS variable patterns (token-optimized)
if ($colorVars.Count -gt 0) {
    # Extract categories and variants from color variables
    $colorCategories = @{}
    $colorVariants = @{}
    
    foreach ($colorVar in $colorVars) {
        $parts = ($colorVar -replace '^--color-', '') -split '-'
        if ($parts.Count -ge 2) {
            # For compound names like "background-elevated", "text-primary"
            $category = $parts[0]
            $variant = $parts[1..($parts.Count-1)] -join '-'
            
            if (-not $colorCategories.ContainsKey($category)) {
                $colorCategories[$category] = @()
            }
            if ($variant -and -not $colorCategories[$category].Contains($variant)) {
                $colorCategories[$category] += $variant
            }
        } else {
            # For single names like "error", "success"
            if (-not $colorVariants.ContainsKey('status')) {
                $colorVariants['status'] = @()
            }
            if (-not $colorVariants['status'].Contains($parts[0])) {
                $colorVariants['status'] += $parts[0]
            }
        }
    }
    
    $cssVariablePatterns['--color-{category}-{variant}'] = @{
        pattern = '--color-{category}-{variant}'
        categories = ($colorCategories.Keys | Sort-Object)
        variants = @{}
        aiHint = 'Use --color-text-primary for main text, --color-background-elevated for cards'
    }
    
    # Add variants for each category
    foreach ($category in $colorCategories.Keys) {
        $cssVariablePatterns['--color-{category}-{variant}'].variants[$category] = $colorCategories[$category] | Sort-Object
    }
    
    # Add single color variables if any
    if ($colorVariants.Count -gt 0) {
        $cssVariablePatterns['--color-{status}'] = @{
            pattern = '--color-{status}'
            values = ($colorVariants['status'] | Sort-Object)
            aiHint = 'Use for status colors like --color-error, --color-success'
        }
    }
}

if ($spacingVars.Count -gt 0) {
    # Extract only the suffix parts after --space-
    $spacingSuffixes = $spacingVars | ForEach-Object { $_ -replace '^--space-', '' } | Sort-Object -Unique
    $cssVariablePatterns['--space-{sizes}'] = @{
        pattern = '--space-{size}'
        sizes = $spacingSuffixes
        aiHint = 'Standard spacing scale for consistent layouts'
    }
}

if ($fontVars.Count -gt 0) {
    # Group font variables by type
    $fontWeights = @()
    $fontFamilies = @()
    $fontSizes = @()
    
    foreach ($fontVar in $fontVars) {
        $suffix = $fontVar -replace '^--font-', ''
        if ($suffix -match '^(thin|extralight|light|normal|medium|semibold|bold|extrabold|black)$') {
            $fontWeights += $suffix
        } elseif ($suffix -match '^family-') {
            $fontFamilies += $suffix -replace '^family-', ''
        } else {
            $fontSizes += $suffix
        }
    }
    
    if ($fontWeights.Count -gt 0) {
        $cssVariablePatterns['--font-{weight}'] = @{
            pattern = '--font-{weight}'
            weights = ($fontWeights | Sort-Object -Unique)
            aiHint = 'Font weight scale from thin to black'
        }
    }
    
    if ($fontFamilies.Count -gt 0) {
        $cssVariablePatterns['--font-family-{type}'] = @{
            pattern = '--font-family-{type}'
            types = ($fontFamilies | Sort-Object -Unique)
            aiHint = 'Font family types (primary, secondary, mono)'
        }
    }
    
    if ($fontSizes.Count -gt 0) {
        $cssVariablePatterns['--font-{size}'] = @{
            pattern = '--font-{size}'
            sizes = ($fontSizes | Sort-Object -Unique)
            aiHint = 'Font size scale if defined'
        }
    }
}

if ($shadowVars.Count -gt 0) {
    # Extract only the suffix parts after --shadow-
    $shadowSuffixes = $shadowVars | ForEach-Object { $_ -replace '^--shadow-', '' } | Sort-Object -Unique
    $cssVariablePatterns['--shadow-{variants}'] = @{
        pattern = '--shadow-{level}'
        variants = $shadowSuffixes
        aiHint = 'Elevation shadow system'
    }
}

if ($radiusVars.Count -gt 0) {
    # Extract only the suffix parts after --radius-
    $radiusSuffixes = $radiusVars | ForEach-Object { $_ -replace '^--radius-', '' } | Sort-Object -Unique
    $cssVariablePatterns['--radius-{variants}'] = @{
        pattern = '--radius-{size}'
        variants = $radiusSuffixes
        aiHint = 'Border radius scale'
    }
}

# Add fallback patterns for common cases where no variables are discovered
if ($cssVariablePatterns.Count -eq 0) {
    $cssVariablePatterns['--discovered-variables'] = @{
        pattern = 'No CSS variables automatically discovered'
        category = 'fallback'
        explanation = 'CSS variables would be extracted from SCSS files when available'
        aiHint = 'Use standard CSS variables for theming and design tokens'
    }
}

$documentation.cssVariables = $cssVariablePatterns

$documentation.utilityPatterns = $utilityPatterns

Write-Host "🎯 Compiling best practices..." -ForegroundColor Yellow

# Best Practices
$documentation.bestPractices = @{
    componentUsage = @{
        cards = 'Use RCard for content containers, combine with elevation-4 and glass-light for professional appearance'
        buttons = 'Use ButtonVariant.Primary for main actions, Secondary for supporting actions, Danger for destructive actions'
        forms = 'Use form-grid--2 for dual-column layouts, Required="true" for mandatory fields'
        spacing = 'Use pa-6 for card content, gap-4 for flex layouts, mb-4 for standard element separation'
    }
    layoutPatterns = @{
        professionalCard = 'elevation-4 glass-light pa-6 rounded-lg'
        headerLayout = 'd-flex justify-between align-center py-3 px-4'
        formSection = 'bg-elevated pa-4 rounded-md border border-light'
        statsGrid = 'stats-grid gap-6 mb-8'
    }
    accessibility = @{
        colors = 'All color combinations meet WCAG AA contrast requirements'
        focus = 'Focus rings automatically applied to interactive elements'
        touchTargets = 'Minimum 44px touch targets for mobile'
    }
}

# AI-First Patterns (common scenarios for AI to understand)
$documentation.aiPatterns = @(
    @{
        name = 'Executive Dashboard Widget'
        category = 'Business'
        prompt = 'Create a professional metrics card for executive dashboard'
        code = '<RCard Title="Revenue" Elevation="4" class="glass-light"><div class="pa-6"><div class="d-flex justify-between align-center"><span class="text-2xl font-bold">$42,580</span><RBadge Text="+12%" Variant="Success" /></div></div></RCard>'
        useCase = 'dashboards, analytics, metrics display'
    },
    @{
        name = 'Form Section Layout'
        category = 'Forms'
        prompt = 'Create organized form section with validation'
        code = '<RForm TModel="AccountSetupModel" ValidationMode="Hybrid"><FormFields><RFormField FieldType="Email" Label="Email" Size="Large" Variant="FloatingLabel" StartIcon="email" /></FormFields></RForm>'
        useCase = 'forms, data entry, user management'
    },
    @{
        name = 'Data Management Interface'
        category = 'Data'
        prompt = 'Create table with search and actions'
        code = '<div class="d-flex justify-between mb-4"><RFormField Type="Search" class="flex-grow-1" /><RButton Text="Add" Variant="Primary" /></div><RDataTable Items="@data" class="elevation-2" />'
        useCase = 'CRUD operations, data tables, management interfaces'
    },
    @{
        name = 'Professional Card with Glass Effect'
        category = 'Layout'
        prompt = 'Create elevated card with glassmorphism for account setup'
        code = '<RCard Elevation="8" Class="glass-frost backdrop-blur-xl"><div class="pa-2"><h5 class="text-h6 font-semibold mb-0">Title</h5><p class="text-caption text--secondary">Description</p></div></RCard>'
        useCase = 'account setup, login forms, professional cards'
    },
    @{
        name = 'Form Field with Icons'
        category = 'Forms'
        prompt = 'Create password field with visibility toggle'
        code = '<RFormField Label="Password" FieldType="@(showPassword ? FieldType.Text : FieldType.Password)" StartIcon="lock" EndIcon="@(showPassword ? \"visibility_off\" : \"visibility\")" OnEndIconClick="ToggleVisibility" Size="Medium" Required />'
        useCase = 'password fields, secure inputs, icon interactions'
    },
    @{
        name = 'Section Divider with Icon'
        category = 'Layout'
        prompt = 'Create section divider for form organization'
        code = '<RSectionDivider Title="Security Setup" Icon="security" Variant="Primary" Size="Compact" ShowLine="true" />'
        useCase = 'form sections, content organization, visual hierarchy'
    },
    @{
        name = 'Responsive Grid Layout'
        category = 'Layout'
        prompt = 'Create responsive two-column form grid'
        code = '<div class="d-grid gap-1 grid-cols-1 grid-cols-md-2"><RFormField Label="First Name" StartIcon="person" /><RFormField Label="Last Name" StartIcon="person" /></div>'
        useCase = 'responsive forms, multi-column layouts, mobile-first design'
    },
    @{
        name = 'Loading Button State'
        category = 'Interaction'
        prompt = 'Create button with loading state'
        code = '<RButton Text="@(isProcessing ? \"Processing...\" : \"Submit\")" Icon="@(isProcessing ? \"progress_activity\" : \"check\")" Variant="Primary" Elevation="4" Loading="@isProcessing" Disabled="@isProcessing" Class="w-full" />'
        useCase = 'form submission, async operations, loading states'
    }
)

# Counts calculated dynamically for output display only
$componentCount = $documentation.components.Count
$utilityPatternCount = $utilityPatterns.Keys.Count

Write-Host "💾 Generating JSON output..." -ForegroundColor Yellow

# Generate JSON with proper escaping
$jsonSettings = @{
    Depth = 10
    Compress = $false
}

$jsonOutput = $documentation | ConvertTo-Json @jsonSettings

# Fix HTML escaping issues
$jsonOutput = $jsonOutput -replace '\\u003c', '<' -replace '\\u003e', '>'

# Write to file
$jsonOutput | Out-File -FilePath $OutputPath -Encoding UTF8 -Force

Write-Host "✅ AI Documentation generated successfully!" -ForegroundColor Green
Write-Host "📄 Components: $componentCount" -ForegroundColor White
Write-Host "🎨 Utility patterns: $utilityPatternCount" -ForegroundColor White
Write-Host "📊 CSS variables: $($documentation.cssVariables.Count)" -ForegroundColor White
Write-Host "💡 AI patterns: $($documentation.aiPatterns.Count)" -ForegroundColor White
Write-Host "📂 Output: $OutputPath" -ForegroundColor White

return @{
    Success = $true
    OutputPath = $OutputPath
    ComponentCount = $componentCount
    UtilityCount = $utilityPatternCount
}