# Build script for TrailerPreview v2
# Usage: .\build.ps1

Write-Host "Building TrailerPreview v2..." -ForegroundColor Green

# Set variables
$projectPath = ".\Jellyfin.Plugin.TrailerPreview\Jellyfin.Plugin.TrailerPreview.csproj"
$binPath = ".\Jellyfin.Plugin.TrailerPreview\bin\Release\net8.0"
$version = "2.0.0"
$zipName = "jellyfin-plugin-trailerpreview_$version.zip"

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
dotnet clean $projectPath --configuration Release

# Restore
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore $projectPath

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create meta.json
Write-Host "Creating meta.json..." -ForegroundColor Yellow
$metaPath = Join-Path $binPath "meta.json"
$meta = @{
    guid = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    name = "Trailer Preview"
    description = "Display movie and TV show trailers in a Netflix-style overlay when hovering over items"
    overview = "A modern, elegant solution for previewing trailers by hovering over movies and TV shows. Features include customizable timing, positioning, size controls, background effects, audio settings, and multi-platform support for Web, Desktop, Mobile, and TV clients."
    owner = "Ziifkah"
    category = "General"
    version = $version
    changelog = "Complete rewrite based on HoverTrailer architecture. Netflix-style overlay with comprehensive configuration options. Fixed JavaScript decimal formatting issues. Improved stability and error handling."
    targetAbi = "10.10.0.0"
    timestamp = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json -Depth 10

$meta | Out-File -FilePath $metaPath -Encoding UTF8

# Create ZIP
Write-Host "Creating ZIP package..." -ForegroundColor Yellow
$dllPath = Join-Path $binPath "Jellyfin.Plugin.TrailerPreview.dll"
$zipPath = ".\$zipName"

if (Test-Path $zipPath) {
    Remove-Item $zipPath
}

Compress-Archive -Path $dllPath,$metaPath -DestinationPath $zipPath -Force

# Calculate checksum
Write-Host "Calculating checksum..." -ForegroundColor Yellow
$hash = (Get-FileHash -Path $zipPath -Algorithm MD5).Hash.ToLower()

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "Package: $zipName" -ForegroundColor Cyan
Write-Host "MD5 Checksum: $hash" -ForegroundColor Cyan
Write-Host "`nUpdate manifest.json with this checksum!" -ForegroundColor Yellow
