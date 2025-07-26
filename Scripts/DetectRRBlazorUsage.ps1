#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory = $false)]
    [string]$OutputFormat = "console"
)

function Write-Output-Message {
    param($Message, $Type = "Info")
    
    if ($OutputFormat -eq "msbuild") {
        switch ($Type) {
            "Error" { Write-Host "##vso[task.logissue type=error]$Message" }
            "Warning" { Write-Host "##vso[task.logissue type=warning]$Message" }
            default { Write-Host $Message }
        }
    } else {
        Write-Host $Message
    }
}

try {
    $hasAddRRBlazor = $false
    $hasDisableValidation = $false
    
    # Search for Program.cs and Startup.cs files
    $programFiles = Get-ChildItem -Path $ProjectPath -Name "Program.cs", "Startup.cs" -Recurse -ErrorAction SilentlyContinue
    
    if ($programFiles) {
        foreach ($file in $programFiles) {
            $fullPath = Join-Path $ProjectPath $file
            if (Test-Path $fullPath) {
                $content = Get-Content $fullPath -Raw -ErrorAction SilentlyContinue
                
                if ($content -match 'AddRRBlazor\s*\(') {
                    $hasAddRRBlazor = $true
                    Write-Output-Message "✅ Found AddRRBlazor() call in: $file"
                    
                    # Check if DisableValidation() is called
                    if ($content -match '\.DisableValidation\s*\(') {
                        $hasDisableValidation = $true
                        Write-Output-Message "🚫 Found DisableValidation() - validation will be disabled"
                    }
                }
            }
        }
    }
    
    if ($hasAddRRBlazor) {
        if ($hasDisableValidation) {
            Write-Output-Message "DETECTED_RRBLAZOR_WITH_DISABLE_VALIDATION"
            exit 2  # Detected but disabled
        } else {
            Write-Output-Message "DETECTED_RRBLAZOR_ENABLE_VALIDATION"
            exit 1  # Detected, enable validation
        }
    } else {
        Write-Output-Message "NO_RRBLAZOR_DETECTED"
        exit 0  # Not detected
    }
}
catch {
    Write-Output-Message "Error detecting RR.Blazor usage: $_" "Error"
    exit 0  # On error, don't enable validation
}