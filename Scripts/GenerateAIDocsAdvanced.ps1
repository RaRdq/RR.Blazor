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

# Initialize documentation structure
$documentation = @{
    '$schema' = 'https://rr-blazor.dev/schema/ai-docs.json'
    version = '1.0.0'
    generated = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    info = @{
        name = 'RR.Blazor'
        description = 'Enterprise Blazor component library - AI-optimized documentation'
        author = 'RaRdq'
        license = 'MIT'
        componentCount = 0
        utilityPatternCount = 0
    }
    components = @{}
    utilityPatterns = @{}
    cssVariables = @{}
    bestPractices = @{}
    aiPatterns = @()
}

Write-Host "📂 Scanning components..." -ForegroundColor Yellow

# Extract Components
$componentFiles = Get-ChildItem -Path "$ProjectPath/Components" -Filter "*.razor" -Recurse | Where-Object { $_.Name.StartsWith('R') }

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
    if ($content -match '@\*\*\s*(.*?)\*\*@' -match '(?s)') {
        $aiBlock = $Matches[1]
        
        # Extract tags
        if ($aiBlock -match '<summary>(.*?)</summary>') { $component.description = $Matches[1].Trim() }
        if ($aiBlock -match '<category>(.*?)</category>') { $component.category = $Matches[1].Trim() }
        if ($aiBlock -match '<complexity>(.*?)</complexity>') { $component.complexity = $Matches[1].Trim() }
        if ($aiBlock -match '<ai-prompt>(.*?)</ai-prompt>') { $component.aiPrompt = $Matches[1].Trim() }
        if ($aiBlock -match '<ai-common-use>(.*?)</ai-common-use>') { $component.commonUse = $Matches[1].Trim() }
        if ($aiBlock -match '<ai-avoid>(.*?)</ai-avoid>') { $component.avoidUsage = $Matches[1].Trim() }
        
        # Extract AI patterns
        $patternMatches = [regex]::Matches($aiBlock, '<ai-pattern name="([^"]+)">(.*?)</ai-pattern>')
        foreach ($match in $patternMatches) {
            $component.patterns[$match.Groups[1].Value] = $match.Groups[2].Value.Trim()
        }
    }
    
    # Extract parameters from @code blocks
    $codeBlockPattern = '@code\s*\{(.*?)\}'
    if ($content -match $codeBlockPattern) {
        $codeBlock = $Matches[1]
        
        # Find Parameter attributes
        $parameterPattern = '\[Parameter\](?:\s*\[AIParameter\([^\]]*\)\])?\s*public\s+(\w+(?:\?)?)\s+(\w+)\s*\{[^}]*\}'
        $paramMatches = [regex]::Matches($codeBlock, $parameterPattern)
        
        foreach ($paramMatch in $paramMatches) {
            $paramType = $paramMatch.Groups[1].Value
            $paramName = $paramMatch.Groups[2].Value
            
            $parameter = @{
                name = $paramName
                type = $paramType
                description = ''
                aiHint = ''
                isRequired = $false
            }
            
            # Extract AIParameter hint if present
            if ($paramMatch.Value -match '\[AIParameter\("([^"]*)"') {
                $parameter.aiHint = $Matches[1]
            }
            
            $component.parameters[$paramName] = $parameter
        }
    }
    
    $documentation.components[$componentName] = $component
}

Write-Host "🎨 Extracting SCSS patterns..." -ForegroundColor Yellow

# Extract Utility Patterns from SCSS
$scssFiles = Get-ChildItem -Path "$ProjectPath/Styles" -Filter "*.scss" -Recurse

$utilityPatterns = @{
    spacing = @{
        padding = @{
            pattern = 'pa-{0-24}, px-{0-24}, py-{0-24}, pt-{0-24}, pr-{0-24}, pb-{0-24}, pl-{0-24}'
            description = 'Padding utilities following design system scale'
            aiHint = 'Use pa-6 for standard card padding, px-4 py-2 for buttons'
            scale = @(0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24)
        }
        margin = @{
            pattern = 'ma-{0-24}, mx-{0-24}, my-{0-24}, mt-{0-24}, mr-{0-24}, mb-{0-24}, ml-{0-24}, mx-auto'
            description = 'Margin utilities including auto centering'
            aiHint = 'Use mx-auto for centering, mb-4 for standard spacing'
            scale = @(0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24)
        }
        gap = @{
            pattern = 'gap-{0-24}, gap-x-{0-24}, gap-y-{0-24}'
            description = 'Grid and flexbox gap utilities'
            aiHint = 'Use gap-4 for standard spacing in flex/grid layouts'
        }
    }
    layout = @{
        flexbox = @{
            pattern = 'd-flex, flex-{direction}, justify-{content}, align-{items}, flex-{wrap}'
            values = @{
                direction = @('row', 'column', 'row-reverse', 'column-reverse')
                content = @('start', 'end', 'center', 'between', 'around', 'evenly')
                items = @('start', 'end', 'center', 'baseline', 'stretch')
                wrap = @('wrap', 'nowrap', 'wrap-reverse')
            }
            aiHint = 'Use d-flex justify-between align-center for header layouts'
        }
        grid = @{
            pattern = 'd-grid, grid-cols-{1-12}, grid-rows-{1-6}, col-span-{1-12}'
            description = 'CSS Grid utilities for complex layouts'
            aiHint = 'Use d-grid grid-cols-2 gap-4 for two-column layouts'
        }
    }
    effects = @{
        elevation = @{
            pattern = 'elevation-{0-24}, elevation-lift, hover:elevation-{0-24}'
            description = 'Material Design elevation system with interactive states'
            aiHint = 'Use elevation-4 for cards, elevation-8 for modals, elevation-lift for hover'
            scale = @(0, 1, 2, 4, 6, 8, 12, 16, 20, 24)
        }
        glassmorphism = @{
            pattern = 'glass, glass-{variant}, backdrop-blur-{size}'
            variants = @('light', 'medium', 'heavy', 'frost', 'crystal', 'interactive')
            description = 'Modern glassmorphism effects with backdrop filters'
            aiHint = 'Use glass-light for subtle effects, glass-medium for prominence'
        }
    }
    typography = @{
        textSize = @{
            pattern = 'text-{size}'
            values = @('xs', 'sm', 'base', 'lg', 'xl', '2xl', '3xl', '4xl', '5xl', '6xl')
            semantic = @('text-h1', 'text-h2', 'text-h3', 'text-h4', 'text-h5', 'text-h6', 'text-body-1', 'text-body-2', 'text-caption')
            aiHint = 'Use text-h4 for section headers, text-body-1 for content'
        }
        textWeight = @{
            pattern = 'font-{weight}'
            values = @('thin', 'light', 'normal', 'medium', 'semibold', 'bold', 'extrabold', 'black')
            aiHint = 'Use font-semibold for emphasis, font-medium for buttons'
        }
    }
    business = @{
        formGrids = @{
            pattern = 'form-grid, form-grid--{columns}'
            values = @('1', '2', '3', '4', 'auto')
            description = 'Professional form layout grids'
            aiHint = 'Use form-grid--2 for dual-column forms'
        }
        statsGrids = @{
            pattern = 'stats-grid, action-grid'
            description = 'Dashboard and analytics layout patterns'
            aiHint = 'Use stats-grid for metric cards layout'
        }
    }
}

