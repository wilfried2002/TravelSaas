using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelSaaS.Models.DTOs;
using TravelSaaS.Models.Entities;

namespace TravelSaaS.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index() => View();

        public IActionResult Login() => View();

        public IActionResult SuperAdminLogin(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SuperAdminLogin(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            _logger.LogInformation("Tentative de connexion SuperAdmin pour {Email}", email);
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email ou mot de passe vide");
                ViewBag.ErrorMessage = "Email et mot de passe requis";
                return View();
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Utilisateur non trouvé: {Email}", email);
                    ViewBag.ErrorMessage = "Email ou mot de passe incorrect";
                    return View();
                }

                _logger.LogInformation("Utilisateur trouvé: {Email}, IsSuperAdmin: {IsSuperAdmin}, IsActive: {IsActive}", 
                    user.Email, user.IsSuperAdmin, user.IsActive);

                if (!await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                {
                    _logger.LogWarning("Utilisateur {Email} n'est pas SuperAdmin", email);
                    ViewBag.ErrorMessage = "Email ou mot de passe incorrect";
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(email, password, true, false);
                _logger.LogInformation("Résultat de connexion: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, RequiresTwoFactor={RequiresTwoFactor}", 
                    result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Connexion réussie pour SuperAdmin: {Email}", email);
                    return RedirectToAction("Dashboard", "SuperAdmin");
                }
                else
                {
                    _logger.LogWarning("Échec de connexion pour SuperAdmin: {Email}", email);
                    ViewBag.ErrorMessage = "Email ou mot de passe incorrect";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion SuperAdmin pour {Email}", email);
                ViewBag.ErrorMessage = "Une erreur est survenue lors de la connexion";
            }

            return View();
        }

        // Action de test pour vérifier les utilisateurs
        public async Task<IActionResult> TestUsers()
        {
            var users = _userManager.Users.ToList();
            var userInfo = new List<object>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userInfo.Add(new
                {
                    Email = user.Email,
                    IsSuperAdmin = user.IsSuperAdmin,
                    IsActive = user.IsActive,
                    Roles = roles,
                    EmailConfirmed = user.EmailConfirmed
                });
            }
            
            return Json(userInfo);
        }

        public IActionResult GlobalAdminLogin() => View();

        [HttpPost]
        public async Task<IActionResult> GlobalAdminLogin(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Email et mot de passe requis";
                return View();
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && await _userManager.IsInRoleAsync(user, "AgencyGlobalAdmin"))
                {
                    var result = await _signInManager.PasswordSignInAsync(email, password, true, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard", "GlobalAdminDashboard");
                    }
                }
                ViewBag.ErrorMessage = "Email ou mot de passe incorrect";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion GlobalAdmin pour {Email}", email);
                ViewBag.ErrorMessage = "Une erreur est survenue lors de la connexion";
            }
            return View();
        }

        public IActionResult PointAdminLogin() => View();

        [HttpPost]
        public async Task<IActionResult> PointAdminLogin(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Email et mot de passe requis";
                return View();
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && await _userManager.IsInRoleAsync(user, "AgencyPointAdmin"))
                {
                    var result = await _signInManager.PasswordSignInAsync(email, password, true, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard", "PointAdminDashboard");
                    }
                }
                ViewBag.ErrorMessage = "Email ou mot de passe incorrect";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion PointAdmin pour {Email}", email);
                ViewBag.ErrorMessage = "Une erreur est survenue lors de la connexion";
            }
            return View();
        }

        public IActionResult OperatorLogin() => View();

        [HttpPost]
        public async Task<IActionResult> OperatorLogin(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Email et mot de passe requis";
                return View();
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && await _userManager.IsInRoleAsync(user, "AgencyOperator"))
                {
                    var result = await _signInManager.PasswordSignInAsync(email, password, true, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard", "OperatorDashboard");
                    }
                }
                ViewBag.ErrorMessage = "Email ou mot de passe incorrect";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion Operator pour {Email}", email);
                ViewBag.ErrorMessage = "Une erreur est survenue lors de la connexion";
            }
            return View();
        }
    }
}