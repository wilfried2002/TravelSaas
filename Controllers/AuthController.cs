using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelSaaS.Models.DTOs;
using TravelSaaS.Models.Entities;
using TravelSaaS.Services;
using Microsoft.EntityFrameworkCore;


namespace TravelSaaS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthService _authService;

        public AuthController(UserManager<ApplicationUser> userManager, AuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }
        [HttpPost("register-superadmin")]
        public async Task<IActionResult> RegisterSuperAdmin([FromBody] RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsSuperAdmin = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "SuperAdmin");
                var token = await _authService.GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Données de connexion invalides", errors = ModelState });
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Compte utilisateur désactivé" });
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect" });
                }

                // Mettre à jour la dernière connexion
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var token = await _authService.GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);
                
                return Ok(new { 
                    Token = token,
                    User = new {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsSuperAdmin = user.IsSuperAdmin,
                        AgencyId = user.AgencyId,
                        AgencyPointId = user.AgencyPointId,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }

        [HttpGet("check-superadmin")]
        public async Task<IActionResult> CheckSuperAdmin()
        {
            try
            {
                var superAdmins = await _userManager.Users
                    .Where(u => u.IsSuperAdmin)
                    .ToListAsync();

                var adminSettings = new List<object>();
                foreach (var u in superAdmins)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    adminSettings.Add(new
                    {
                        u.Id,
                        u.Email,
                        u.UserName,
                        u.FirstName,
                        u.LastName,
                        u.IsActive,
                        u.EmailConfirmed,
                        u.CreatedAt,
                        Roles = roles
                    });
                }

                return Ok(new
                {
                    message = "Vérification des Super Admins",
                    count = adminSettings.Count,
                    users = adminSettings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la vérification", error = ex.Message });
            }
        }

        [HttpPost("create-superadmin")]
        public async Task<IActionResult> CreateSuperAdmin([FromBody] CreateSuperAdminDto model)
        {
            try
            {
                // Vérifier si l'utilisateur existe déjà
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Un utilisateur avec cet email existe déjà" });
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    IsSuperAdmin = true,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "SuperAdmin");
                    return Ok(new { message = "Super Admin créé avec succès", userId = user.Id });
                }

                return BadRequest(new { message = "Erreur lors de la création", errors = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne du serveur", error = ex.Message });
            }
        }
    }
}
