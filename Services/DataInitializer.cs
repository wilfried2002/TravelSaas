using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TravelSaaS.Data;
using TravelSaaS.Models.Entities;
using TravelSaaS.Models.DTOs;

namespace TravelSaaS.Services
{
    public class DataInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public DataInitializer(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("🚀 Initialisation de la base de données TravelSaaS...");
                
                await _context.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Base de données créée/vérifiée");

                // Create roles avec hiérarchie claire
                Console.WriteLine("🔐 Création des rôles système...");
                var systemRoles = new[] { "SuperAdmin", "AgencyGlobalAdmin", "AgencyPointAdmin", "AgencyOperator" };
                foreach (var role in systemRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role));
                        if (result.Succeeded)
                        {
                            Console.WriteLine($"✅ Rôle '{role}' créé avec succès");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Erreur lors de la création du rôle '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ Rôle '{role}' existe déjà");
                    }
                }

                // Create default Super Admin user from configuration
                Console.WriteLine("👑 Création du Super Administrateur...");
                var adminSettings = _configuration.GetSection("AdminSettings").Get<AdminSettingsDto>();
                
                if (adminSettings != null)
                {
                    Console.WriteLine($"📋 Configuration trouvée: {adminSettings.Email}");
                    var superAdmin = await _userManager.FindByEmailAsync(adminSettings.Email);
                    
                    if (superAdmin == null)
                    {
                        Console.WriteLine("🆕 Création d'un nouveau Super Admin...");
                        superAdmin = new ApplicationUser
                        {
                            UserName = adminSettings.Email,
                            Email = adminSettings.Email,
                            FirstName = adminSettings.FirstName,
                            LastName = adminSettings.LastName,
                            PhoneNumber = adminSettings.PhoneNumber,
                            IsSuperAdmin = true,
                            IsActive = true,
                            EmailConfirmed = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        var result = await _userManager.CreateAsync(superAdmin, adminSettings.Password);
                        if (result.Succeeded)
                        {
                            var roleResult = await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                            if (roleResult.Succeeded)
                            {
                                Console.WriteLine($"✅ Super Admin créé avec succès: {adminSettings.Email}");
                                Console.WriteLine($"✅ Rôle SuperAdmin attribué avec succès");
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Super Admin créé mais erreur lors de l'attribution du rôle: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"❌ Erreur lors de la création du Super Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ Super Admin existe déjà: {adminSettings.Email}");
                        
                        // Vérifier si le rôle est attribué
                        var userRoles = await _userManager.GetRolesAsync(superAdmin);
                        if (!userRoles.Contains("SuperAdmin"))
                        {
                            Console.WriteLine("⚠️ Super Admin existe mais n'a pas le rôle SuperAdmin, attribution en cours...");
                            var roleResult = await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                            if (roleResult.Succeeded)
                            {
                                Console.WriteLine("✅ Rôle SuperAdmin attribué avec succès");
                            }
                            else
                            {
                                Console.WriteLine($"❌ Erreur lors de l'attribution du rôle: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("✅ Rôle SuperAdmin déjà attribué");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Configuration AdminSettings non trouvée dans appsettings.json");
                }
                
                Console.WriteLine("🎉 Initialisation terminée avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'initialisation: {ex.Message}");
                Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
