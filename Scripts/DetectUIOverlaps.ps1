#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Comprehensive RR.Blazor Architecture & UI Analysis Tool

.DESCRIPTION
    Analyzes SCSS, CSS, and Razor files to identify:
    - UI overlap patterns and z-index conflicts
    - Undefined mixins/extends usage
    - Duplicated styling blocks across classes
    - Onion architecture violations (heavy styling in classes vs mixins/extends)
    - Code quality and maintainability issues
    
.PARAMETER Path  
    Path to analyze (defaults to RR.Blazor directory)
    
.PARAMETER Detailed
    Show detailed analysis with recommendations
    
.PARAMETER ExportReport
    Export findings to markdown report
    
.EXAMPLE
    ./DetectUIOverlaps.ps1 -Detailed -ExportReport
#>

param(
    [string]$Path = "RR.Blazor",
    [switch]$Detailed,
    [switch]$ExportReport
)

# Color functions for output
function Write-ColorOutput($ForegroundColor, $Message) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Output $Message
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Success($Message) { Write-ColorOutput Green $Message }
function Write-Warning($Message) { Write-ColorOutput Yellow $Message }
function Write-Error($Message) { Write-ColorOutput Red $Message }
function Write-Info($Message) { Write-ColorOutput Cyan $Message }

# Initialize findings
$findings = @{
    ZIndexConflicts = @()
    PositionOverlaps = @()
    FlexboxIssues = @()
    ButtonIconOverlaps = @()
    ModalOverlaps = @()
    TooltipOverlaps = @()
    DropdownOverlaps = @()
    UndefinedMixins = @()
    UndefinedExtends = @()
    DuplicatedBlocks = @()
    OnionViolations = @()
    CriticalIssues = @()
    Warnings = @()
    Recommendations = @()
}

Write-Info "🔍 RR.Blazor Comprehensive Architecture & UI Analysis"
Write-Info "=" * 60

# Get all relevant files
Write-Info "📁 Scanning files in $Path..."
$scssFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.scss" -ErrorAction SilentlyContinue
$cssFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.css" -ErrorAction SilentlyContinue  
$razorFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.razor" -ErrorAction SilentlyContinue

$totalFiles = $scssFiles.Count + $cssFiles.Count + $razorFiles.Count
Write-Info "📊 Found $totalFiles files ($($scssFiles.Count) SCSS, $($cssFiles.Count) CSS, $($razorFiles.Count) Razor)"

# Z-Index Analysis
Write-Info "`n🎯 Analyzing Z-Index Conflicts..."
$zIndexMap = @{}

foreach ($file in $scssFiles + $cssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        # Extract z-index values
        $zIndexMatches = [regex]::Matches($content, 'z-index:\s*([^;]+);', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $zIndexMatches) {
            $zValue = $match.Groups[1].Value.Trim()
            $context = ($content.Substring([Math]::Max(0, $match.Index - 100), [Math]::Min(200, $content.Length - [Math]::Max(0, $match.Index - 100)))).Trim()
            
            if (-not $zIndexMap.ContainsKey($zValue)) {
                $zIndexMap[$zValue] = @()
            }
            
            $zIndexMap[$zValue] += @{
                File = $file.Name
                Context = $context
                Value = $zValue
            }
        }
    } catch {
        Write-Warning "⚠️ Could not process $($file.Name): $_"
    }
}

# Identify z-index conflicts
foreach ($zIndex in $zIndexMap.Keys) {
    $occurrences = $zIndexMap[$zIndex]
    if ($occurrences.Count -gt 1) {
        # Check if it's actually a conflict (same numeric value, different contexts)
        $numericValue = if ($zIndex -match '^\d+$') { [int]$zIndex } else { $null }
        
        if ($numericValue -and $numericValue -gt 0) {
            $contexts = $occurrences | ForEach-Object { $_.Context } | Sort-Object -Unique
            if ($contexts.Count -gt 1) {
                $findings.ZIndexConflicts += @{
                    ZIndex = $zIndex
                    Count = $occurrences.Count
                    Files = ($occurrences | ForEach-Object { $_.File }) -join ", "
                    Severity = if ($numericValue -gt 1000) { "High" } else { "Medium" }
                }
            }
        }
    }
}

