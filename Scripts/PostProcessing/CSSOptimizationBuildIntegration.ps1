<#
.SYNOPSIS
    RR.Blazor Build Integration Script for CSS Tree-Shaking
    
.DESCRIPTION
    Integrates CSS tree-shaking optimization into the standard .NET build process.
    Can be called from MSBuild targets, CI/CD pipelines, or development workflows.

.PARAMETER BuildConfiguration
    Build configuration (Debug, Release)
    
.PARAMETER ProjectPath
    Path to the project being built
    
.PARAMETER Force
    Force optimization even if cache indicates no changes
    
.PARAMETER Silent
    Suppress all output except errors
    
.EXAMPLE
    .\BuildIntegration.ps1 -BuildConfiguration Release -ProjectPath "C:\MyApp"
    
.EXAMPLE
    .\BuildIntegration.ps1 -Force -Silent
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$BuildConfiguration = "Release",
    
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = ".",
    
    [switch]$Force = $false,
    
    [switch]$Silent = $false
)

# PowerShell version check and auto-install
function Ensure-PowerShell7 {
    $currentVersion = $PSVersionTable.PSVersion
    if ($currentVersion.Major -lt 7) {
        Write-Host "⚠️  PowerShell 7+ required. Current: $currentVersion" -ForegroundColor Yellow
        
        if ($IsWindows -or $env:OS -eq "Windows_NT") {
            Write-Host "🔧 Installing PowerShell 7 via winget..." -ForegroundColor Cyan
            try {
                winget install Microsoft.PowerShell --silent --accept-package-agreements --accept-source-agreements
                Write-Host "✅ PowerShell 7 installed. Restart terminal and re-run script." -ForegroundColor Green
                exit 0
            }
            catch {
                Write-Host "❌ Winget failed. Manual install: https://aka.ms/powershell-release" -ForegroundColor Red
                exit 1
            }
        }
        else {
            Write-Host "❌ Install PowerShell 7+: https://aka.ms/powershell-release" -ForegroundColor Red
            exit 1
        }
    }
}

Ensure-PowerShell7

# Set error handling
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Logging function
function Write-BuildLog {
    param([string]$Message, [string]$Level = "INFO")
    
    if ($Silent -and $Level -ne "ERROR") {
        return
    }
    
    $timestamp = Get-Date -Format "HH:mm:ss"
    $prefix = switch ($Level) {
        "ERROR" { "##[error]" }
        "WARN"  { "##[warning]" }
        "INFO"  { "##[info]" }
        default { "" }
    }
    
    Write-Host "$prefix[$timestamp] $Message"
}

Write-BuildLog "🔧 RR.Blazor Build Integration Started" "INFO"
Write-BuildLog "   Configuration: $BuildConfiguration" "INFO"
Write-BuildLog "   Project Path: $ProjectPath" "INFO"

# Resolve project path
$ProjectPath = Resolve-Path $ProjectPath -ErrorAction SilentlyContinue
if (-not $ProjectPath) {
    Write-BuildLog "Project path not found: $ProjectPath" "ERROR"
    exit 1
}

# Check if tree-shaking is enabled for this project
$configFile = Join-Path $ProjectPath "rr-blazor.config.json"
$enableTreeShaking = $true
$goldenRatioEnabled = $true

if (Test-Path $configFile) {
    try {
        $config = Get-Content $configFile | ConvertFrom-Json
        $enableTreeShaking = $config.treeShaking.enabled ?? $true
        $goldenRatioEnabled = $config.goldenRatio.enabled ?? $true
        Write-BuildLog "Loaded configuration from rr-blazor.config.json" "INFO"
    }
    catch {
        Write-BuildLog "Warning: Could not parse rr-blazor.config.json, using defaults" "WARN"
    }
}

# Skip tree-shaking in Debug mode unless forced
if ($BuildConfiguration -eq "Debug" -and -not $Force) {
    Write-BuildLog "Skipping tree-shaking in Debug configuration (use -Force to override)" "INFO"
    exit 0
}

# Skip if tree-shaking is disabled
if (-not $enableTreeShaking) {
    Write-BuildLog "Tree-shaking is disabled in configuration" "INFO"
    exit 0
}

# Check for required files
$treeShakeScript = Join-Path $PSScriptRoot "TreeShakeOptimize.ps1"
if (-not (Test-Path $treeShakeScript)) {
    Write-BuildLog "TreeShakeOptimize.ps1 not found at: $treeShakeScript" "ERROR"
    exit 1
}

