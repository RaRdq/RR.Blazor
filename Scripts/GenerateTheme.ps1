[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$ThemeName,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".",
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludeAllVariables,
    
    [Parameter(Mandatory=$false)]
    [string]$BaseTheme = "default"
)

$ErrorActionPreference = "Stop"

function Get-ThemeTemplate {
    param([string]$Name, [bool]$FullTemplate)
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $template = @"
// Theme: $Name
// Generated: $timestamp
// Based on: RR.Blazor Theme System

@use '~RR.Blazor/Styles/abstracts' as *;

:root[data-theme="$($Name.ToLower())"] {
  // Core Colors
  --theme-primary: #4687f1;
  --theme-primary-hover: #2d67e6;
  --theme-primary-active: #1e4dcf;
  --theme-success: #10b981;
  --theme-warning: #f59e0b;
  --theme-error: #ef4444;
  --theme-info: #06b6d4;
  
  // Surface Colors
  --theme-canvas: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  --theme-surface: #fafbfc;
  --theme-surface-elevated: #f4f5f7;
  
  // Text Colors
  --theme-text: #0f172a;
  --theme-text-muted: #475569;
  --theme-text-subtle: #94a3b8;
  
  // Border
  --theme-border: #e2e8f0;
  
  // Shadows (5 elevation levels)
  --theme-shadow-sm: 0 2px 4px -1px rgb(0 0 0 / 0.06), 0 1px 2px -1px rgb(0 0 0 / 0.04);
  --theme-shadow-md: 0 8px 25px -5px rgb(0 0 0 / 0.1), 0 4px 6px -2px rgb(0 0 0 / 0.05);
  --theme-shadow-lg: 0 20px 40px -10px rgb(0 0 0 / 0.15), 0 8px 16px -4px rgb(0 0 0 / 0.1);
  --theme-shadow-xl: 0 32px 64px -12px rgb(0 0 0 / 0.25), 0 12px 24px -6px rgb(0 0 0 / 0.15);
  --theme-shadow-2xl: 0 48px 96px -16px rgb(0 0 0 / 0.35), 0 20px 40px -8px rgb(0 0 0 / 0.2);
  
  // Gradients
  --theme-success-gradient: linear-gradient(135deg, #10b981 0%, #059669 100%);
  --theme-warning-gradient: linear-gradient(135deg, rgb(235, 167, 64) 0%, #ce881f 100%);
  --theme-error-gradient: linear-gradient(135deg, #f13f3f 0%, #d41e1e 100%);
  --theme-info-gradient: linear-gradient(135deg, #2db6ce 0%, #1089a8 100%);
  
  // Background Gradients
  --theme-bg-gradient-primary: linear-gradient(135deg, #dbeafe, #bfdbfe, #93c5fd);
  --theme-bg-gradient-success: linear-gradient(135deg, #d1fae5, #a7f3d0, #6ee7b7);
  --theme-bg-gradient-warning: linear-gradient(135deg, #fef3c7, #fde68a, #fcd34d);
  --theme-bg-gradient-error: linear-gradient(135deg, #fee2e2, #fecaca, #fca5a5);
  --theme-bg-gradient-info: linear-gradient(135deg, #e0f2fe, #bae6fd, #7dd3fc);
"@

    if ($FullTemplate) {
        $template += @"
  
  // Border Radius Tokens
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-2xl: 24px;
  --radius-full: 9999px;
  
  // Border Width Tokens
  --border-1: 1px;
  --border-2: 2px;
  --border-3: 3px;
  
  // Spacing (Golden Ratio)
  --space-0: 0;
  --space-0-5: 0.125rem;
  --space-1: 0.25rem;
  --space-1-5: 0.375rem;
  --space-2: 0.5rem;
  --space-2-5: 0.625rem;
  --space-3: 0.75rem;
  --space-3-5: 0.875rem;
  --space-4: 1rem;
  --space-5: 1.25rem;
  --space-6: 1.5rem;
  --space-7: 1.75rem;
  --space-8: 2rem;
  --space-9: 2.25rem;
  --space-10: 2.5rem;
  --space-11: 2.75rem;
  --space-12: 3rem;
  --space-14: 3.5rem;
  --space-16: 4rem;
  --space-20: 5rem;
  --space-24: 6rem;
  --space-32: 8rem;
  --space-40: 10rem;
  --space-48: 12rem;
  --space-64: 16rem;
  --space-80: 20rem;
  --space-px: 1px;
  
  // Typography Scale
  --text-2xs: 0.625rem;
  --text-xs: 0.75rem;
  --text-sm: 0.875rem;
  --text-base: 1rem;
  --text-lg: 1.125rem;
  --text-xl: 1.25rem;
  --text-2xl: 1.5rem;
  --text-3xl: 1.875rem;
  --text-4xl: 2.25rem;
  --text-5xl: 3rem;
  --text-6xl: 3.75rem;
  --text-7xl: 4.5rem;
  --text-8xl: 6rem;
  
  // Animation Durations
  --duration-ultra-fast: 50ms;
  --duration-fast: 150ms;
  --duration-normal: 300ms;
  --duration-slow: 500ms;
  --duration-very-slow: 1000ms;
  
  // Glass Morphism
  --glass-blur-sm: 8px;
  --glass-blur-md: 12px;
  --glass-blur-lg: 16px;
  --glass-blur-xl: 24px;
  --glass-brightness: 1.05;
  --glass-saturate: 1.2;
  
  // Opacity Scale
  --opacity-0: 0;
  --opacity-5: 0.05;
  --opacity-10: 0.1;
  --opacity-20: 0.2;
  --opacity-25: 0.25;
  --opacity-30: 0.3;
  --opacity-40: 0.4;
  --opacity-50: 0.5;
  --opacity-60: 0.6;
  --opacity-70: 0.7;
  --opacity-75: 0.75;
  --opacity-80: 0.8;
  --opacity-90: 0.9;
  --opacity-95: 0.95;
  --opacity-100: 1;
"@
    }

    $template += @"

}
"@

    return $template
}

function Test-ThemeNameSecurity {
    param([string]$Name)
    
    # Validate theme name for security
    if ([string]::IsNullOrWhiteSpace($Name)) {
        throw "Theme name cannot be empty or whitespace"
    }
    
    # Check for path traversal attempts
    if ($Name -match '\.\.' -or $Name -match '[/\\]' -or $Name -match ':') {
        throw "Theme name contains invalid characters. Use only alphanumeric characters, hyphens, and underscores"
    }
    
    # Check for reserved names and dangerous patterns
    $reservedNames = @('con', 'prn', 'aux', 'nul', 'com1', 'com2', 'com3', 'com4', 'com5', 'com6', 'com7', 'com8', 'com9', 'lpt1', 'lpt2', 'lpt3', 'lpt4', 'lpt5', 'lpt6', 'lpt7', 'lpt8', 'lpt9')
    if ($reservedNames -contains $Name.ToLower()) {
        throw "Theme name '$Name' is reserved and cannot be used"
    }
    
    # Validate length
    if ($Name.Length -gt 50) {
        throw "Theme name is too long. Maximum 50 characters allowed"
    }
    
    # Ensure valid filename characters only
    if ($Name -notmatch '^[a-zA-Z0-9_-]+$') {
        throw "Theme name must contain only alphanumeric characters, hyphens, and underscores"
    }
}

function Test-OutputPathSecurity {
    param([string]$Path)
    
    # Resolve and validate output path
    try {
        $resolvedPath = Resolve-Path $Path -ErrorAction Stop
        $canonicalPath = $resolvedPath.Path
    }
    catch {
        # Path doesn't exist, validate parent directory
        $parentPath = Split-Path $Path -Parent
        if (-not (Test-Path $parentPath)) {
            throw "Output directory does not exist: $parentPath"
        }
        $canonicalPath = (Resolve-Path $parentPath).Path
    }
    
    # Ensure we're not writing outside allowed areas
    $currentLocation = Get-Location
    if (-not $canonicalPath.StartsWith($currentLocation.Path)) {
        throw "Output path must be within current directory or subdirectories"
    }
}

function New-RRBlazorTheme {
    # Security validation
    Test-ThemeNameSecurity -Name $ThemeName
    Test-OutputPathSecurity -Path $OutputPath
    
    $sanitizedThemeName = $ThemeName.ToLower() -replace '[^a-z0-9_-]', ''
    $themePath = Join-Path $OutputPath "$sanitizedThemeName.scss"
    
    Write-Host "Creating RR.Blazor theme: $sanitizedThemeName" -ForegroundColor Green
    Write-Host "Output path: $themePath" -ForegroundColor Gray
    
    # Create backup if file exists
    if (Test-Path $themePath) {
        $backupPath = "$themePath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Write-Host "Backing up existing theme to: $backupPath" -ForegroundColor Yellow
        Copy-Item $themePath $backupPath
        
        $response = Read-Host "Theme file already exists (backup created). Overwrite? (y/N)"
        if ($response -ne 'y') {
            Write-Host "Theme generation cancelled." -ForegroundColor Yellow
            return
        }
    }
    
    $themeContent = Get-ThemeTemplate -Name $sanitizedThemeName -FullTemplate $IncludeAllVariables
    
    # Ensure directory exists and write atomically
    $directory = Split-Path $themePath -Parent
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    
    # Write to temporary file first, then move (atomic operation)
    $tempPath = "$themePath.tmp"
    try {
        $themeContent | Out-File -FilePath $tempPath -Encoding UTF8 -Force
        Move-Item $tempPath $themePath -Force
    }
    catch {
        if (Test-Path $tempPath) {
            Remove-Item $tempPath -Force
        }
        throw "Failed to write theme file: $_"
    }
    
    Write-Host "`nTheme created successfully!" -ForegroundColor Green
    Write-Host "Theme file: $themePath" -ForegroundColor Gray
    Write-Host "`nTo use this theme:" -ForegroundColor Cyan
    Write-Host "1. Place the theme file in your project" -ForegroundColor White
    Write-Host "2. In Program.cs, use:" -ForegroundColor White
    Write-Host "   builder.Services.AddRRBlazor(options =>" -ForegroundColor Yellow
    Write-Host "   {" -ForegroundColor Yellow
    Write-Host "       options.WithCustomTheme(`"$sanitizedThemeName`", `"path/to/$sanitizedThemeName.scss`");" -ForegroundColor Yellow
    Write-Host "   });" -ForegroundColor Yellow
    Write-Host "`n3. The theme will be compiled into your CSS at build time" -ForegroundColor White
    Write-Host "`nTip: Customize the CSS variables in the generated file to match your brand!" -ForegroundColor Gray
    Write-Host "`nSecurity: Theme name was sanitized to: $sanitizedThemeName" -ForegroundColor Magenta
}

try {
    New-RRBlazorTheme
}
catch {
    Write-Host "Error creating theme: $_" -ForegroundColor Red
    exit 1
}