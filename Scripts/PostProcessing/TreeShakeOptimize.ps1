<#
.SYNOPSIS
    RR.Blazor CSS Tree-Shaking and Optimization Script
    
.DESCRIPTION
    Analyzes Blazor applications to identify used RR.Blazor components and utilities,
    then generates optimized CSS bundles with unused styles removed.
    Based on LiftKit tree-shaking principles for maximum performance.

.PARAMETER ProjectPath
    Path to the Blazor project to analyze (default: current directory)
    
.PARAMETER OutputPath  
    Path where optimized CSS files will be generated
    
    
.PARAMETER ComponentRegistry
    Path to component registry JSON file
    
.PARAMETER VerboseLogging
    Enable detailed logging of the optimization process

.EXAMPLE
    .\TreeShakeOptimize.ps1 -ProjectPath "C:\MyBlazorApp" -OutputPath "./wwwroot/css"
    
#>

param(
    [Parameter(Position = 0)]
    [string]$ProjectPath = ".",
    
    [Parameter(Position = 1)]
    [string]$OutputPath = "./wwwroot/css/optimized",
    
    
    [string]$ComponentRegistry = "./wwwroot/rr-ai-components.json",
    
    [switch]$VerboseLogging = $false
)

# PowerShell version check and auto-install
function Ensure-PowerShell7 {
    $currentVersion = $PSVersionTable.PSVersion
    if ($currentVersion.Major -lt 7) {
        Write-Host "PowerShell 7+ required. Current version: $currentVersion" -ForegroundColor Yellow
        
        if ($IsWindows -or $env:OS -eq "Windows_NT") {
            Write-Host "Installing PowerShell 7 via winget..." -ForegroundColor Cyan
            try {
                winget install Microsoft.PowerShell --silent --accept-package-agreements --accept-source-agreements
                Write-Host "PowerShell 7 installed. Please restart your terminal and run the script again." -ForegroundColor Green
                exit 0
            }
            catch {
                Write-Host "Failed to install via winget. Please install PowerShell 7 manually: https://aka.ms/powershell-release" -ForegroundColor Red
                exit 1
            }
        }
        else {
            Write-Host "Please install PowerShell 7+: https://aka.ms/powershell-release" -ForegroundColor Red
            exit 1
        }
    }
}

Ensure-PowerShell7

# Set error handling
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Initialize logging
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    if ($VerboseLogging -or $Level -eq "ERROR") {
        Write-Host $logMessage -ForegroundColor $(
            switch ($Level) {
                "ERROR" { "Red" }
                "WARN" { "Yellow" }
                "SUCCESS" { "Green" }
                default { "Cyan" }
            }
        )
    }
}

Write-Log "🚀 Starting RR.Blazor Tree-Shaking Optimization" "SUCCESS"

# Resolve paths
$ResolvedProjectPath = Resolve-Path $ProjectPath -ErrorAction SilentlyContinue
if (-not $ResolvedProjectPath) {
    Write-Log "Project path not found: $ProjectPath" "ERROR"
    exit 1
}
$ProjectPath = $ResolvedProjectPath.Path

$OutputPath = Join-Path $ProjectPath $OutputPath
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Log "Created output directory: $OutputPath"
}

# Component usage tracking
$ComponentUsage = @{}
$UtilityUsage = @{}

Write-Log "📁 Scanning project files for RR.Blazor usage..."

# Scan Blazor files (.razor, .cs, .html)
$BlazorFiles = Get-ChildItem -Path $ProjectPath -Recurse -Include "*.razor", "*.cs", "*.html" -ErrorAction SilentlyContinue
$totalFiles = $BlazorFiles.Count
$currentFile = 0

