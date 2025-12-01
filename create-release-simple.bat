@echo off
echo ========================================
echo   Jellyfin Trailer Preview - Release
echo ========================================
echo.
echo IMPORTANT: Ce script suppose que vous avez deja compile le plugin
echo ou que vous allez telecharger le DLL compile.
echo.
pause

REM Check if DLL exists
if not exist "Jellyfin.Plugin.TrailerPreview\bin\Release\net8.0\Jellyfin.Plugin.TrailerPreview.dll" (
    echo.
    echo ERREUR: Le DLL n'existe pas!
    echo.
    echo Vous devez soit:
    echo 1. Installer .NET SDK: winget install Microsoft.DotNet.SDK.8
    echo 2. Compiler sur une autre machine
    echo 3. Telecharger le DLL compile
    echo.
    echo Voulez-vous creer la release SANS le DLL pour l'instant? (O/N)
    set /p response=
    if /i not "%response%"=="O" exit /b

    echo.
    echo Creation d'un fichier temporaire...
    mkdir release 2>nul
    echo Plugin temporaire - A remplacer > release\README.txt
    powershell -Command "Compress-Archive -Path release\README.txt -DestinationPath jellyfin-plugin-trailerpreview_1.0.0.zip -Force"

    echo.
    echo ZIP temporaire cree. Vous devrez le remplacer plus tard.
    echo Checksum temporaire: 00000000000000000000000000000000
    set CHECKSUM=00000000000000000000000000000000
) else (
    echo [1/3] Creating release package...
    if exist release rmdir /s /q release
    mkdir release

    copy "Jellyfin.Plugin.TrailerPreview\bin\Release\net8.0\Jellyfin.Plugin.TrailerPreview.dll" release\

    if exist jellyfin-plugin-trailerpreview_1.0.0.zip del jellyfin-plugin-trailerpreview_1.0.0.zip
    powershell -Command "Compress-Archive -Path release\* -DestinationPath jellyfin-plugin-trailerpreview_1.0.0.zip"
    echo Package created!
    echo.

    echo [2/3] Calculating checksum...
    powershell -Command "$hash = Get-FileHash -Path 'jellyfin-plugin-trailerpreview_1.0.0.zip' -Algorithm MD5; Write-Host 'MD5:' $hash.Hash.ToLower(); $hash.Hash.ToLower() | Out-File -FilePath checksum.txt -NoNewline"
    set /p CHECKSUM=<checksum.txt
    echo Checksum: %CHECKSUM%
    echo.
)

echo [3/3] Next steps:
echo.
echo 1. Go to: https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/releases/new
echo 2. Fill in:
echo    - Tag: v1.0.0
echo    - Title: Trailer Preview v1.0.0
echo    - Description: Initial release - Netflix-style trailer previews
echo 3. Upload: jellyfin-plugin-trailerpreview_1.0.0.zip
echo 4. Click "Publish release"
echo.
echo 5. After release, press any key to update manifest...
pause

echo.
echo Updating manifest.json...
powershell -Command "(Get-Content manifest.json) -replace 'yourusername', 'Ziifkah' -replace '00000000000000000000000000000000', '%CHECKSUM%' -replace '\"owner\": \"jellyfin\"', '\"owner\": \"Ziifkah\"' | Set-Content manifest.json"
echo Manifest updated!
echo.

echo Committing...
git add manifest.json
git commit -m "Update manifest with v1.0.0 release info"
git push origin main

echo.
echo ========================================
echo   SUCCESS!
echo ========================================
echo.
echo Installation URL:
echo https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json
echo.
pause