# Position Overlap Analysis
Write-Info "📍 Analyzing Position Overlaps..."
$positionPatterns = @(
    @{ Pattern = 'position:\s*absolute.*?top:\s*0.*?left:\s*0'; Description = "Full overlay pattern" }
    @{ Pattern = 'position:\s*fixed.*?top:\s*0.*?left:\s*0'; Description = "Fixed full screen overlay" }
    @{ Pattern = 'position:\s*absolute.*?bottom:\s*0.*?right:\s*0'; Description = "Bottom-right absolute positioning" }
    @{ Pattern = 'transform:\s*translate\(-50%,\s*-50%\).*?position:\s*absolute'; Description = "Centered absolute positioning" }
)

foreach ($file in $scssFiles + $cssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        foreach ($pattern in $positionPatterns) {
            $matches = [regex]::Matches($content, $pattern.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)
            
            foreach ($match in $matches) {
                $findings.PositionOverlaps += @{
                    File = $file.Name
                    Pattern = $pattern.Description
                    Context = $content.Substring([Math]::Max(0, $match.Index - 50), [Math]::Min(150, $content.Length - [Math]::Max(0, $match.Index - 50))).Trim()
                }
            }
        }
    } catch {
        continue
    }
}

# Button + Icon Overlap Analysis (specific to the screenshot issue)
Write-Info "🔘 Analyzing Button Icon Overlaps..."
$buttonIconPatterns = @(
    'display:\s*inline-flex.*?align-items:\s*center.*?::before',
    'material-symbols.*?inline-flex',
    'button.*?\{[^}]*display:\s*inline-flex[^}]*align-items:\s*center[^}]*\}.*?::before',
    '\.icon[^{]*\{[^}]*position:\s*absolute[^}]*\}'
)

foreach ($file in $scssFiles + $cssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        foreach ($pattern in $buttonIconPatterns) {
            $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)
            
            if ($matches.Count -gt 0) {
                $findings.ButtonIconOverlaps += @{
                    File = $file.Name
                    PatternType = "Button Icon Layout"
                    Count = $matches.Count
                    Severity = "Medium"
                }
            }
        }
    } catch {
        continue
    }
}

# Flexbox Gap Issues
Write-Info "📦 Analyzing Flexbox Overlap Issues..."
$flexboxIssues = @(
    @{ Pattern = 'display:\s*flex.*?gap:\s*0'; Description = "Flex with zero gap - potential overlap" }
    @{ Pattern = 'flex-shrink:\s*0.*?flex-grow:\s*1.*?flex-shrink:\s*0'; Description = "Conflicting flex-shrink values" }
    @{ Pattern = 'justify-content:\s*center.*?align-items:\s*center.*?position:\s*absolute'; Description = "Centered flex with absolute positioning" }
)

foreach ($file in $scssFiles + $cssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        foreach ($issue in $flexboxIssues) {
            $matches = [regex]::Matches($content, $issue.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)
            
            foreach ($match in $matches) {
                $findings.FlexboxIssues += @{
                    File = $file.Name
                    Issue = $issue.Description
                    Context = $content.Substring([Math]::Max(0, $match.Index - 30), [Math]::Min(100, $content.Length - [Math]::Max(0, $match.Index - 30))).Trim()
                }
            }
        }
    } catch {
        continue
    }
}

# Undefined Mixins/Extends Analysis
Write-Info "🔧 Analyzing Undefined Mixins and Extends..."
$definedMixins = @()
$definedExtends = @()

# First, collect all defined mixins and extends
foreach ($file in $scssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        # Find mixin definitions
        $mixinDefs = [regex]::Matches($content, '@mixin\s+([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $mixinDefs) {
            $definedMixins += $match.Groups[1].Value
        }
        
        # Find extend definitions (% placeholders)
        $extendDefs = [regex]::Matches($content, '%([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $extendDefs) {
            $definedExtends += $match.Groups[1].Value
        }
    } catch {
        continue
    }
}

$definedMixins = $definedMixins | Sort-Object -Unique
$definedExtends = $definedExtends | Sort-Object -Unique

Write-Info "   Found $($definedMixins.Count) defined mixins and $($definedExtends.Count) defined extends"