foreach ($file in $BlazorFiles) {
    $currentFile++
    $progress = [math]::Round(($currentFile / $totalFiles) * 100, 1)
    
    if ($VerboseLogging) {
        Write-Progress -Activity "Analyzing Files" -Status "Processing: $($file.Name)" -PercentComplete $progress
    }
    
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }
        
        # Track RR.Blazor component usage (R-prefixed components)
        $componentMatches = [regex]::Matches($content, '<R[A-Z]\w+|@*R[A-Z]\w+', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $componentMatches) {
            $componentName = $match.Value -replace '^[@<]*', '' -replace '\s.*$', ''
            $ComponentUsage[$componentName] = ($ComponentUsage[$componentName] ?? 0) + 1
        }
        
        # Track utility class usage
        $utilityMatches = [regex]::Matches($content, 'class="([^"]*)"')
        foreach ($match in $utilityMatches) {
            $classes = $match.Groups[1].Value -split '\s+' | Where-Object { $_ -match '^(btn|card|flex|grid|text|bg|border|shadow|space|gap|p-|m-|mt-|mb-|ml-|mr-|pt-|pb-|pl-|pr-|px-|py-|mx-|my-)' }
            foreach ($class in $classes) {
                $UtilityUsage[$class] = ($UtilityUsage[$class] ?? 0) + 1
            }
        }
        
    }
    catch {
        Write-Log "Error processing file $($file.FullName): $($_.Exception.Message)" "WARN"
    }
}

Write-Progress -Activity "Analyzing Files" -Completed

Write-Log "📊 Analysis Results:"
Write-Log "   • Components found: $($ComponentUsage.Keys.Count)"
Write-Log "   • Utilities found: $($UtilityUsage.Keys.Count)"

# Load component registry for dependency tracking
$registry = @{}
$registryPath = Join-Path $ProjectPath $ComponentRegistry
if (Test-Path $registryPath) {
    try {
        $registryContent = Get-Content $registryPath -Raw | ConvertFrom-Json
        $registry = $registryContent
        $componentCount = if ($registry -and $registry.PSObject.Properties.Name -contains 'components' -and $registry.components) { 
            ($registry.components.PSObject.Properties | Measure-Object).Count 
        } else { 0 }
        Write-Log "📋 Loaded component registry with $componentCount components"
    }
    catch {
        Write-Log "Warning: Could not load component registry: $($_.Exception.Message)" "WARN"
    }
}

# Generate safelist for critical CSS classes
$safelist = @(
    # Core RR.Blazor classes that should always be preserved
    'rr-*', 'blazor-*', 'r-*',
    # Interactive states
    'hover\\:', 'focus\\:', 'active\\:', 'disabled\\:',
    # Responsive breakpoints  
    'sm\\:', 'md\\:', 'lg\\:', 'xl\\:',
    # Animation classes
    'animate-*', 'transition-*', 'duration-*', 'ease-*',
    # Critical layout utilities
    'd-flex', 'd-grid', 'd-block', 'd-none', 'container'
)

# Add component-specific safelists from registry
if ($registry -and $registry.PSObject.Properties.Name -contains 'components' -and $registry.components) {
    foreach ($componentName in $ComponentUsage.Keys) {
        if ($registry.components.PSObject.Properties.Name -contains $componentName) {
            $component = $registry.components.$componentName
            if ($component -and $component.PSObject.Properties.Name -contains 'treeShaking' -and $component.treeShaking -and $component.treeShaking.PSObject.Properties.Name -contains 'safelist') {
                $safelist += $component.treeShaking.safelist
            }
        }
    }
}


Write-Log "🔧 Generating optimized CSS bundle..."

# Read source CSS files
$sourceCSS = ""
$cssFiles = @(
    (Join-Path $ProjectPath "wwwroot/css/main.css"),
    (Join-Path $ProjectPath "Styles/dist/rr-blazor.css"),
    (Join-Path $ProjectPath "wwwroot/css/rr-blazor.css")
)

foreach ($cssFile in $cssFiles) {
    if (Test-Path $cssFile) {
        $cssContent = Get-Content $cssFile -Raw
        $sourceCSS += "`n/* From: $cssFile */`n" + $cssContent
        Write-Log "   • Loaded: $($cssFile | Split-Path -Leaf) ($([math]::Round($cssContent.Length / 1KB, 1))KB)"
        break  # Use first available CSS file
    }
}

if (-not $sourceCSS) {
    Write-Log "No source CSS files found. Please build the project first." "ERROR"
    exit 1
}

$originalSize = $sourceCSS.Length

# CSS optimization: Remove unused selectors based on component and utility usage
$optimizedCSS = ""
$usedSelectors = @()