# Extract CSS Variables as concise patterns (AI-optimized)
$cssVariablePatterns = @{}

# Define semantic patterns that AI can understand and extrapolate
$cssVariablePatterns = @{
    '--color-[category]-[variant]' = @{
        pattern = '--color-[category]-[variant]'
        category = 'color'
        explanation = 'Semantic color system'
        categories = @('interactive', 'text', 'background', 'status', 'surface', 'border', 'glass', 'gradient')
        variants = @('primary', 'secondary', 'light', 'dark', 'hover', 'focus', 'disabled')
        aiHint = 'Use --color-text-primary for main text, --color-background-elevated for cards'
    }
    '--space-{0-24}' = @{
        pattern = '--space-{0-24}'
        category = 'spacing'
        explanation = 'Design system spacing scale'
        scale = @(0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24)
        aiHint = 'Standard spacing scale: 0=0px, 1=0.25rem, 4=1rem, 6=1.5rem, 24=6rem'
    }
    '--shadow-{variant}' = @{
        pattern = '--shadow-{variant}'
        category = 'effect'
        explanation = 'Elevation shadow system'
        variants = @('none', 'sm', 'md', 'lg', 'xl', '2xl', 'inset', 'lift', 'focus')
        aiHint = 'Use --shadow-md for cards, --shadow-lg for modals, --shadow-lift for hover'
    }
    '--gradient-{variant}' = @{
        pattern = '--gradient-{variant}'
        category = 'effect'
        explanation = 'Professional gradient backgrounds'
        variants = @('subtle', 'executive', 'primary', 'success', 'warning', 'danger', 'dark')
        aiHint = 'Use --gradient-subtle for backgrounds, --gradient-executive for premium feel'
    }
    '--font-{type}-{variant}' = @{
        pattern = '--font-{type}-{variant}'
        category = 'typography'
        explanation = 'Typography system variables'
        types = @('size', 'weight', 'family', 'height')
        variants = @('xs', 'sm', 'md', 'lg', 'xl', 'thin', 'normal', 'bold', 'mono', 'sans')
        aiHint = 'Use --font-size-lg for headers, --font-weight-semibold for emphasis'
    }
    '--radius-{size}' = @{
        pattern = '--radius-{size}'
        category = 'shape'
        explanation = 'Border radius scale'
        sizes = @('none', 'sm', 'md', 'lg', 'xl', '2xl', 'full', 'pill')
        aiHint = 'Use --radius-md for cards, --radius-full for circular elements'
    }
    '--transition-{speed}' = @{
        pattern = '--transition-{speed}'
        category = 'animation'
        explanation = 'Animation timing system'
        speeds = @('fast', 'normal', 'slow', 'none')
        aiHint = 'Use --transition-normal for most interactions, --transition-fast for hover'
    }
    '--z-{layer}' = @{
        pattern = '--z-{layer}'
        category = 'layout'
        explanation = 'Z-index layering system'
        layers = @('base', 'elevated', 'sticky', 'fixed', 'modal', 'popover', 'tooltip')
        aiHint = 'Use --z-modal for overlays, --z-tooltip for highest priority'
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

# Update counts
$documentation.info.componentCount = $documentation.components.Count
$documentation.info.utilityPatternCount = $utilityPatterns.Keys.Count

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
Write-Host "📄 Components: $($documentation.info.componentCount)" -ForegroundColor White
Write-Host "🎨 Utility patterns: $($documentation.info.utilityPatternCount)" -ForegroundColor White
Write-Host "📊 CSS variables: $($documentation.cssVariables.Count)" -ForegroundColor White
Write-Host "💡 AI patterns: $($documentation.aiPatterns.Count)" -ForegroundColor White
Write-Host "📂 Output: $OutputPath" -ForegroundColor White

return @{
    Success = $true
    OutputPath = $OutputPath
    ComponentCount = $documentation.info.componentCount
    UtilityCount = $documentation.info.utilityPatternCount
}