# Now check for usage of undefined mixins/extends
foreach ($file in $scssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        # Check @include usage
        $includeUsages = [regex]::Matches($content, '@include\s+([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $includeUsages) {
            $usedMixin = $match.Groups[1].Value
            if ($definedMixins -notcontains $usedMixin -and $usedMixin -notmatch '^(media|supports|keyframes)$') {
                # Check if it has !optional
                $context = $content.Substring([Math]::Max(0, $match.Index), [Math]::Min(100, $content.Length - $match.Index))
                $hasOptional = $context -match '!optional'
                
                $findings.UndefinedMixins += @{
                    File = $file.Name
                    Mixin = $usedMixin
                    HasOptional = $hasOptional
                    Context = $context.Split("`n")[0].Trim()
                    Severity = if ($hasOptional) { "Low" } else { "High" }
                }
            }
        }
        
        # Check @extend usage
        $extendUsages = [regex]::Matches($content, '@extend\s+%([a-zA-Z0-9_-]+)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $extendUsages) {
            $usedExtend = $match.Groups[1].Value
            if ($definedExtends -notcontains $usedExtend) {
                # Check if it has !optional
                $context = $content.Substring([Math]::Max(0, $match.Index), [Math]::Min(100, $content.Length - $match.Index))
                $hasOptional = $context -match '!optional'
                
                $findings.UndefinedExtends += @{
                    File = $file.Name
                    Extend = $usedExtend
                    HasOptional = $hasOptional
                    Context = $context.Split("`n")[0].Trim()
                    Severity = if ($hasOptional) { "Low" } else { "High" }
                }
            }
        }
    } catch {
        continue
    }
}

# Duplicated Styling Blocks Analysis
Write-Info "📋 Analyzing Duplicated Styling Blocks..."
$stylingBlocks = @{}

foreach ($file in $scssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        # Extract CSS property blocks (simplified - looks for common patterns)
        $propertyPatterns = @(
            'display:\s*flex.*?align-items:\s*center.*?justify-content:\s*[^;]+',
            'position:\s*absolute.*?top:\s*[^;]+.*?left:\s*[^;]+',
            'border-radius:\s*[^;]+.*?padding:\s*[^;]+.*?margin:\s*[^;]+',
            'font-size:\s*[^;]+.*?font-weight:\s*[^;]+.*?color:\s*[^;]+',
            'width:\s*100%.*?height:\s*100%',
            'transition:\s*all\s+[^;]+.*?transform:\s*[^;]+',
            'background:\s*[^;]+.*?border:\s*[^;]+.*?box-shadow:\s*[^;]+'
        )
        
        foreach ($pattern in $propertyPatterns) {
            $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)
            
            foreach ($match in $matches) {
                $blockSignature = $match.Value -replace '\s+', ' ' -replace '[^a-zA-Z0-9:\-;() ]', ''
                if ($blockSignature.Length -gt 20) { # Only track substantial blocks
                    if (-not $stylingBlocks.ContainsKey($blockSignature)) {
                        $stylingBlocks[$blockSignature] = @()
                    }
                    
                    $stylingBlocks[$blockSignature] += @{
                        File = $file.Name
                        Block = $match.Value.Substring(0, [Math]::Min(150, $match.Value.Length)).Trim()
                    }
                }
            }
        }
    } catch {
        continue
    }
}

# Find duplicated blocks
foreach ($signature in $stylingBlocks.Keys) {
    $occurrences = $stylingBlocks[$signature]
    if ($occurrences.Count -gt 1) {
        $uniqueFiles = ($occurrences | ForEach-Object { $_.File }) | Sort-Object -Unique
        if ($uniqueFiles.Count -gt 1) {
            $findings.DuplicatedBlocks += @{
                Signature = $signature.Substring(0, [Math]::Min(80, $signature.Length))
                Count = $occurrences.Count
                Files = $uniqueFiles -join ", "
                Severity = if ($occurrences.Count -gt 3) { "High" } else { "Medium" }
                Example = $occurrences[0].Block
            }
        }
    }
}

# Onion Architecture Analysis (always enabled per CLAUDE.md)
Write-Info "🧅 Analyzing Onion Architecture Compliance..."

