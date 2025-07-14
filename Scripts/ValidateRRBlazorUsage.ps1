[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SolutionPath,
    
    [Parameter(Mandatory=$false)]
    [string]$AIDocsPath = "$PSScriptRoot\..\wwwroot\rr-ai-docs.json",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFormat = "msbuild", # msbuild, json, or console
    
    [Parameter(Mandatory=$false)]
    [switch]$FailOnWarning = $false
)

# Initialize counters
$script:errorCount = 0
$script:warningCount = 0
$script:issues = @()

# Load AI documentation
function Load-AIDocumentation {
    if (-not (Test-Path $AIDocsPath)) {
        Write-Error "AI documentation not found at: $AIDocsPath"
        exit 1
    }
    
    $docs = Get-Content $AIDocsPath -Raw | ConvertFrom-Json
    return $docs
}

# Output diagnostic message in MSBuild format
function Write-Diagnostic {
    param(
        [string]$FilePath,
        [int]$Line = 1,
        [int]$Column = 1,
        [string]$Severity, # error, warning, info
        [string]$Code,
        [string]$Message
    )
    
    $issue = @{
        File = $FilePath
        Line = $Line
        Column = $Column
        Severity = $Severity
        Code = $Code
        Message = $Message
    }
    
    $script:issues += $issue
    
    if ($Severity -eq "error") {
        $script:errorCount++
    } elseif ($Severity -eq "warning") {
        $script:warningCount++
    }
    
    if ($OutputFormat -eq "msbuild") {
        # MSBuild format: file(line,col): severity code: message
        Write-Host "${FilePath}(${Line},${Column}): $Severity $Code`: $Message"
    } elseif ($OutputFormat -eq "console") {
        $color = switch ($Severity) {
            "error" { "Red" }
            "warning" { "Yellow" }
            default { "White" }
        }
        Write-Host "[$Severity] $FilePath`:$Line - $Message" -ForegroundColor $color
    }
}

# Parse component usage from Razor files
function Get-ComponentUsage {
    param([string]$Content, [string]$FilePath)
    
    $usages = @()
    
    # Regex to match RR.Blazor components: <RComponentName param="value" />
    $componentRegex = '<(R[A-Z][a-zA-Z]*)\s+([^>]*?)/?>'
    $matches = [regex]::Matches($Content, $componentRegex)
    
    foreach ($match in $matches) {
        $componentName = $match.Groups[1].Value
        $parametersText = $match.Groups[2].Value
        $lineNumber = ($Content.Substring(0, $match.Index) -split "`n").Count
        
        # Parse parameters
        $parameters = @{}
        $paramRegex = '([a-zA-Z][a-zA-Z0-9]*)\s*=\s*"([^"]*)"'
        $paramMatches = [regex]::Matches($parametersText, $paramRegex)
        
        foreach ($paramMatch in $paramMatches) {
            $paramName = $paramMatch.Groups[1].Value
            $paramValue = $paramMatch.Groups[2].Value
            $parameters[$paramName] = $paramValue
        }
        
        $usages += @{
            Component = $componentName
            Parameters = $parameters
            Line = $lineNumber
            Column = $match.Index - ($Content.LastIndexOf("`n", $match.Index) + 1)
            FullMatch = $match.Value
        }
    }
    
    return $usages
}

