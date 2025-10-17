# 🚀 TravelSaaS - Système de Gestion de Réservations de Voyages

## 📋 Vue d'ensemble

**TravelSaaS** est une application SaaS moderne de gestion de réservations de voyages en ligne, conçue avec une architecture multi-niveaux et une interface utilisateur intuitive. Le système supporte une hiérarchie complète d'utilisateurs avec des rôles et permissions bien définis.

## 🏗️ Architecture du Système

### 👥 Hiérarchie des Utilisateurs

```
👑 Super Administrateur
├── 🏢 Administrateur Global d'Agence
│   ├── 📍 Administrateur de Point d'Agence
│   │   └── 🎫 Opérateur Client
│   └── 📍 Administrateur de Point d'Agence
│       └── 🎫 Opérateur Client
└── 🏢 Administrateur Global d'Agence
    └── 📍 Administrateur de Point d'Agence
        └── 🎫 Opérateur Client
```

### 🔐 Rôles et Permissions

| Rôle | Description | Permissions |
|------|-------------|-------------|
| **SuperAdmin** | Gestion globale du système | Création d'agences, gestion de tous les utilisateurs |
| **AgencyGlobalAdmin** | Gestion d'une agence complète | Gestion des points d'agence et utilisateurs de son agence |
| **AgencyPointAdmin** | Gestion d'un point d'agence | Gestion des opérateurs de son point d'agence |
| **AgencyOperator** | Validation des réservations | Validation des billets et réservations clients |

## 🚀 Démarrage Rapide

### Prérequis

- **.NET 8.0** ou supérieur
- **SQL Server** (LocalDB inclus avec Visual Studio)
- **PowerShell** (pour les scripts de démarrage automatique)

### Installation et Démarrage

#### 🎯 Option 1: Démarrage Optimisé (Recommandé)

```powershell
# Script de démarrage optimisé avec gestion automatique des erreurs
.\start-optimized.ps1
```

**Avantages** :
- ✅ Vérification automatique de la configuration
- ✅ Gestion des erreurs de compilation
- ✅ Test automatique de l'API
- ✅ Création automatique du Super Admin si nécessaire
- ✅ Ouverture automatique du navigateur

#### 🔧 Option 2: Script de Test Complet

```powershell
# Test complet du système avec diagnostic détaillé
.\test-complet-superadmin.ps1
```

#### 🚨 Option 3: Diagnostic des Problèmes

```powershell
# Diagnostic automatique en cas de problème
.\diagnose-superadmin.ps1
```

#### 📚 Option 4: Démarrage Manuel

```bash
# Restaurer les packages
dotnet restore

# Compiler le projet
dotnet build

# Mettre à jour la base de données
dotnet ef database update

# Lancer l'application
dotnet run
```

### 🔑 Connexion Super Admin

Une fois l'application démarrée, accédez à `https://localhost:7199/Home/SuperAdminLogin` et connectez-vous avec :

- **Email**: `superadmin@travelsaas.com`
- **Mot de passe**: `Admin@12345`

## 🛠️ Résolution des Problèmes

### 📖 Guides de Résolution

- **[GUIDE_RESOLUTION_RAPIDE.md](GUIDE_RESOLUTION_RAPIDE.md)** - Guide express pour résoudre les problèmes courants
- **[RESOLUTION_ERREUR_CONNEXION.md](RESOLUTION_ERREUR_CONNEXION.md)** - Résolution détaillée des erreurs de connexion
- **[SUPER_ADMIN_SETUP.md](SUPER_ADMIN_SETUP.md)** - Guide complet d'installation et configuration

### 🚨 Erreurs Fréquentes

| Erreur | Solution | Script |
|--------|----------|---------|
| **"Email ou mot de passe incorrect"** | Vérifier la configuration et redémarrer | `.\diagnose-superadmin.ps1` |
| **Erreurs de compilation** | Nettoyer et recompiler | `.\start-optimized.ps1` |
| **Base de données non initialisée** | Recréer la base ou utiliser l'API | `.\test-complet-superadmin.ps1` |

### 🔍 Diagnostic Automatique