$heavyStylingPatterns = @(
    @{ Pattern = 'display:\s*[^;]+.*?position:\s*[^;]+.*?z-index:\s*[^;]+.*?transform:\s*[^;]+'; Weight = 4; Description = "Complex layout with positioning and transforms" }
    @{ Pattern = 'background:\s*linear-gradient[^;]+.*?box-shadow:\s*[^;]+.*?border-radius:\s*[^;]+'; Weight = 3; Description = "Visual effects with gradients and shadows" }
    @{ Pattern = '(flex|grid)-[^:]*:\s*[^;]+.*?(flex|grid)-[^:]*:\s*[^;]+.*?(flex|grid)-[^:]*:\s*[^;]+'; Weight = 3; Description = "Complex flexbox/grid layout" }
    @{ Pattern = 'animation:\s*[^;]+.*?transition:\s*[^;]+.*?transform:\s*[^;]+'; Weight = 3; Description = "Animation and transition effects" }
    @{ Pattern = '::before.*?\{[^}]{50,}'; Weight = 2; Description = "Complex pseudo-elements" }
    @{ Pattern = '::after.*?\{[^}]{50,}'; Weight = 2; Description = "Complex pseudo-elements" }
)

foreach ($file in $scssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        # Skip mixin/extend files
        if ($file.Name -match '_(mixins|extends|base)\.scss$' -or $file.FullName -match 'abstracts[\\/]') {
            continue
        }
        
        # Extract class definitions
        $classBlocks = [regex]::Matches($content, '\.([a-zA-Z0-9_-]+)\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        
        foreach ($classMatch in $classBlocks) {
            $className = $classMatch.Groups[1].Value
            $classContent = $classMatch.Groups[2].Value
            $heavyWeight = 0
            $violations = @()
            
            foreach ($pattern in $heavyStylingPatterns) {
                if ($classContent -match $pattern.Pattern) {
                    $heavyWeight += $pattern.Weight
                    $violations += $pattern.Description
                }
            }
            
            # Check if class uses mixins/extends (good)
            $usesMixins = ($classContent -match '@include') -or ($classContent -match '@extend')
            $directProps = ([regex]::Matches($classContent, '[a-z-]+:\s*[^;}]+', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
            
            if ($heavyWeight -gt 2 -and -not $usesMixins) {
                $findings.OnionViolations += @{
                    File = $file.Name
                    Class = $className
                    Weight = $heavyWeight
                    DirectProperties = $directProps
                    Violations = $violations
                    Severity = if ($heavyWeight -gt 4) { "High" } else { "Medium" }
                    Recommendation = "Move heavy styling to mixins/extends"
                }
            }
        }
    } catch {
        continue
    }
}

# Modal Overlap Analysis
Write-Info "🗂️ Analyzing Modal Overlaps..."
foreach ($file in $scssFiles + $cssFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Continue
        if ($null -eq $content) { continue }
        
        # Look for multiple modal-related z-index values
        if ($content -match 'modal|backdrop|overlay' -and $content -match 'z-index') {
            $modalZIndexes = [regex]::Matches($content, '(modal|backdrop|overlay)[^{]*\{[^}]*z-index:\s*([^;}]+)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            
            if ($modalZIndexes.Count -gt 1) {
                $findings.ModalOverlaps += @{
                    File = $file.Name
                    Count = $modalZIndexes.Count
                    ZIndexValues = ($modalZIndexes | ForEach-Object { $_.Groups[2].Value }) -join ", "
                }
            }
        }
    } catch {
        continue
    }
}

# Generate Report
Write-Info "`n📊 COMPREHENSIVE ARCHITECTURE ANALYSIS RESULTS"
Write-Info "=" * 60

# Z-Index Conflicts
if ($findings.ZIndexConflicts.Count -gt 0) {
    Write-Error "❌ Z-INDEX CONFLICTS ($($findings.ZIndexConflicts.Count)):"
    foreach ($conflict in $findings.ZIndexConflicts) {
        Write-Output "   • z-index: $($conflict.ZIndex) used in $($conflict.Count) different contexts"
        Write-Output "     Files: $($conflict.Files)"
        Write-Output "     Severity: $($conflict.Severity)"
        Write-Output ""
        
        if ($conflict.Severity -eq "High") {
            $findings.CriticalIssues += "High z-index conflict: $($conflict.ZIndex)"
        }
    }
} else {
    Write-Success "✅ No z-index conflicts found"
}

# Position Overlaps
if ($findings.PositionOverlaps.Count -gt 0) {
    Write-Warning "⚠️ POSITION OVERLAPS ($($findings.PositionOverlaps.Count)):"
    $findings.PositionOverlaps | Group-Object Pattern | ForEach-Object {
        Write-Output "   • $($_.Name): $($_.Count) occurrences"
        $_.Group | Select-Object -First 3 | ForEach-Object {
            Write-Output "     - $($_.File)"
        }
        Write-Output ""
    }
} else {
    Write-Success "✅ No position overlaps detected"
}

# Button Icon Overlaps
if ($findings.ButtonIconOverlaps.Count -gt 0) {
    Write-Error "🔘 BUTTON ICON OVERLAPS ($($findings.ButtonIconOverlaps.Count)):"
    foreach ($overlap in $findings.ButtonIconOverlaps) {
        Write-Output "   • $($overlap.File): $($overlap.Count) potential icon overlap patterns"
        $findings.CriticalIssues += "Button icon overlap in $($overlap.File)"
    }
    Write-Output ""
} else {
    Write-Success "✅ No button icon overlaps detected"
}

# Flexbox Issues
if ($findings.FlexboxIssues.Count -gt 0) {
    Write-Warning "📦 FLEXBOX ISSUES ($($findings.FlexboxIssues.Count)):"
    $findings.FlexboxIssues | Group-Object Issue | ForEach-Object {
        Write-Output "   • $($_.Name): $($_.Count) occurrences"
        $_.Group | Select-Object -First 2 | ForEach-Object {
            Write-Output "     - $($_.File)"
        }
    }
    Write-Output ""
}

# Modal Overlaps
if ($findings.ModalOverlaps.Count -gt 0) {
    Write-Warning "🗂️ MODAL OVERLAPS ($($findings.ModalOverlaps.Count)):"
    foreach ($overlap in $findings.ModalOverlaps) {
        Write-Output "   • $($overlap.File): $($overlap.Count) modal z-index definitions"
        Write-Output "     Z-indexes: $($overlap.ZIndexValues)"
    }
    Write-Output ""
}

# Undefined Mixins/Extends
if ($findings.UndefinedMixins.Count -gt 0 -or $findings.UndefinedExtends.Count -gt 0) {
    Write-Error "🔧 UNDEFINED MIXINS & EXTENDS:"
    
    if ($findings.UndefinedMixins.Count -gt 0) {
        Write-Output "   📦 Undefined Mixins ($($findings.UndefinedMixins.Count)):"
        $findings.UndefinedMixins | ForEach-Object {
            $icon = if ($_.HasOptional) { "⚠️" } else { "❌" }
            Write-Output "   $icon $($_.File): @include $($_.Mixin) $(if ($_.HasOptional) { "(has !optional)" } else { "(NO !optional)" })"
            if (-not $_.HasOptional) {
                $findings.CriticalIssues += "Undefined mixin: $($_.Mixin) in $($_.File)"
            }
        }
        Write-Output ""
    }
    
    if ($findings.UndefinedExtends.Count -gt 0) {
        Write-Output "   📐 Undefined Extends ($($findings.UndefinedExtends.Count)):"
        $findings.UndefinedExtends | ForEach-Object {
            $icon = if ($_.HasOptional) { "⚠️" } else { "❌" }
            Write-Output "   $icon $($_.File): @extend %$($_.Extend) $(if ($_.HasOptional) { "(has !optional)" } else { "(NO !optional)" })"
            if (-not $_.HasOptional) {
                $findings.CriticalIssues += "Undefined extend: %$($_.Extend) in $($_.File)"
            }
        }
        Write-Output ""
    }
} else {
    Write-Success "✅ All mixins and extends properly defined"
}

# Duplicated Styling Blocks
if ($findings.DuplicatedBlocks.Count -gt 0) {
    Write-Warning "📋 DUPLICATED STYLING BLOCKS ($($findings.DuplicatedBlocks.Count)):"
    $findings.DuplicatedBlocks | Sort-Object Count -Descending | Select-Object -First 10 | ForEach-Object {
        Write-Output "   • $($_.Count) duplicates: $($_.Signature)..."
        Write-Output "     Files: $($_.Files)"
        Write-Output "     Severity: $($_.Severity)"
        if ($_.Severity -eq "High") {
            $findings.CriticalIssues += "High duplication: $($_.Signature) in $($_.Files)"
        }
        Write-Output ""
    }
} else {
    Write-Success "✅ No significant duplicated styling blocks found"
}

# Onion Architecture Violations
if ($findings.OnionViolations.Count -gt 0) {
    Write-Error "🧅 ONION ARCHITECTURE VIOLATIONS ($($findings.OnionViolations.Count)):"
    $findings.OnionViolations | Sort-Object Weight -Descending | ForEach-Object {
        Write-Output "   • .$($_.Class) in $($_.File) (Weight: $($_.Weight), Props: $($_.DirectProperties))"
        Write-Output "     Violations: $($_.Violations -join ', ')"
        Write-Output "     Recommendation: $($_.Recommendation)"
        if ($_.Severity -eq "High") {
            $findings.CriticalIssues += "Heavy styling in class: .$($_.Class) in $($_.File)"
        }
        Write-Output ""
    }
} else {
    Write-Success "✅ Good onion architecture compliance - classes use mixins/extends properly"
}

# Recommendations
Write-Info "`n💡 RECOMMENDATIONS:"
if ($findings.CriticalIssues.Count -gt 0) {
    Write-Output "🔥 CRITICAL FIXES NEEDED:"
    $findings.CriticalIssues | ForEach-Object {
        Write-Output "   • $_"
    }
    Write-Output ""
}

Write-Output "📋 General Recommendations:"
Write-Output "   🎯 UI & Layout:"
Write-Output "      • Use CSS Grid for complex layouts instead of absolute positioning"
Write-Output "      • Implement consistent z-index scale (1-10, 100-110, 1000-1010, etc.)"
Write-Output "      • Use flexbox gap instead of margins for button icon spacing"
Write-Output "      • Consider CSS container queries for responsive overlaps"
Write-Output "      • Implement proper focus management for overlapping elements"
Write-Output ""
Write-Output "   🏗️ Architecture & Code Quality:"
Write-Output "      • Always use !optional flag when including potentially undefined mixins"
Write-Output "      • Extract duplicate styling blocks into reusable mixins or extends"
Write-Output "      • Move heavy styling from classes to mixins/extends (onion architecture)"
Write-Output "      • Use placeholder selectors (%) for extends to avoid CSS bloat"
Write-Output "      • Implement consistent naming conventions for mixins and extends"
Write-Output ""
Write-Output "   🔧 Best Practices:"
Write-Output "      • Group related mixins in dedicated files (_mixins folder)"
Write-Output "      • Use semantic naming for extends (%button-base, %card-elevated)"
Write-Output "      • Document complex mixins with @param annotations"
Write-Output "      • Use CSS logical properties for better RTL support"

# Export Report
if ($ExportReport) {
    $reportPath = "UI_Overlap_Analysis_$(Get-Date -Format 'yyyyMMdd_HHmmss').md"
    
    $reportContent = @"
# UI Overlap Analysis Report
Generated: $(Get-Date)
Path Analyzed: $Path

## Summary
- Total Files Analyzed: $totalFiles
- Z-Index Conflicts: $($findings.ZIndexConflicts.Count)
- Position Overlaps: $($findings.PositionOverlaps.Count)  
- Button Icon Overlaps: $($findings.ButtonIconOverlaps.Count)
- Flexbox Issues: $($findings.FlexboxIssues.Count)
- Modal Overlaps: $($findings.ModalOverlaps.Count)

## Critical Issues
$($findings.CriticalIssues | ForEach-Object { "- $_" } | Out-String)

## Z-Index Conflicts
$($findings.ZIndexConflicts | ForEach-Object { "- z-index $($_.ZIndex): $($_.Count) conflicts in $($_.Files) (Severity: $($_.Severity))" } | Out-String)

## Button Icon Overlaps  
$($findings.ButtonIconOverlaps | ForEach-Object { "- $($_.File): $($_.Count) potential overlaps" } | Out-String)

## Recommendations
- Implement consistent z-index scale
- Use CSS Grid for complex layouts
- Use flexbox gap for button spacing
- Implement proper focus management
- Consider CSS container queries
"@

    $reportContent | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Success "📄 Report exported to: $reportPath"
}

# Summary
Write-Info "`n📈 ANALYSIS SUMMARY:"
$totalIssues = $findings.ZIndexConflicts.Count + $findings.PositionOverlaps.Count + $findings.ButtonIconOverlaps.Count + $findings.FlexboxIssues.Count + $findings.ModalOverlaps.Count

if ($totalIssues -eq 0) {
    Write-Success "🎉 No critical UI overlap issues detected!"
} elseif ($findings.CriticalIssues.Count -eq 0) {
    Write-Warning "⚠️ $totalIssues minor UI overlap issues found - recommend review"
} else {
    Write-Error "❌ $totalIssues UI overlap issues found, including $($findings.CriticalIssues.Count) critical issues"
}

Write-Info "🔍 Analysis complete. Use -Detailed for more information."