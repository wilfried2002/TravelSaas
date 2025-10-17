# 🚀 Script de Démarrage et Test - Super Admin TravelSaaS
# Auteur: TravelSaaS Team
# Date: 2024

Write-Host "🎯 Démarrage du système Super Admin TravelSaaS" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

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

# Étape 1: Vérification de l'environnement
Write-Info "Vérification de l'environnement de développement..."

# Vérifier si .NET est installé
try {
    $dotnetVersion = dotnet --version
    Write-Success ".NET version: $dotnetVersion"
} catch {
    Write-Error ".NET n'est pas installé ou n'est pas dans le PATH"
    exit 1
}

# Vérifier si Entity Framework est installé
try {
    $efVersion = dotnet ef --version
    Write-Success "Entity Framework version: $efVersion"
} catch {
    Write-Warning "Entity Framework CLI n'est pas installé globalement"
    Write-Info "Installation d'Entity Framework CLI..."
    dotnet tool install --global dotnet-ef
}

# Étape 2: Nettoyage et compilation
Write-Info "Nettoyage et compilation du projet..."

# Nettoyer les builds précédents
dotnet clean
if ($LASTEXITCODE -eq 0) {
    Write-Success "Nettoyage terminé"
} else {
    Write-Error "Erreur lors du nettoyage"
    exit 1
}

# Restaurer les packages
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Success "Restauration des packages terminée"
} else {
    Write-Error "Erreur lors de la restauration des packages"
    exit 1
}

# Compiler le projet
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Success "Compilation réussie"
} else {
    Write-Error "Erreur de compilation"
    exit 1
}

# Étape 3: Configuration de la base de données
Write-Info "Configuration de la base de données..."

# Créer la base de données si elle n'existe pas
dotnet ef database update
if ($LASTEXITCODE -eq 0) {
    Write-Success "Base de données mise à jour"
} else {
    Write-Warning "Erreur lors de la mise à jour de la base de données"
    Write-Info "Tentative de création de la base de données..."
    dotnet ef database drop --force
    dotnet ef database update
}

# Étape 4: Démarrage de l'application
Write-Info "Démarrage de l'application TravelSaaS..."

# Lancer l'application en arrière-plan
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -WindowStyle Hidden

# Attendre que l'application démarre
Write-Info "Attente du démarrage de l'application..."
Start-Sleep -Seconds 10

# Étape 5: Test de l'application
Write-Info "Test de l'application..."

# Vérifier si l'application répond
try {
    $response = Invoke-WebRequest -Uri "https://localhost:7000" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Success "Application accessible sur https://localhost:7000"
    } else {
        Write-Warning "Application accessible mais statut: $($response.StatusCode)"
    }
} catch {
    Write-Warning "Application pas encore prête, nouvelle tentative dans 5 secondes..."
    Start-Sleep -Seconds 5
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:7000" -UseBasicParsing -TimeoutSec 10
        Write-Success "Application maintenant accessible"
    } catch {
        Write-Error "Impossible d'accéder à l'application"
    }
}

# Étape 6: Affichage des informations de connexion
Write-Host ""
Write-Host "🎉 Configuration Super Admin Terminée !" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Informations de Connexion:" -ForegroundColor White
Write-Host "   URL: https://localhost:7000" -ForegroundColor Cyan
Write-Host "   Email: superadmin@travelsaas.com" -ForegroundColor Cyan
Write-Host "   Mot de passe: Admin@12345" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔧 Pour l'environnement de développement:" -ForegroundColor White
Write-Host "   Email: dev.superadmin@travelsaas.com" -ForegroundColor Cyan
Write-Host "   Mot de passe: DevAdmin@2024!" -ForegroundColor Cyan
Write-Host ""
Write-Host "📚 Documentation:" -ForegroundColor White
Write-Host "   - SUPER_ADMIN_SETUP.md" -ForegroundColor Cyan
Write-Host "   - TEST_SUPER_ADMIN.md" -ForegroundColor Cyan
Write-Host ""
Write-Host "🛑 Pour arrêter l'application: Ctrl+C" -ForegroundColor Yellow
Write-Host ""

# Étape 7: Ouverture automatique du navigateur
Write-Info "Ouverture du navigateur..."
Start-Process "https://localhost:7000"

# Étape 8: Surveillance de l'application
Write-Info "Surveillance de l'application en cours..."
Write-Host "Appuyez sur 'Q' pour quitter, 'R' pour redémarrer" -ForegroundColor Yellow

while ($true) {
    if ([Console]::KeyAvailable) {
        $key = [Console]::ReadKey($true)
        if ($key.Key -eq 'Q') {
            Write-Info "Arrêt de l'application..."
            Stop-Process -Id $process.Id -Force
            Write-Success "Application arrêtée"
            break
        }
        elseif ($key.Key -eq 'R') {
            Write-Info "Redémarrage de l'application..."
            Stop-Process -Id $process.Id -Force
            Start-Sleep -Seconds 2
            $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -WindowStyle Hidden
            Write-Success "Application redémarrée"
        }
    }
    
    # Vérifier si l'application fonctionne toujours
    if ($process.HasExited) {
        Write-Error "L'application s'est arrêtée inopinément"
        break
    }
    
    Start-Sleep -Seconds 1
}

Write-Host ""
Write-Host "👋 Merci d'avoir utilisé TravelSaaS !" -ForegroundColor Green
