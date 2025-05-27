# === Configuration ===
$solution = "rpf2fivem.sln"
$configuration = "Release"
$outputDir = "build_output"

function Write-Log {
    param (
        [string]$Message,
        [ConsoleColor]$Color = 'White'
    )
    $oldColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Host "[+] $Message"
    $Host.UI.RawUI.ForegroundColor = $oldColor
}

function Show-ProgressBar {
    param (
        [string]$Activity,
        [int]$PercentComplete
    )
    Write-Progress -Activity $Activity -PercentComplete $PercentComplete
}

# === Clean output directory ===
Write-Log "Cleaning output directory..." Cyan
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

# === Locate MSBuild.exe ===
$msbuildPath = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (!(Test-Path $msbuildPath)) {
    Write-Log "MSBuild not found at '$msbuildPath'. Please update the path or install VS2022." Red
    exit 1
}

# === Build the solution ===
Write-Log "Building solution: $solution ($configuration)..." Yellow
& $msbuildPath $solution /p:Configuration=$configuration /p:DebugType=None /p:DebugSymbols=false /m /v:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Log "Build failed. Exiting." Red
    exit 1
} else {
    Write-Log "Build succeeded!" Green
}

# === Project target framework mappings ===
$projects = @{
    "rpf2fivem" = "Release"  # Use actual build subfolder name (like 'AnyCPU' or 'x86')
}

# === Copy built files to outputDir ===
$counter = 0
$total = $projects.Count

foreach ($proj in $projects.Keys) {
    $counter++
    $percent = [int](($counter / $total) * 100)
    Show-ProgressBar -Activity "Copying project files..." -PercentComplete $percent

    $platform = $projects[$proj]
    $buildPath = ".\bin\$platform"

    if (!(Test-Path $buildPath)) {
        Write-Log "Build output not found: $buildPath" Red
        continue
    }

    Write-Log "Copying $proj ($platform)..." DarkCyan
    Copy-Item -Path "$buildPath\*" -Destination $outputDir -Recurse -Force
}

Write-Progress -Activity "Copying project files..." -Completed
Write-Log "All project files copied to '$outputDir'." Green

# === Run Inno Setup ===
$innoPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$innoScript = "installer-script.iss"

if (!(Test-Path $innoPath)) {
    Write-Log "Inno Setup not found at '$innoPath'. Please install it or update the path." Red
    exit 1
}

Write-Log "Running Inno Setup installer script..." Magenta
& $innoPath $innoScript

if ($LASTEXITCODE -ne 0) {
    Write-Log "Inno Setup failed. Please check the script or paths." Red
    exit 1
} else {
    Write-Log "Installer built successfully!" Green
}
