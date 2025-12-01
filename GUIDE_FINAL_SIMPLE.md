# Guide Ultra-Simple - 5 Minutes

## Étape 1: Installer .NET SDK (2 minutes)

Copiez-collez dans votre CMD:

```cmd
winget install Microsoft.DotNet.SDK.8
```

Attendez que ça se termine. Puis **fermez et rouvrez** votre CMD.

---

## Étape 2: Compiler le Plugin (30 secondes)

Allez dans le bon dossier:

```cmd
cd C:\Users\Dorian\JellyfinTrailerPreview
```

Compilez:

```cmd
dotnet build Jellyfin.Plugin.TrailerPreview.sln --configuration Release
```

---

## Étape 3: Créer le ZIP (30 secondes)

Créez le dossier release:

```cmd
mkdir release
```

Copiez le DLL:

```cmd
copy Jellyfin.Plugin.TrailerPreview\bin\Release\net8.0\Jellyfin.Plugin.TrailerPreview.dll release\
```

Créez le ZIP:

```cmd
powershell -Command "Compress-Archive -Path release\* -DestinationPath jellyfin-plugin-trailerpreview_1.0.0.zip -Force"
```

Calculez le checksum:

```cmd
powershell -Command "$hash = (Get-FileHash jellyfin-plugin-trailerpreview_1.0.0.zip -Algorithm MD5).Hash.ToLower(); Write-Host $hash; $hash | clip"
```

Le checksum est maintenant **dans votre presse-papier** (Ctrl+V pour le coller).

---

## Étape 4: Créer la Release sur GitHub (1 minute)

1. Ouvrez: https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/releases/new

2. Remplissez:
   - **Tag**: `v1.0.0`
   - **Release title**: `Trailer Preview v1.0.0`
   - **Description**: Copiez-collez ceci:
     ```
     ## Features
     - Netflix-style hover trailer previews
     - Multi-platform support (Web, Desktop, TV, Mobile)
     - 40+ configuration options
     - Local and remote trailer support

     ## Installation
     Add this URL in Jellyfin Dashboard → Plugins → Repositories:
     https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json
     ```

3. **Glissez-déposez** le fichier `jellyfin-plugin-trailerpreview_1.0.0.zip`

4. Cliquez **Publish release**

5. **Notez le checksum** que vous avez copié!

---

## Étape 5: Mettre à Jour manifest.json (1 minute)

Ouvrez le fichier `manifest.json` avec Notepad.

Remplacez ces lignes:

**AVANT**:
```json
"sourceUrl": "https://github.com/yourusername/jellyfin-plugin-trailerpreview/releases/download/v1.0.0/jellyfin-plugin-trailerpreview_1.0.0.zip",
"checksum": "00000000000000000000000000000000",
```

**APRÈS** (collez VOTRE checksum):
```json
"sourceUrl": "https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/releases/download/v1.0.0/jellyfin-plugin-trailerpreview_1.0.0.zip",
"checksum": "COLLEZ_VOTRE_CHECKSUM_ICI",
```

Changez aussi:
```json
"owner": "jellyfin",
```

En:
```json
"owner": "Ziifkah",
```

Sauvegardez le fichier.

---

## Étape 6: Pusher (30 secondes)

Dans votre CMD:

```cmd
git add manifest.json
git commit -m "Add v1.0.0 release"
git push origin main
```

---

## ✅ TERMINÉ!

Testez maintenant dans Jellyfin:

1. **Dashboard** → **Plugins** → **Repositories**
2. Cliquez **+**
3. Collez: `https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json`
4. Allez dans **Catalog**
5. Installez **Trailer Preview**

---

## Problème?

Si une étape bloque, dites-moi LAQUELLE et je vous aide!
