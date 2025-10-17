# 🔧 Script de Compilation et Test - TravelSaaS
# Auteur: TravelSaaS Team
# Date: 2024

Write-Host "🔧 Compilation et Test du Projet TravelSaaS" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Configuration des couleurs
$SuccessColor = "Green"
$ErrorColor = "Red"
$WarningColor = "Yellow"
$InfoColor = "Cyan"

# Fonction pour afficher les messages
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor $SuccessColor }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor $ErrorColor }
function Write-Warning { param($Message) Write-Host "⚠️ $Message" -ForegroundColor $WarningColor }
function Write-Info { param($Message) Write-Host "ℹ️ $Message" -ForegroundColor $InfoColor }

# Étape 1: Nettoyage du projet
Write-Info "Étape 1: Nettoyage du projet..."
try {
    dotnet clean --verbosity quiet 2>$null
    Write-Success "Projet nettoyé"
} catch {
    Write-Warning "Erreur lors du nettoyage: $($_.Exception.Message)"
}

# Étape 2: Restauration des packages
Write-Info "Étape 2: Restauration des packages NuGet..."
try {
    $restoreResult = dotnet restore --verbosity quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Packages NuGet restaurés"
    } else {
        Write-Warning "Problème lors de la restauration des packages"
        Write-Host $restoreResult -ForegroundColor Yellow
    }
} catch {
    Write-Warning "Erreur lors de la restauration: $($_.Exception.Message)"
}

# Étape 3: Compilation du projet
Write-Info "Étape 3: Compilation du projet..."
try {
    Write-Host "Compilation en cours..." -ForegroundColor White
    $buildResult = dotnet build --no-restore --verbosity normal 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "✅ Projet compilé avec succès !"
        
        # Afficher le résumé de la compilation
        $buildOutput = $buildResult -join "`n"
        if ($buildOutput -match "(\d+) Warning\(s\)") {
            $warnings = $matches[1]
            if ([int]$warnings -gt 0) {
                Write-Warning "$warnings avertissement(s) détecté(s)"
            }
        }
        
        if ($buildOutput -match "(\d+) Error\(s\)") {
            $errors = $matches[1]
            if ([int]$errors -gt 0) {
                Write-Error "$errors erreur(s) détectée(s)"
                Write-Host "Détails des erreurs:" -ForegroundColor Red
                Write-Host $buildOutput -ForegroundColor Red
                exit 1
            }
        }
        
        Write-Success "Aucune erreur de compilation détectée"
        
    } else {
        Write-Error "❌ Erreur de compilation détectée"
        Write-Host "Détails de l'erreur:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        
        # Analyse des erreurs courantes
        Write-Host ""
        Write-Host "🔍 Analyse des erreurs courantes:" -ForegroundColor Yellow
        
        if ($buildResult -match "UpdateAgencyDto") {
            Write-Host "❌ Problème: UpdateAgencyDto introuvable" -ForegroundColor Red
            Write-Host "💡 Solution: Vérifiez que le DTO est défini dans AdminDtos.cs" -ForegroundColor Yellow
        }
        
        if ($buildResult -match "Encoding\.GetBytes") {
            Write-Host "❌ Problème: Paramètre null dans Encoding.GetBytes" -ForegroundColor Red
            Write-Host "💡 Solution: Vérifiez la configuration JWT dans appsettings.json" -ForegroundColor Yellow
        }
        
        if ($buildResult -match "Runtime") {
            Write-Host "❌ Problème: Modifications pendant l'exécution" -ForegroundColor Red
            Write-Host "💡 Solution: Arrêtez l'application avant de recompiler" -ForegroundColor Yellow
        }
        
        exit 1
    }
} catch {
    Write-Error "Erreur lors de la compilation: $($_.Exception.Message)"
    exit 1
}

# Étape 4: Vérification des DTOs
Write-Info "Étape 4: Vérification des DTOs..."
$dtoFile = "Models/DTOs/AdminDtos.cs"
if (Test-Path $dtoFile) {
    $dtoContent = Get-Content $dtoFile -Raw
    
    $requiredDtos = @(
        "UpdateAgencyDto",
        "UpdateAgencyPointDto", 
        "ToggleStatusDto",
        "CreateAgencyDto",
        "CreateAgencyPointDto",
        "CreateUserDto",
        "UpdateUserDto"
    )
    
    $missingDtos = @()
    foreach ($dto in $requiredDtos) {
        if ($dtoContent -notmatch "class $dto") {
            $missingDtos += $dto
        }
    }
    
    if ($missingDtos.Count -eq 0) {
        Write-Success "Tous les DTOs requis sont présents"
    } else {
        Write-Warning "DTOs manquants détectés:"
        foreach ($dto in $missingDtos) {
            Write-Host "   - $dto" -ForegroundColor Yellow
        }
    }
} else {
    Write-Error "Fichier AdminDtos.cs non trouvé"
}

# Étape 5: Vérification de la configuration
Write-Info "Étape 5: Vérification de la configuration..."
$appsettingsPath = "appsettings.json"
if (Test-Path $appsettingsPath) {
    try {
        $config = Get-Content $appsettingsPath | ConvertFrom-Json
        
        $configOk = $true
        
        if (-not $config.AdminSettings) {
            Write-Warning "Section AdminSettings manquante"
            $configOk = $false
        }
        
        if (-not $config.ConnectionStrings.DefaultConnection) {
            Write-Warning "ConnectionString manquant"
            $configOk = $false
        }
        
        if (-not $config.JwtSettings) {
            Write-Warning "Configuration JWT manquante"
            $configOk = $false
        }
        
        if ($configOk) {
            Write-Success "Configuration validée"
        } else {
            Write-Warning "Problèmes de configuration détectés"
        }
        
    } catch {
        Write-Error "Erreur lors de la lecture de la configuration: $($_.Exception.Message)"
    }
} else {
    Write-Error "Fichier appsettings.json non trouvé"
}

# Étape 6: Test de démarrage rapide
Write-Info "Étape 6: Test de démarrage rapide..."
$choice = Read-Host "Voulez-vous tester le démarrage de l'application ? (O/N)"
if ($choice -eq "O" -or $choice -eq "o") {
    Write-Info "Démarrage de l'application..."
    try {
        Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Normal
        Write-Success "Application démarrée"
        Write-Host "Attendez quelques secondes puis testez:" -ForegroundColor White
        Write-Host "   - API: https://localhost:7199/api/Auth/check-superadmin" -ForegroundColor White
        Write-Host "   - Interface: https://localhost:7199/Home/SuperAdminLogin" -ForegroundColor White
    } catch {
        Write-Error "Erreur lors du démarrage: $($_.Exception.Message)"
    }
}

# Résumé final
Write-Host ""
Write-Host "📋 Résumé de la Compilation:" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan

if ($LASTEXITCODE -eq 0) {
    Write-Success "✅ Compilation réussie !"
    Write-Host "Le projet est prêt pour le développement" -ForegroundColor White
} else {
    Write-Error "❌ Compilation échouée"
    Write-Host "Corrigez les erreurs avant de continuer" -ForegroundColor White
}

Write-Host ""
Write-Host "🎯 Prochaines étapes recommandées:" -ForegroundColor Yellow
Write-Host "1. Si compilation OK: Utilisez .\start-optimized.ps1" -ForegroundColor White
Write-Host "2. Si erreurs: Corrigez les problèmes et relancez ce script" -ForegroundColor White
Write-Host "3. Pour diagnostic: Utilisez .\diagnose-superadmin.ps1" -ForegroundColor White

Write-Host ""
Write-Host "🎉 Script terminé !" -ForegroundColor Green





















