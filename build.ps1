param(
    [switch]$SkipPublish = $false,
    [switch]$SkipInstaller = $false
)

$ErrorActionPreference = "Stop"
$rootDir = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $rootDir "MacroDeck"
$publishDir = Join-Path $projectDir "bin\Publish"
$setupDir = Join-Path $rootDir "MacroDeck.Setup"
$issFile = Join-Path $setupDir "Macro Deck.iss"

if (-not $SkipPublish) {
    Write-Host "=== Step 1/2: Publishing Macro Deck Pro ===" -ForegroundColor Cyan
    dotnet publish $projectDir\MacroDeck.csproj `
        -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
    Write-Host "Publish succeeded!" -ForegroundColor Green
}

if (-not $SkipInstaller) {
    $iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (-not (Test-Path $iscc)) {
        $iscc = "C:\Program Files\Inno Setup 6\ISCC.exe"
    }
    if (-not (Test-Path $iscc)) {
        Write-Warning "Inno Setup not found. Install from https://jrsoftware.org/isdl.php"
        Write-Host "Portable build available at: $publishDir\Macro Deck 2.exe" -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host "=== Step 2/2: Building Installer ===" -ForegroundColor Cyan
    & $iscc $issFile
    if ($LASTEXITCODE -ne 0) { throw "Installer build failed" }
    
    $resultDir = Join-Path $setupDir "Result"
    $installer = Get-ChildItem $resultDir -Filter *.exe | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    Write-Host "Installer built: $($installer.FullName)" -ForegroundColor Green
}
