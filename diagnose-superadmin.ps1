# 🔍 Script de Diagnostic - Super Admin TravelSaaS
# Auteur: TravelSaaS Team
# Date: 2024

Write-Host "🔍 Diagnostic du système Super Admin TravelSaaS" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

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

# Étape 1: Vérification de la configuration
Write-Info "Vérification de la configuration..."

$appsettingsPath = "appsettings.json"
if (Test-Path $appsettingsPath) {
    Write-Success "Fichier appsettings.json trouvé"
    
    try {
        $config = Get-Content $appsettingsPath | ConvertFrom-Json
        Write-Success "Configuration JSON valide"
        
        if ($config.AdminSettings) {
            Write-Success "Section AdminSettings trouvée"
            Write-Host "   Email: $($config.AdminSettings.Email)" -ForegroundColor White
            Write-Host "   Password: $($config.AdminSettings.Password)" -ForegroundColor White
            Write-Host "   FirstName: $($config.AdminSettings.FirstName)" -ForegroundColor White
            Write-Host "   LastName: $($config.AdminSettings.LastName)" -ForegroundColor White
        } else {
            Write-Error "Section AdminSettings manquante dans appsettings.json"
        }
        
        if ($config.ConnectionStrings.DefaultConnection) {
            Write-Success "ConnectionString trouvé"
            Write-Host "   Base de données: $($config.ConnectionStrings.DefaultConnection)" -ForegroundColor White
        } else {
            Write-Error "ConnectionString manquant dans appsettings.json"
        }
        
        if ($config.JwtSettings) {
            Write-Success "Configuration JWT trouvée"
            Write-Host "   SecretKey: $($config.JwtSettings.SecretKey.Substring(0, [Math]::Min(20, $config.JwtSettings.SecretKey.Length)))..." -ForegroundColor White
        } else {
            Write-Error "Configuration JWT manquante dans appsettings.json"
        }
    } catch {
        Write-Error "Erreur lors de la lecture de la configuration: $($_.Exception.Message)"
    }
} else {
    Write-Error "Fichier appsettings.json non trouvé"
}

# Étape 2: Vérification de la compilation
Write-Info "Vérification de la compilation..."

try {
    $buildResult = dotnet build --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Projet compilé avec succès"
    } else {
        Write-Error "Erreur de compilation"
        Write-Host $buildResult -ForegroundColor Red
    }
} catch {
    Write-Error "Erreur lors de la compilation: $($_.Exception.Message)"
}

# Étape 3: Vérification de la base de données
Write-Info "Vérification de la base de données..."

try {
    $dbResult = dotnet ef database update --dry-run 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Base de données à jour"
    } else {
        Write-Warning "Problème avec la base de données"
        Write-Host $dbResult -ForegroundColor Yellow
    }
} catch {
    Write-Warning "Impossible de vérifier la base de données: $($_.Exception.Message)"
}

# Étape 4: Test de l'API (si l'application est en cours d'exécution)
Write-Info "Test de l'API d'authentification..."

$apiUrl = "https://localhost:7199/api/Auth/check-superadmin"
try {
    $response = Invoke-WebRequest -Uri $apiUrl -UseBasicParsing -TimeoutSec 10 -SkipCertificateCheck
    if ($response.StatusCode -eq 200) {
        Write-Success "API accessible"
        $data = $response.Content | ConvertFrom-Json
        Write-Host "   Super Admins trouvés: $($data.count)" -ForegroundColor White
        
        if ($data.count -gt 0) {
            foreach ($user in $data.users) {
                Write-Host "   - $($user.email) (Rôles: $($user.roles -join ', '))" -ForegroundColor White
            }
        } else {
            Write-Warning "Aucun Super Admin trouvé dans la base de données"
        }
    } else {
        Write-Warning "API accessible mais statut: $($response.StatusCode)"
    }
} catch {
    Write-Warning "API non accessible: $($_.Exception.Message)"
    Write-Info "L'application n'est peut-être pas démarrée"
}

# Étape 5: Recommandations
Write-Host ""
Write-Host "📋 Recommandations:" -ForegroundColor Yellow

if (Test-Path $appsettingsPath) {
    $config = Get-Content $appsettingsPath | ConvertFrom-Json
    
    if (-not $config.AdminSettings) {
        Write-Host "1. ❌ Ajouter la section AdminSettings dans appsettings.json" -ForegroundColor Red
    }
    
    if (-not $config.ConnectionStrings.DefaultConnection) {
        Write-Host "2. ❌ Vérifier la ConnectionString dans appsettings.json" -ForegroundColor Red
    }
    
    if (-not $config.JwtSettings) {
        Write-Host "3. ❌ Ajouter la configuration JWT dans appsettings.json" -ForegroundColor Red
    }
}

Write-Host "4. 🔄 Redémarrer l'application après les corrections" -ForegroundColor Yellow
Write-Host "5. 📊 Vérifier les logs de l'application pour plus de détails" -ForegroundColor Yellow

# Étape 6: Test de connexion manuel
Write-Host ""
Write-Info "Test de connexion manuel..."

$loginUrl = "https://localhost:7199/api/Auth/login"
$loginData = @{
    email = "superadmin@travelsaas.com"
    password = "Admin@12345"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri $loginUrl -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing -TimeoutSec 10 -SkipCertificateCheck
    if ($loginResponse.StatusCode -eq 200) {
        Write-Success "Connexion réussie !"
        $loginResult = $loginResponse.Content | ConvertFrom-Json
        Write-Host "   Token reçu: $($loginResult.Token.Substring(0, [Math]::Min(50, $loginResult.Token.Length)))..." -ForegroundColor White
    } else {
        Write-Warning "Connexion échouée avec le statut: $($loginResponse.StatusCode)"
        Write-Host "   Réponse: $($loginResponse.Content)" -ForegroundColor Yellow
    }
} catch {
    Write-Error "Erreur lors du test de connexion: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "🎯 Diagnostic terminé !" -ForegroundColor Green
Write-Host "Consultez les recommandations ci-dessus pour résoudre les problèmes." -ForegroundColor White
