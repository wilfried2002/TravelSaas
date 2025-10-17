# 🏗️ Configuration du Super Administrateur - TravelSaaS

## 📋 Vue d'ensemble

Le **Super Administrateur** est le niveau d'accès le plus élevé dans le système TravelSaaS. Il a la responsabilité de gérer l'ensemble de la plateforme, y compris la création et la gestion des agences, des points d'agence et de tous les utilisateurs.

## 🔐 Informations de Connexion par Défaut

### Configuration dans `appsettings.json`

```json
{
  "AdminSettings": {
    "Email": "superadmin@travelsaas.com",
    "Password": "Admin@12345",
    "FirstName": "Super",
    "LastName": "Administrator",
    "PhoneNumber": "+1234567890"
  }
}
```

### 🔑 Identifiants de Connexion

- **Email**: `superadmin@travelsaas.com`
- **Mot de passe**: `Admin@12345`

> ⚠️ **IMPORTANT**: Changez ces identifiants par défaut en production !

## 🚀 Première Connexion

### 1. Démarrage de l'Application

```bash
dotnet run
```

### 2. Accès au Dashboard

1. Ouvrez votre navigateur
2. Accédez à `https://localhost:7000`
3. Cliquez sur **"👑 Super Administrateur"**
4. Connectez-vous avec les identifiants ci-dessus

### 3. Initialisation Automatique

Lors du premier démarrage, le système :
- ✅ Crée automatiquement le Super Admin
- ✅ Initialise tous les rôles nécessaires
- ✅ Configure la base de données

## 🎯 Fonctionnalités du Super Admin

### 📊 Dashboard Principal

Le Super Admin a accès à un dashboard consolidé avec :

- **Statistiques Globales** :
  - Nombre total d'agences
  - Nombre total d'utilisateurs
  - Nombre total de réservations
  - Utilisateurs actifs

- **Vue d'ensemble des Agences** :
  - Liste de toutes les agences
  - Statut actif/inactif
  - Nombre de points d'agence
  - Nombre d'utilisateurs

### 🏢 Gestion des Agences

#### Création d'une Agence

```json
POST /api/SuperAdmin/agencies
{
  "name": "Agence de Voyages Premium",
  "address": "123 Rue de la Paix, Paris",
  "phone": "+33123456789",
  "email": "contact@agence-premium.com"
}
```

#### Actions Disponibles

- ✅ **Créer une nouvelle agence**
- ✅ **Modifier les informations d'une agence**
- ✅ **Activer/Désactiver une agence**
- ✅ **Voir toutes les agences avec statistiques**

### 📍 Gestion des Points d'Agence

#### Création d'un Point d'Agence

```json
POST /api/SuperAdmin/agency-points
{
  "agencyId": "guid-de-l-agence",
  "name": "Point de Vente Centre-Ville",
  "address": "456 Avenue des Champs, Paris",
  "phone": "+33123456790",
  "email": "centreville@agence-premium.com"
}
```

### 👥 Gestion des Utilisateurs

#### Hiérarchie des Rôles

1. **SuperAdmin** (vous) - Gestion globale
2. **AgencyGlobalAdmin** - Gestion d'une agence complète
3. **AgencyPointAdmin** - Gestion d'un point d'agence
4. **AgencyOperator** - Validation des réservations

#### Création d'un Utilisateur

```json
POST /api/SuperAdmin/users
{
  "email": "admin@agence-premium.com",
  "password": "MotDePasseSecurise123!",
  "firstName": "Jean",
  "lastName": "Dupont",
  "phoneNumber": "+33123456791",
  "role": "AgencyGlobalAdmin",
  "agencyId": "guid-de-l-agence",
  "agencyPointId": null
}
```

#### Règles de Création

| Rôle | AgencyId Requis | AgencyPointId Requis |
|------|----------------|---------------------|
| SuperAdmin | ❌ | ❌ |
| AgencyGlobalAdmin | ✅ | ❌ |
| AgencyPointAdmin | ✅ | ✅ |
| AgencyOperator | ✅ | ✅ |

## 🔧 Configuration Avancée

### Sécurité

#### Changer le Mot de Passe du Super Admin

1. **Via l'interface web** (recommandé)
2. **Via la base de données** (en cas d'urgence)

```sql
-- Changer le mot de passe (hashé)
UPDATE AspNetUsers 
SET PasswordHash = 'nouveau_hash_ici'
WHERE Email = 'superadmin@travelsaas.com';
```

#### Configuration JWT

```json
{
  "JwtSettings": {
    "SecretKey": "VotreCléSecrèteTrèsLongueEtComplexe",
    "Issuer": "TravelBookingSaaS",
    "Audience": "TravelBookingSaaSUsers",
    "ExpiryInMinutes": 60
  }
}
```

### Base de Données

#### Connexion

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TravelSaaS;Trusted_Connection=true;"
  }
}
```

## 📱 Interface Utilisateur

### Navigation

1. **Page d'accueil** → Sélection du type d'utilisateur
2. **Connexion Super Admin** → Formulaire sécurisé
3. **Dashboard** → Vue consolidée avec toutes les fonctionnalités

### Fonctionnalités UI

- 🎨 **Design moderne et responsive**
- 🔄 **Rafraîchissement automatique des données**
- 📊 **Statistiques en temps réel**
- ⚡ **Actions instantanées**
- 🔒 **Sécurité intégrée**

## 🛡️ Bonnes Pratiques de Sécurité

### 1. Changer les Identifiants par Défaut

```json
{
  "AdminSettings": {
    "Email": "votre-email-securise@domaine.com",
    "Password": "MotDePasseComplexe123!@#",
    "FirstName": "Votre",
    "LastName": "Nom",
    "PhoneNumber": "+33XXXXXXXXX"
  }
}
```

### 2. Utiliser HTTPS en Production

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:7000"
      }
    }
  }
}
```

### 3. Configurer les Logs

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "TravelSaaS": "Debug"
    }
  }
}
```

## 🔍 Dépannage

### Problèmes Courants

#### 1. Super Admin non créé

```bash
# Vérifier les logs
dotnet run --verbosity detailed

# Vérifier la configuration
cat appsettings.json
```

#### 2. Erreur de connexion

- Vérifier que la base de données est créée
- Vérifier les identifiants dans `appsettings.json`
- Vérifier que le service DataInitializer s'est exécuté

#### 3. Permissions insuffisantes

```bash
# Vérifier les rôles
dotnet ef database update
```

## 📞 Support

En cas de problème :

1. **Vérifiez les logs** de l'application
2. **Consultez la documentation** technique
3. **Contactez l'équipe de développement**

---

## 🎯 Workflow Type du Super Admin

### 1. Première Connexion
```
Connexion → Dashboard → Vérification des statistiques
```

### 2. Création d'une Agence
```
Dashboard → Créer Agence → Remplir formulaire → Validation
```

### 3. Création d'un Admin Global
```
Dashboard → Créer Utilisateur → Rôle AgencyGlobalAdmin → Associer à l'agence
```

### 4. Création de Points d'Agence
```
Dashboard → Créer Point d'Agence → Associer à l'agence → Validation
```

### 5. Création d'Opérateurs
```
Dashboard → Créer Utilisateur → Rôle AgencyOperator → Associer au point d'agence
```

---

**🎉 Votre système TravelSaaS est maintenant prêt à être utilisé !**