Tous nos scripts incluent :
- ✅ Vérification de la configuration
- ✅ Test de compilation
- ✅ Vérification de la base de données
- ✅ Test des API
- ✅ Test de connexion
- ✅ Création automatique du Super Admin si nécessaire

## 📁 Structure du Projet

```
TravelSaaS/
├── 📁 Controllers/           # Contrôleurs API
│   ├── AuthController.cs
│   ├── SuperAdminController.cs
│   ├── GlobalAdminController.cs
│   ├── PointAdminController.cs
│   └── OperatorController.cs
├── 📁 Models/               # Modèles de données
│   ├── 📁 Entities/         # Entités de base de données
│   ├── 📁 DTOs/            # Objets de transfert de données
│   └── 📁 ViewModels/      # Modèles de vue
├── 📁 Services/            # Services métier
│   ├── AuthService.cs
│   └── DataInitializer.cs
├── 📁 Data/               # Contexte de base de données
│   └── ApplicationDbContext.cs
├── 📁 Views/              # Vues Razor
│   ├── 📁 Home/           # Pages d'accueil et connexion
│   └── 📁 *Dashboard/     # Dashboards par rôle
├── 📁 wwwroot/           # Fichiers statiques
│   ├── 📁 css/
│   ├── 📁 js/
│   └── 📁 images/
├── 📁 Migrations/        # Migrations Entity Framework
└── 📁 Scripts/           # Scripts PowerShell
    ├── start-optimized.ps1
    ├── test-complet-superadmin.ps1
    └── diagnose-superadmin.ps1
```

## 🔧 Configuration

### Fichiers de Configuration

- **`appsettings.json`** - Configuration de production
- **`appsettings.Development.json`** - Configuration de développement

### Variables Importantes

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelSaaS;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHereAtLeast32CharactersLong",
    "Issuer": "TravelBookingSaaS",
    "Audience": "TravelBookingSaaSUsers",
    "ExpiryInMinutes": 60
  },
  "AdminSettings": {
    "Email": "superadmin@travelsaas.com",
    "Password": "Admin@12345"
  }
}
```

## 🎯 Fonctionnalités Principales

### 👑 Super Administrateur

- **Dashboard consolidé** avec statistiques globales
- **Gestion des agences** (création, modification, activation/désactivation)
- **Gestion des utilisateurs** (création, attribution de rôles)
- **Gestion des points d'agence** (création, configuration)
- **Vue d'ensemble** de toutes les agences et leurs performances

### 🏢 Administrateur Global d'Agence

- **Dashboard d'agence** avec statistiques spécifiques
- **Gestion des points d'agence** de son agence
- **Gestion des utilisateurs** de son agence
- **Statistiques** de réservations et performances

### 📍 Administrateur de Point d'Agence

- **Dashboard de point d'agence** avec métriques locales
- **Gestion des opérateurs** de son point d'agence
- **Suivi des réservations** de son point
- **Statistiques** de performance locale

### 🎫 Opérateur Client

- **Interface de validation** des réservations
- **Gestion des billets** et confirmations
- **Rapports** quotidiens, hebdomadaires et mensuels
- **Validation** des réservations en ligne

## 🔒 Sécurité

### Authentification

- **JWT (JSON Web Tokens)** pour l'authentification API
- **ASP.NET Core Identity** pour la gestion des utilisateurs
- **Hachage sécurisé** des mots de passe
- **Expiration automatique** des tokens

### Autorisation

- **Policies basées sur les rôles** pour l'accès aux API
- **Claims personnalisés** pour les permissions granulaires
- **Validation côté serveur** de toutes les requêtes
- **Protection CSRF** intégrée

### Validation des Données

- **ModelState validation** sur tous les endpoints
- **Vérification des doublons** (emails, noms d'agence)
- **Contraintes de rôles** respectées
- **Sanitisation** des entrées utilisateur

## 📊 Base de Données

### Entités Principales

- **ApplicationUser** - Utilisateurs du système
- **Agency** - Agences de voyage
- **AgencyPoint** - Points de vente des agences
- **Reservation** - Réservations clients
- **Travel** - Offres de voyage
- **Client** - Clients finaux

### Relations

```
Agency (1) ←→ (N) AgencyPoint
Agency (1) ←→ (N) ApplicationUser
AgencyPoint (1) ←→ (N) ApplicationUser
AgencyPoint (1) ←→ (N) Reservation
ApplicationUser (1) ←→ (N) Reservation (ConfirmedBy)
```

## 🎨 Interface Utilisateur

### Design

- **Interface moderne** et responsive
- **Design system** cohérent
- **Animations fluides** et transitions
- **Accessibilité** conforme aux standards WCAG

### Technologies Frontend

- **HTML5** et **CSS3** modernes
- **JavaScript ES6+** pour l'interactivité
- **Fetch API** pour les appels AJAX
- **localStorage** pour la persistance des tokens

### Composants

- **Cards** pour les statistiques
- **Tables** pour les données
- **Modals** pour les actions
- **Forms** pour la saisie
- **Navigation** intuitive

## 🧪 Tests

### Guide de Test

Consultez le fichier `TEST_SUPER_ADMIN.md` pour un guide complet de test du système.

### Tests Automatisés

```bash
# Lancer les tests unitaires
dotnet test

