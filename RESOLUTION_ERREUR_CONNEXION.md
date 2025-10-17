# 🚨 Résolution Rapide - Erreur de Connexion Super Admin

## ❌ Problème Identifié

**Erreur** : "Email ou mot de passe incorrect" lors de la connexion Super Admin

**URL** : `https://localhost:7199/Home/SuperAdminLogin`

## 🔍 Diagnostic Rapide

### 1. Vérifier la Configuration

Ouvrez `appsettings.json` et vérifiez que la section `AdminSettings` est présente :

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

### 2. Vérifier les Logs de l'Application

Lancez l'application et regardez la console pour voir les messages d'initialisation :

```bash
dotnet run
```

**Messages attendus** :
```
✅ Super Admin créé avec succès: superadmin@travelsaas.com
✅ Rôle SuperAdmin attribué avec succès
```

### 3. Test de l'API

Testez l'endpoint de vérification des Super Admins :

```bash
curl -X GET "https://localhost:7199/api/Auth/check-superadmin" -k
```

## 🛠️ Solutions

### Solution 1: Recréer le Super Admin

Si le Super Admin n'existe pas, utilisez l'endpoint de création :

```bash
curl -X POST "https://localhost:7199/api/Auth/create-superadmin" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "superadmin@travelsaas.com",
    "password": "Admin@12345",
    "firstName": "Super",
    "lastName": "Administrator",
    "phoneNumber": "+1234567890"
  }' -k
```

### Solution 2: Vérifier la Base de Données

```bash
# Mettre à jour la base de données
dotnet ef database update

# Ou recréer complètement
dotnet ef database drop --force
dotnet ef database update
```

### Solution 3: Script de Diagnostic

Exécutez le script de diagnostic :

```powershell
.\diagnose-superadmin.ps1
```

## 🔧 Vérifications Techniques

### 1. Vérifier l'Initialisation

Dans `Program.cs`, assurez-vous que le DataInitializer est appelé :

```csharp
// Initialize database and roles
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DataInitializer>();
    await initializer.InitializeAsync();
}
```

### 2. Vérifier les Rôles

Assurez-vous que le rôle "SuperAdmin" existe dans la base de données.

### 3. Vérifier les Permissions

Vérifiez que l'utilisateur a bien le rôle "SuperAdmin" attribué.

## 📋 Checklist de Résolution

- [ ] **Configuration** : `AdminSettings` présent dans `appsettings.json`
- [ ] **Base de données** : Migrations à jour
- [ ] **Rôles** : Rôle "SuperAdmin" créé
- [ ] **Utilisateur** : Super Admin créé avec le bon rôle
- [ ] **Logs** : Messages de succès dans la console
- [ ] **API** : Endpoint `/api/Auth/check-superadmin` accessible

## 🚀 Redémarrage

Après chaque correction :

1. **Arrêter** l'application (Ctrl+C)
2. **Recompiler** : `dotnet build`
3. **Relancer** : `dotnet run`
4. **Vérifier** les logs d'initialisation
5. **Tester** la connexion

## 🔍 Debug Avancé

### Vérifier l'État de la Base de Données

```sql
-- Vérifier les utilisateurs
SELECT Id, UserName, Email, IsSuperAdmin, IsActive 
FROM AspNetUsers 
WHERE IsSuperAdmin = 1;

-- Vérifier les rôles
SELECT * FROM AspNetRoles;

-- Vérifier les attributions de rôles
SELECT u.Email, r.Name as Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.IsSuperAdmin = 1;
```

### Logs Détaillés

Activez les logs détaillés dans `appsettings.Development.json` :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "TravelSaaS": "Debug"
    }
  }
}
```

## 📞 Support

Si le problème persiste :

1. **Exécuter** le script de diagnostic
2. **Vérifier** tous les points de la checklist
3. **Consulter** les logs d'erreur
4. **Tester** avec l'endpoint de création manuel

---

## 🎯 Résultat Attendu

Après résolution, vous devriez pouvoir vous connecter avec :
- **Email** : `superadmin@travelsaas.com`
- **Mot de passe** : `Admin@12345`

Et accéder au dashboard Super Admin sans erreur.
