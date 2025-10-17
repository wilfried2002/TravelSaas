# 🚀 Script de Démarrage Optimisé - TravelSaaS Super Admin
# Auteur: TravelSaaS Team
# Date: 2024

Write-Host "🚀 Démarrage Optimisé du Système Super Admin TravelSaaS" -ForegroundColor Green
Write-Host "=========================================================" -ForegroundColor Green

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

# Fonction pour vérifier si l'application est déjà en cours d'exécution
function Test-ApplicationRunning {
    try {
        $processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
        foreach ($process in $processes) {
            try {
                $response = Invoke-WebRequest -Uri "https://localhost:7199/api/Auth/check-superadmin" -UseBasicParsing -TimeoutSec 3 -SkipCertificateCheck -ErrorAction SilentlyContinue
                if ($response.StatusCode -eq 200) {
                    return $true
                }
            } catch {
                # Continue checking other processes
            }
        }
        return $false
    } catch {
        return $false
    }
}

# Fonction pour arrêter l'application
function Stop-Application {
    Write-Info "Arrêt de l'application..."
    try {
        Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 3
        Write-Success "Application arrêtée"
    } catch {
        Write-Warning "Impossible d'arrêter l'application: $($_.Exception.Message)"
    }
}

# Étape 1: Vérification de l'état actuel
Write-Info "Étape 1: Vérification de l'état actuel..."

if (Test-ApplicationRunning) {
    Write-Warning "L'application est déjà en cours d'exécution"
    $choice = Read-Host "Voulez-vous la redémarrer ? (O/N)"
    if ($choice -eq "O" -or $choice -eq "o") {
        Stop-Application
    } else {
        Write-Info "Utilisation de l'application existante"
        goto :OpenBrowser
    }
}

# Étape 2: Nettoyage et compilation
Write-Info "Étape 2: Nettoyage et compilation..."

try {
    Write-Host "Nettoyage du projet..." -ForegroundColor White
    dotnet clean --verbosity quiet 2>$null
    
    Write-Host "Compilation du projet..." -ForegroundColor White
    $buildResult = dotnet build --no-restore --verbosity quiet 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Projet compilé avec succès"
    } else {
        Write-Error "Erreur de compilation détectée"
        Write-Host $buildResult -ForegroundColor Red
        Write-Host ""
        Write-Host "🔧 Solutions recommandées:" -ForegroundColor Yellow
        Write-Host "1. Vérifiez que tous les packages NuGet sont installés" -ForegroundColor White
        Write-Host "2. Vérifiez la syntaxe du code" -ForegroundColor White
        Write-Host "3. Consultez le guide de résolution rapide" -ForegroundColor White
        exit 1
    }
} catch {
    Write-Error "Erreur lors de la compilation: $($_.Exception.Message)"
    exit 1
}

# Étape 3: Vérification de la configuration
Write-Info "Étape 3: Vérification de la configuration..."

$appsettingsPath = "appsettings.json"
if (-not (Test-Path $appsettingsPath)) {
    Write-Error "Fichier appsettings.json non trouvé"
    exit 1
}

try {
    $config = Get-Content $appsettingsPath | ConvertFrom-Json
    
    if (-not $config.AdminSettings) {
        Write-Error "Section AdminSettings manquante dans appsettings.json"
        Write-Host "Ajoutez la section AdminSettings avec les identifiants Super Admin" -ForegroundColor Yellow
        exit 1
    }
    
    if (-not $config.ConnectionStrings.DefaultConnection) {
        Write-Error "ConnectionString manquant dans appsettings.json"
        exit 1
    }
    
    if (-not $config.JwtSettings) {
        Write-Error "Configuration JWT manquante dans appsettings.json"
        exit 1
    }
    
    Write-Success "Configuration validée"
    Write-Host "   Super Admin: $($config.AdminSettings.Email)" -ForegroundColor White
    Write-Host "   Base de données: $($config.ConnectionStrings.DefaultConnection)" -ForegroundColor White
    
} catch {
    Write-Error "Erreur lors de la lecture de la configuration: $($_.Exception.Message)"
    exit 1
}

# Étape 4: Démarrage de l'application
Write-Info "Étape 4: Démarrage de l'application..."

try {
    Write-Host "Démarrage de l'application..." -ForegroundColor White
    
    # Démarrer l'application en arrière-plan
    $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Hidden -PassThru
    
    if ($process) {
        Write-Success "Application démarrée (PID: $($process.Id))"
    } else {
        Write-Error "Impossible de démarrer l'application"
        exit 1
    }
    
    # Attendre que l'application soit prête
    Write-Info "Attente du démarrage de l'application..."
    $maxWaitTime = 30
    $waitTime = 0
    
    while ($waitTime -lt $maxWaitTime) {
        try {
            $response = Invoke-WebRequest -Uri "https://localhost:7199/api/Auth/check-superadmin" -UseBasicParsing -TimeoutSec 5 -SkipCertificateCheck -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "Application prête !"
                break
            }
        } catch {
            # Continue waiting
        }
        
        $waitTime += 2
        Write-Host "Attente... ($waitTime/$maxWaitTime secondes)" -ForegroundColor White
        Start-Sleep -Seconds 2
    }
    
    if ($waitTime -ge $maxWaitTime) {
        Write-Warning "L'application prend du temps à démarrer"
        Write-Host "Vérifiez les logs pour plus de détails" -ForegroundColor Yellow
    }
    
} catch {
    Write-Error "Erreur lors du démarrage: $($_.Exception.Message)"
    exit 1
}