# Tests d'intégration
dotnet test --filter "Category=Integration"
```

## 📚 Documentation

### Fichiers de Documentation

- **`README.md`** - Ce fichier (vue d'ensemble)
- **`SUPER_ADMIN_SETUP.md`** - Configuration détaillée du Super Admin
- **`TEST_SUPER_ADMIN.md`** - Guide de test complet
- **`start-superadmin.ps1`** - Script de démarrage automatique

### API Documentation

L'API est documentée via **Swagger** accessible à :
`https://localhost:7000/swagger`

## 🚀 Déploiement

### Environnement de Développement

```bash
dotnet run --environment Development
```

### Environnement de Production

```bash
dotnet run --environment Production
```

### Variables d'Environnement

```bash
# Base de données
ConnectionStrings__DefaultConnection="Server=prod-server;Database=TravelSaaS;..."

# JWT
JwtSettings__SecretKey="ProductionSecretKeyVeryLongAndComplex"
JwtSettings__Issuer="TravelBookingSaaS_Prod"

# Super Admin
AdminSettings__Email="admin@yourdomain.com"
AdminSettings__Password="SecurePassword123!"
```

## 🔧 Maintenance

### Logs

Les logs sont configurés pour différents niveaux :
- **Debug** - Informations détaillées pour le développement
- **Information** - Événements généraux
- **Warning** - Avertissements
- **Error** - Erreurs critiques

### Sauvegarde

```sql
-- Sauvegarde de la base de données
BACKUP DATABASE TravelSaaS TO DISK = 'C:\Backups\TravelSaaS.bak'
```

### Monitoring

- **Health checks** intégrés
- **Métriques de performance**
- **Surveillance des erreurs**
- **Alertes automatiques**

## 🤝 Contribution

### Standards de Code

- **C# Coding Conventions** respectées
- **SOLID Principles** appliqués
- **Clean Architecture** mise en œuvre
- **Code reviews** obligatoires

### Workflow de Développement

1. **Fork** du repository
2. **Branch** pour les nouvelles fonctionnalités
3. **Commit** avec messages descriptifs
4. **Pull Request** avec tests
5. **Code Review** et approbation
6. **Merge** dans la branche principale

## 📞 Support

### Problèmes Courants

1. **Erreur de connexion à la base de données**
   - Vérifier la chaîne de connexion
   - S'assurer que SQL Server est démarré

2. **Super Admin non créé**
   - Vérifier la configuration AdminSettings
   - Consulter les logs d'initialisation

3. **Erreur JWT**
   - Vérifier la clé secrète
   - Contrôler l'expiration du token

### Contact

- **Documentation technique** : Consultez les fichiers `.md`
- **Issues** : Utilisez le système de tickets du projet
- **Support** : Contactez l'équipe de développement

---

## 🎉 Conclusion

**TravelSaaS** est un système SaaS complet et professionnel pour la gestion de réservations de voyages. Avec son architecture modulaire, sa sécurité robuste et son interface utilisateur moderne, il répond aux besoins des agences de voyage de toutes tailles.

**🚀 Prêt à transformer votre gestion de réservations !**