# Include CSS for used components
foreach ($componentName in $ComponentUsage.Keys) {
    # Add component-specific CSS patterns
    $componentPatterns = @(
        "\.r-$($componentName.ToLower())",
        "\.R$componentName",
        "\.$($componentName.ToLower())"
    )
    
    foreach ($pattern in $componentPatterns) {
        $matches = [regex]::Matches($sourceCSS, "$pattern[^{]*\{[^}]*\}", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $matches) {
            $usedSelectors += $match.Value
        }
    }
}

# Include CSS for used utilities
foreach ($utilityClass in $UtilityUsage.Keys) {
    $escapedClass = [regex]::Escape($utilityClass)
    $pattern = "\.$escapedClass(?:[^a-zA-Z0-9_-]|$)[^{]*\{[^}]*\}"
    $matches = [regex]::Matches($sourceCSS, $pattern)
    foreach ($match in $matches) {
        $usedSelectors += $match.Value
    }
}


# Always include CSS custom properties and base styles
$basePatterns = @(
    ':root\s*\{[^}]*\}',
    '^\s*\/\*[^*]*\*\/',  # Comments
    '@import[^;]*;',       # Imports
    '@charset[^;]*;',      # Charset
    'html\s*\{[^}]*\}',    # HTML base styles
    'body\s*\{[^}]*\}',    # Body base styles
    '\*[^{]*\{[^}]*\}'     # Universal selectors
)

foreach ($pattern in $basePatterns) {
    $matches = [regex]::Matches($sourceCSS, $pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $matches) {
        $usedSelectors += $match.Value
    }
}

# Remove duplicates and sort
$usedSelectors = $usedSelectors | Sort-Object -Unique

# Build optimized CSS
$optimizedCSS = @"
/* 
 * RR.Blazor Optimized CSS Bundle
 * Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
 * Components: $($ComponentUsage.Keys.Count)
 * Utilities: $($UtilityUsage.Keys.Count)
 */

"@ + ($usedSelectors -join "`n`n")

# Calculate savings
$optimizedSize = $optimizedCSS.Length
$savings = [math]::Round((($originalSize - $optimizedSize) / $originalSize) * 100, 1)
$originalKB = [math]::Round($originalSize / 1KB, 1)
$optimizedKB = [math]::Round($optimizedSize / 1KB, 1)

# Write optimized files
$optimizedPath = Join-Path $OutputPath "rr-blazor.optimized.css"
$minifiedPath = Join-Path $OutputPath "rr-blazor.min.css"

Set-Content -Path $optimizedPath -Value $optimizedCSS -Encoding UTF8
Write-Log "   • Generated: rr-blazor.optimized.css ($optimizedKB KB)"

# Create minified version (basic minification)
$minifiedCSS = $optimizedCSS -replace '/\*[^*]*\*/', '' -replace '\s+', ' ' -replace ';\s*}', '}' -replace '{\s*', '{'
Set-Content -Path $minifiedPath -Value $minifiedCSS -Encoding UTF8
$minifiedKB = [math]::Round($minifiedCSS.Length / 1KB, 1)
Write-Log "   • Generated: rr-blazor.min.css ($minifiedKB KB)"

# Generate usage report
$reportPath = Join-Path $OutputPath "optimization-report.json"
$report = @{
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    projectPath = $ProjectPath
    originalSize = @{
        bytes = $originalSize
        kb = $originalKB
    }
    optimizedSize = @{
        bytes = $optimizedSize
        kb = $optimizedKB
    }
    minifiedSize = @{
        bytes = $minifiedCSS.Length
        kb = $minifiedKB
    }
    savings = @{
        percentage = $savings
        bytes = $originalSize - $optimizedSize
        kb = $originalKB - $optimizedKB
    }
    components = $ComponentUsage
    utilities = $UtilityUsage
    filesScanned = $totalFiles
    selectorsPreserved = $usedSelectors.Count
}

$report | ConvertTo-Json -Depth 10 | Set-Content -Path $reportPath -Encoding UTF8

Write-Log "📊 Optimization Complete!" "SUCCESS"
Write-Log "   • Original size: $originalKB KB"
Write-Log "   • Optimized size: $optimizedKB KB"
Write-Log "   • Minified size: $minifiedKB KB"
Write-Log "   • Space saved: $savings% ($($originalKB - $optimizedKB) KB)"
Write-Log "   • Files scanned: $totalFiles"
Write-Log "   • Selectors preserved: $($usedSelectors.Count)"
Write-Log "   • Report: optimization-report.json"

# Return success code
exit 0