$componentRegistry = Join-Path $ProjectPath "wwwroot/component-registry.json"
if (-not (Test-Path $componentRegistry)) {
    Write-BuildLog "Component registry not found, tree-shaking may be less effective" "WARN"
}

# Prepare parameters for tree-shaking script
$treeShakeParams = @{
    ProjectPath = $ProjectPath
    OutputPath = "./wwwroot/css/optimized"
    EnableGoldenRatio = $goldenRatioEnabled
    ComponentRegistry = "./wwwroot/component-registry.json"
}

# Add verbosity based on build configuration
if ($BuildConfiguration -eq "Debug" -or -not $Silent) {
    $treeShakeParams.Verbose = $true
}

# Build parameter string for PowerShell execution
$paramString = ($treeShakeParams.GetEnumerator() | ForEach-Object {
    if ($_.Value -is [bool] -and $_.Value) {
        "-$($_.Key)"
    } elseif ($_.Value -is [bool] -and -not $_.Value) {
        # Skip false boolean parameters
    } else {
        "-$($_.Key) `"$($_.Value)`""
    }
}) -join " "

Write-BuildLog "🚀 Starting CSS optimization..." "INFO"

try {
    # Execute tree-shaking optimization
    $startTime = Get-Date
    
    $process = Start-Process -FilePath "pwsh" -ArgumentList @(
        "-ExecutionPolicy", "Bypass",
        "-File", $treeShakeScript,
        $paramString -split " "
    ) -Wait -PassThru -NoNewWindow -RedirectStandardOutput "tree-shake-output.log" -RedirectStandardError "tree-shake-error.log"
    
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalSeconds
    
    # Read output and error logs
    $output = if (Test-Path "tree-shake-output.log") { Get-Content "tree-shake-output.log" -Raw } else { "" }
    $errors = if (Test-Path "tree-shake-error.log") { Get-Content "tree-shake-error.log" -Raw } else { "" }
    
    # Display output if not silent
    if (-not $Silent -and $output) {
        Write-Host $output
    }
    
    # Always display errors
    if ($errors) {
        Write-BuildLog "Tree-shaking errors:" "ERROR"
        Write-Host $errors -ForegroundColor Red
    }
    
    if ($process.ExitCode -eq 0) {
        Write-BuildLog "✅ CSS optimization completed successfully in $([math]::Round($duration, 1))s" "INFO"
        
        # Read and display optimization results
        $reportPath = Join-Path $ProjectPath "wwwroot/css/optimized/optimization-report.json"
        if (Test-Path $reportPath) {
            try {
                $report = Get-Content $reportPath | ConvertFrom-Json
                $savings = [math]::Round($report.savings.percentage, 1)
                $originalKB = [math]::Round($report.originalSize.kb, 1)
                $optimizedKB = [math]::Round($report.optimizedSize.kb, 1)
                
                Write-BuildLog "📊 Optimization Results:" "INFO"
                Write-BuildLog "   • Original size: $originalKB KB" "INFO"
                Write-BuildLog "   • Optimized size: $optimizedKB KB" "INFO"
                Write-BuildLog "   • Space saved: $savings% ($($originalKB - $optimizedKB) KB)" "INFO"
                Write-BuildLog "   • Components: $($report.components.PSObject.Properties.Count)" "INFO"
                Write-BuildLog "   • Utilities: $($report.utilities.PSObject.Properties.Count)" "INFO"
            }
            catch {
                Write-BuildLog "Could not read optimization report" "WARN"
            }
        }
        
        # Update build cache
        $cacheDir = Join-Path $ProjectPath "obj/rr-blazor-cache"
        if (-not (Test-Path $cacheDir)) {
            New-Item -ItemType Directory -Path $cacheDir -Force | Out-Null
        }
        
        $cacheData = @{
            lastOptimization = Get-Date
            buildConfiguration = $BuildConfiguration
            projectPath = $ProjectPath.ToString()
            version = "2.0.0"
        }
        
        $cacheData | ConvertTo-Json | Set-Content -Path (Join-Path $cacheDir "build-cache.json")
        
    } else {
        Write-BuildLog "❌ CSS optimization failed with exit code $($process.ExitCode)" "ERROR"
        exit $process.ExitCode
    }
}
catch {
    Write-BuildLog "❌ CSS optimization failed: $($_.Exception.Message)" "ERROR"
    exit 1
}
finally {
    # Clean up temporary log files
    @("tree-shake-output.log", "tree-shake-error.log") | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item $_ -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-BuildLog "🏁 Build integration completed successfully" "INFO"
exit 0