# Validate component parameters
function Test-ComponentParameters {
    param(
        [hashtable]$Usage,
        [PSCustomObject]$ComponentDocs,
        [string]$FilePath
    )
    
    $componentName = $Usage.Component
    
    # Check if component exists
    $components = $ComponentDocs.components
    if (-not $components -or -not $components.$componentName) {
        Write-Diagnostic -FilePath $FilePath -Line $Usage.Line -Column $Usage.Column `
            -Severity "warning" -Code "RR1002" `
            -Message "Unknown RR.Blazor component '$componentName'. Check component name spelling."
        return
    }
    
    $componentInfo = $components.$componentName
    $validParameters = @()
    
    # Get valid parameters from documentation
    if ($componentInfo.parameters) {
        $parameterNames = @()
        $componentInfo.parameters.PSObject.Properties | ForEach-Object {
            $parameterNames += $_.Name
        }
        $validParameters = $parameterNames
        
        # Debug: Show what parameters were found for RButton
        if ($componentName -eq "RButton" -and $FilePath -match "TestSingle") {
            Write-Host "DEBUG: RButton parameters found: $($parameterNames -join ', ')" -ForegroundColor Magenta
        }
    }
    
    # Add common Blazor parameters
    $commonParameters = @("Class", "Id", "ChildContent", "OnClick", "Disabled", "@bind-Value", "@ref")
    $validParameters += $commonParameters
    
    # Deprecated parameter mappings
    $deprecatedParams = @{
        "Style" = @{ Replacement = "Class"; Message = "use utility classes instead" }
        "IsClickable" = @{ Replacement = "Clickable"; Message = "" }
        "Icon" = @{ Replacement = "StartIcon or EndIcon"; Message = "based on position" }
        "IconPosition" = @{ Replacement = "StartIcon or EndIcon"; Message = "use StartIcon/EndIcon directly" }
    }
    
    # Check each parameter used
    foreach ($param in $Usage.Parameters.Keys) {
        $paramName = $param -replace '^@', '' # Remove @ prefix for binding
        
        # Check for deprecated parameters
        if ($deprecatedParams.ContainsKey($paramName)) {
            $deprecated = $deprecatedParams[$paramName]
            $message = "Parameter '$paramName' on component '$componentName' is deprecated. Use '$($deprecated.Replacement)' instead"
            if ($deprecated.Message) {
                $message += " ($($deprecated.Message))"
            }
            
            Write-Diagnostic -FilePath $FilePath -Line $Usage.Line -Column $Usage.Column `
                -Severity "warning" -Code "RR1003" -Message "$message."
            continue
        }
        
        # Check if parameter is valid
        if ($paramName -notin $validParameters) {
            $availableParams = ($validParameters | Where-Object { $_ -notlike "@*" } | Select-Object -First 10) -join ", "
            if ($validParameters.Count -gt 10) {
                $availableParams += "..."
            }
            
            Write-Diagnostic -FilePath $FilePath -Line $Usage.Line -Column $Usage.Column `
                -Severity "error" -Code "RR1001" `
                -Message "Component '$componentName' does not have a parameter named '$paramName'. Available parameters: $availableParams."
        }
    }
}

# Check for common issues
function Test-CommonIssues {
    param(
        [hashtable]$Usage,
        [string]$FilePath,
        [string]$Content
    )
    
    $componentName = $Usage.Component
    $line = $Usage.Line
    
    # Check for inline styles
    if ($Usage.Parameters.ContainsKey("Style")) {
        Write-Diagnostic -FilePath $FilePath -Line $line -Column $Usage.Column `
            -Severity "error" -Code "RR1004" `
            -Message "Inline styles are not allowed. Use utility classes with the 'Class' parameter instead."
    }
    
    # Check for missing required parameters based on component
    switch ($componentName) {
        "RFormField" {
            if (-not $Usage.Parameters.ContainsKey("Label") -and 
                -not $Usage.Parameters.ContainsKey("LabelContent")) {
                Write-Diagnostic -FilePath $FilePath -Line $line -Column $Usage.Column `
                    -Severity "warning" -Code "RR1005" `
                    -Message "RFormField should have a Label for accessibility."
            }
        }
        
        "RButton" {
            # Check for empty OnClick
            if ($Usage.Parameters.ContainsKey("OnClick") -and 
                [string]::IsNullOrWhiteSpace($Usage.Parameters["OnClick"])) {
                Write-Diagnostic -FilePath $FilePath -Line $line -Column $Usage.Column `
                    -Severity "warning" -Code "RR1006" `
                    -Message "RButton has empty OnClick handler."
            }
        }
        
        "RDataTable" {
            # Check if it has columns defined
            $nextLines = $Content -split "`n" | Select-Object -Skip ($line - 1) -First 10
            $hasColumns = $nextLines -join " " -match "<RDataTableColumn"
            if (-not $hasColumns) {
                Write-Diagnostic -FilePath $FilePath -Line $line -Column $Usage.Column `
                    -Severity "warning" -Code "RR1007" `
                    -Message "RDataTable should have RDataTableColumn definitions."
            }
        }
    }
    
    # Check for common typos
    $commonTypos = @{
        "Lable" = "Label"
        "Buttom" = "Button"
        "Clas" = "Class"
        "Clickabel" = "Clickable"
        "Disabeld" = "Disabled"
    }
    
    foreach ($param in $Usage.Parameters.Keys) {
        if ($commonTypos.ContainsKey($param)) {
            Write-Diagnostic -FilePath $FilePath -Line $line -Column $Usage.Column `
                -Severity "error" -Code "RR1008" `
                -Message "Typo in parameter name '$param'. Did you mean '$($commonTypos[$param])'?"
        }
    }
}

# Main validation function
function Start-Validation {
    Write-Host "Loading RR.Blazor AI documentation..." -ForegroundColor Cyan
    $aiDocs = Load-AIDocumentation
    
    # Find solution file and directory
    $solutionFile = Get-Item $SolutionPath -ErrorAction Stop
    $solutionDir = $solutionFile.DirectoryName
    
    Write-Host "Scanning for Razor files in solution directory: $solutionDir" -ForegroundColor Cyan
    
    # Get all .razor files in the solution (excluding bin/obj folders)
    $razorFiles = Get-ChildItem -Path $solutionDir -Filter "*.razor" -Recurse |
        Where-Object { 
            $_.FullName -notmatch "\\(bin|obj)\\" -and
            $_.FullName -notmatch "/(bin|obj)/"
        }
    
    Write-Host "Found $($razorFiles.Count) Razor files to validate" -ForegroundColor Gray
    
    foreach ($file in $razorFiles) {
        $content = Get-Content $file.FullName -Raw
        $relativePath = $file.FullName.Substring($solutionDir.Length).TrimStart('\', '/')
        
        # Skip if no RR.Blazor components
        if ($content -notmatch '<R[A-Z]') {
            continue
        }
        
        # Get all component usages in the file
        $usages = Get-ComponentUsage -Content $content -FilePath $relativePath
        
        foreach ($usage in $usages) {
            # Validate parameters
            Test-ComponentParameters -Usage $usage -ComponentDocs $aiDocs -FilePath $relativePath
            
            # Check for common issues
            Test-CommonIssues -Usage $usage -FilePath $relativePath -Content $content
        }
    }
    
    # Output summary
    Write-Host "`n========== Validation Summary ==========" -ForegroundColor Cyan
    Write-Host "Errors: $script:errorCount" -ForegroundColor $(if ($script:errorCount -gt 0) { "Red" } else { "Green" })
    Write-Host "Warnings: $script:warningCount" -ForegroundColor $(if ($script:warningCount -gt 0) { "Yellow" } else { "Green" })
    
    if ($OutputFormat -eq "json") {
        $script:issues | ConvertTo-Json -Depth 10
    }
    
    # Exit with error code if errors found
    if ($script:errorCount -gt 0 -or ($FailOnWarning -and $script:warningCount -gt 0)) {
        exit 1
    }
    
    exit 0
}

# Run validation
Start-Validation