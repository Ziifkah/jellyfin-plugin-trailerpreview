@echo off
echo ========================================
echo   Jellyfin Trailer Preview - Release
echo ========================================
echo.

REM Build the plugin
echo [1/5] Building plugin...
dotnet build Jellyfin.Plugin.TrailerPreview.sln --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build complete!
echo.

REM Create release folder
echo [2/5] Creating release package...
if exist release rmdir /s /q release
mkdir release

REM Copy DLL
copy "Jellyfin.Plugin.TrailerPreview\bin\Release\net8.0\Jellyfin.Plugin.TrailerPreview.dll" release\

REM Create ZIP
if exist jellyfin-plugin-trailerpreview_1.0.0.zip del jellyfin-plugin-trailerpreview_1.0.0.zip
powershell -Command "Compress-Archive -Path release\* -DestinationPath jellyfin-plugin-trailerpreview_1.0.0.zip"
echo Package created!
echo.

REM Calculate MD5
echo [3/5] Calculating checksum...
powershell -Command "$hash = Get-FileHash -Path 'jellyfin-plugin-trailerpreview_1.0.0.zip' -Algorithm MD5; Write-Host 'MD5:' $hash.Hash.ToLower(); $hash.Hash.ToLower() | Out-File -FilePath checksum.txt -NoNewline"
set /p CHECKSUM=<checksum.txt
echo Checksum: %CHECKSUM%
echo.

echo [4/5] Next steps:
echo.
echo 1. Go to: https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/releases/new
echo 2. Fill in:
echo    - Tag: v1.0.0
echo    - Title: Trailer Preview v1.0.0
echo    - Description: Initial release - Netflix-style trailer previews
echo 3. Upload the file: jellyfin-plugin-trailerpreview_1.0.0.zip
echo 4. Click "Publish release"
echo.
echo 5. After creating the release, press any key here to update manifest...
pause

REM Update manifest.json
echo.
echo [5/5] Updating manifest.json...
powershell -Command "(Get-Content manifest.json) -replace 'yourusername', 'Ziifkah' -replace '00000000000000000000000000000000', '%CHECKSUM%' -replace '\"owner\": \"jellyfin\"', '\"owner\": \"Ziifkah\"' | Set-Content manifest.json"
echo Manifest updated!
echo.

REM Commit and push
echo Committing changes...
git add manifest.json
git commit -m "Update manifest with v1.0.0 release info"
git push origin main
echo.

echo ========================================
echo   SUCCESS! Plugin is ready!
echo ========================================
echo.
echo Installation URL for Jellyfin:
echo https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json
echo.
echo Test it now in Jellyfin!
echo.
pause