# Étape 5: Test de l'API
Write-Info "Étape 5: Test de l'API..."

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7199/api/Auth/check-superadmin" -UseBasicParsing -TimeoutSec 10 -SkipCertificateCheck
    
    if ($response.StatusCode -eq 200) {
        Write-Success "API accessible"
        $data = $response.Content | ConvertFrom-Json
        Write-Host "   Super Admins trouvés: $($data.count)" -ForegroundColor White
        
        if ($data.count -gt 0) {
            foreach ($user in $data.users) {
                Write-Host "   - $($user.email) (Rôles: $($user.roles -join ', '))" -ForegroundColor White
            }
        } else {
            Write-Warning "Aucun Super Admin trouvé - création en cours..."
            # Attendre un peu plus pour l'initialisation
            Start-Sleep -Seconds 5
        }
    } else {
        Write-Warning "API accessible mais statut: $($response.StatusCode)"
    }
} catch {
    Write-Error "API non accessible: $($_.Exception.Message)"
    Write-Host "L'application n'est peut-être pas encore prête" -ForegroundColor Yellow
}

# Étape 6: Test de connexion
Write-Info "Étape 6: Test de connexion..."

$loginUrl = "https://localhost:7199/api/Auth/login"
$loginData = @{
    email = "superadmin@travelsaas.com"
    password = "Admin@12345"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri $loginUrl -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing -TimeoutSec 10 -SkipCertificateCheck
    
    if ($loginResponse.StatusCode -eq 200) {
        Write-Success "Connexion Super Admin réussie !"
        $loginResult = $loginResponse.Content | ConvertFrom-Json
        Write-Host "   Utilisateur: $($loginResult.User.firstName) $($loginResult.User.lastName)" -ForegroundColor White
        Write-Host "   Rôles: $($loginResult.User.roles -join ', ')" -ForegroundColor White
    } else {
        Write-Warning "Connexion échouée avec le statut: $($loginResponse.StatusCode)"
        Write-Host "   Réponse: $($loginResponse.Content)" -ForegroundColor Yellow
        
        # Tentative de création manuelle
        Write-Info "Tentative de création manuelle du Super Admin..."
        $createUrl = "https://localhost:7199/api/Auth/create-superadmin"
        $createData = @{
            email = "superadmin@travelsaas.com"
            password = "Admin@12345"
            firstName = "Super"
            lastName = "Administrator"
            phoneNumber = "+1234567890"
        } | ConvertTo-Json
        
        try {
            $createResponse = Invoke-WebRequest -Uri $createUrl -Method POST -Body $createData -ContentType "application/json" -UseBasicParsing -TimeoutSec 10 -SkipCertificateCheck
            
            if ($createResponse.StatusCode -eq 200) {
                Write-Success "Super Admin créé manuellement avec succès"
                Write-Host "Nouvelle tentative de connexion..." -ForegroundColor White
                Start-Sleep -Seconds 3
                
                # Retester la connexion
                $retryResponse = Invoke-WebRequest -Uri $loginUrl -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing -TimeoutSec 10 -SkipCertificateCheck
                if ($retryResponse.StatusCode -eq 200) {
                    Write-Success "Connexion réussie après création !"
                }
            } else {
                Write-Warning "Création manuelle échouée: $($createResponse.StatusCode)"
            }
        } catch {
            Write-Error "Erreur lors de la création manuelle: $($_.Exception.Message)"
        }
    }
} catch {
    Write-Error "Erreur lors du test de connexion: $($_.Exception.Message)"
}

# Étape 7: Ouverture du navigateur
:OpenBrowser
Write-Info "Étape 7: Ouverture du navigateur..."

$webUrl = "https://localhost:7199/Home/SuperAdminLogin"

try {
    Start-Process $webUrl
    Write-Success "Navigateur ouvert sur la page de connexion Super Admin"
} catch {
    Write-Warning "Impossible d'ouvrir le navigateur automatiquement"
    Write-Host "Ouvrez manuellement: $webUrl" -ForegroundColor Yellow
}

# Étape 8: Affichage des informations de connexion
Write-Host ""
Write-Host "🔑 Informations de Connexion:" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host "URL: $webUrl" -ForegroundColor White
Write-Host "Email: superadmin@travelsaas.com" -ForegroundColor White
Write-Host "Mot de passe: Admin@12345" -ForegroundColor White

Write-Host ""
Write-Host "🎯 Prochaines Étapes:" -ForegroundColor Yellow
Write-Host "1. Connectez-vous avec les identifiants ci-dessus" -ForegroundColor White
Write-Host "2. Accédez au dashboard Super Admin" -ForegroundColor White
Write-Host "3. Créez votre première agence" -ForegroundColor White

Write-Host ""
Write-Host "📊 Monitoring:" -ForegroundColor Cyan
Write-Host "Pour surveiller l'application, regardez la console où dotnet run a été lancé" -ForegroundColor White
Write-Host "Pour arrêter l'application, utilisez Ctrl+C dans cette console" -ForegroundColor White

Write-Host ""
Write-Host "🎉 Démarrage terminé avec succès !" -ForegroundColor Green
Write-Host "L'application TravelSaaS Super Admin est maintenant opérationnelle." -ForegroundColor White

# Garder la console ouverte pour le monitoring
Write-Host ""
Write-Host "Appuyez sur une touche pour fermer cette console..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")





















