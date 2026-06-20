# pack.ps1
# This script builds the projects locally and packages them into ZIP archives matching the structure in README.md

$ErrorActionPreference = "Stop"

# Define paths
$projectRoot = $PSScriptRoot
$distDir = Join-Path $projectRoot "dist"
$tempDir = Join-Path $distDir "temp"

# Clean previous builds
if (Test-Path $distDir) {
    Remove-Item -Recurse -Force $distDir
}
New-Item -ItemType Directory -Path $distDir | Out-Null

# Read version from AssemblyInfo.cs (Below Zero)
$bzAssemblyInfo = Get-Content (Join-Path $projectRoot "src-bz\AssemblyInfo.cs") -Raw
if ($bzAssemblyInfo -match 'version = "([^"]+)"') {
    $version = $Matches[1]
} else {
    $version = "0.0.5" # fallback
}

Write-Host "Detected version: v$version" -ForegroundColor Green

# 1. Build projects in Release configuration
Write-Host "Building Subnautica 1 mod..." -ForegroundColor Cyan
dotnet build (Join-Path $projectRoot "efool-custom-inventory.csproj") -c Release

Write-Host "Building Subnautica Below Zero mod..." -ForegroundColor Cyan
dotnet build (Join-Path $projectRoot "efool-bz-custom-inventory.csproj") -c Release

# 2. Create directory structure for Subnautica 1
$snTemp = Join-Path $tempDir "subnautica\BepInEx\plugins\efool-custom-inventory"
New-Item -ItemType Directory -Path $snTemp -Force | Out-Null
Copy-Item (Join-Path $projectRoot "bin\Release\efool-custom-inventory.dll") -Destination $snTemp
Copy-Item (Join-Path $projectRoot "res\presets.json") -Destination $snTemp

# 3. Create directory structure for Below Zero
$bzTemp = Join-Path $tempDir "belowzero\BepInEx\plugins\efool-bz-custom-inventory"
New-Item -ItemType Directory -Path $bzTemp -Force | Out-Null
Copy-Item (Join-Path $projectRoot "bin\ReleaseBZ\efool-bz-custom-inventory.dll") -Destination $bzTemp
Copy-Item (Join-Path $projectRoot "res\presets.json") -Destination $bzTemp

# 4. Create ZIP archives
$snZip = Join-Path $distDir "efool-custom-inventory_v$version.zip"
$bzZip = Join-Path $distDir "efool-bz-custom-inventory_v$version.zip"

Write-Host "Packaging Subnautica 1 archive..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $tempDir "subnautica\*") -DestinationPath $snZip

Write-Host "Packaging Subnautica Below Zero archive..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $tempDir "belowzero\*") -DestinationPath $bzZip

# Clean up temporary folders
Remove-Item -Recurse -Force $tempDir

Write-Host "--------------------------------------------------" -ForegroundColor Gray
Write-Host "Done! Archives successfully created in 'dist/' folder:" -ForegroundColor Green
Write-Host " - $(Split-Path $snZip -Leaf)"
Write-Host " - $(Split-Path $bzZip -Leaf)"
Write-Host "--------------------------------------------------" -ForegroundColor Gray

$ghCheck = Get-Command gh -ErrorAction SilentlyContinue
if ($ghCheck) {
    $response = Read-Host "GitHub CLI (gh) detected. Would you like to create a Draft Release v$version and upload these ZIPs now? (y/n)"
    if ($response -eq 'y' -or $response -eq 'yes') {
        Write-Host "Creating Draft Release v$version on GitHub..." -ForegroundColor Cyan
        
        $repoName = ""
        $repoUrl = git config --get remote.origin.url
        if ($repoUrl -match 'github\.com[:/]([^/]+/[^.]+)') {
            $repoName = $Matches[1]
        }
        
        if ($repoName) {
            gh release create "v$version" $snZip $bzZip --repo $repoName --title "v$version" --notes "Release v$version" --draft
        } else {
            gh release create "v$version" $snZip $bzZip --title "v$version" --notes "Release v$version" --draft
        }
        Write-Host "Draft Release v$version created successfully with attached archives!" -ForegroundColor Green
    }
} else {
    Write-Host "You can now upload these files to GitHub Releases." -ForegroundColor Yellow
    Write-Host "Tip: Install GitHub CLI (https://cli.github.com) to automate uploading directly from this script." -ForegroundColor Gray
}
