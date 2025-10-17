using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelSaaS.Data;
using TravelSaaS.Models.DTOs;
using TravelSaaS.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace TravelSaaS.Controllers
{
    [Route("SuperAdmin")]
    [Authorize(Policy = "SuperAdminOnly")]
    public class SuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SuperAdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        #region Dashboard Statistics
        [HttpGet("stats")]  // Renommer la route pour éviter le conflit
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalAgencies = await _context.Agencies.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var totalReservations = await _context.Reservations.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

                return Ok(new
                {
                    totalAgencies,
                    totalUsers,
                    totalReservations,
                    activeUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des statistiques");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }
        #endregion

        [HttpGet("api/agencies/{id}")]
public async Task<IActionResult> GetAgencyById(Guid id)
{
    try
    {
        var agency = await _context.Agencies
            .Include(a => a.AgencyPoints)
            .Include(a => a.Users)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Email,
                a.Phone,
                a.Address,
                a.IsActive,
                a.CreatedAt,
                agencyPointsCount = a.AgencyPoints.Count,
                usersCount = a.Users.Count
            })
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agency == null)
        {
            return NotFound(new { message = "Agence non trouvée" });
        }

        return Ok(agency);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors du chargement de l'agence {Id}", id);
        return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
    }
}
        #region Agency Management
        [HttpGet("api/agencies")]
        public async Task<IActionResult> GetAgencies()
        {
            try
            {
                var agencies = await _context.Agencies
                    .Include(a => a.AgencyPoints)
                    .Include(a => a.Users)
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Email,
                        a.Phone,
                        a.Address,
                        a.IsActive,
                        a.CreatedAt,
                        agencyPointsCount = a.AgencyPoints.Count,
                        usersCount = a.Users.Count
                    })
                    .ToListAsync();

                return Ok(agencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des agences");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPost("api/agencies")]
        public async Task<IActionResult> CreateAgency([FromBody] CreateAgencyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Vérifier si l'agence existe déjà
                if (await _context.Agencies.AnyAsync(a => a.Name == dto.Name))
                {
                    return BadRequest(new { message = "Une agence avec ce nom existe déjà" });
                }

                var agency = new Agency
                {
                    Name = dto.Name,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Agencies.Add(agency);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAgencies), new { id = agency.Id }, agency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPut("api/agencies/{id}")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] UpdateAgencyDto dto)
        {
            try
            {
                var agency = await _context.Agencies.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agence non trouvée" });
                }

                if (dto.Name != null) agency.Name = dto.Name;
                if (dto.Address != null) agency.Address = dto.Address;
                if (dto.Phone != null) agency.Phone = dto.Phone;
                if (dto.Email != null) agency.Email = dto.Email;

                await _context.SaveChangesAsync();

                return Ok(agency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPut("api/agencies/{id}/toggle-status")]
        public async Task<IActionResult> ToggleAgencyStatus(Guid id, [FromBody] ToggleStatusDto dto)
        {
            try
            {
                var agency = await _context.Agencies.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agence non trouvée" });
                }

                agency.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Agence {(dto.IsActive ? "activée" : "désactivée")} avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification du statut de l'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }
        #endregion

        #region Agency Points Management
        [HttpGet("api/agency-points")]
        public async Task<IActionResult> GetAgencyPoints()
        {
            try
            {
                var agencyPoints = await _context.AgencyPoints
                    .Include(ap => ap.Agency)
                    .Include(ap => ap.Users)
                    .Select(ap => new
                    {
                        ap.Id,
                        ap.Name,
                        ap.Address,
                        ap.Phone,
                        ap.Email,
                        ap.IsActive,
                        ap.CreatedAt,
                        ap.AgencyId,
                        agencyName = ap.Agency.Name,
                        usersCount = ap.Users.Count
                    })
                    .ToListAsync();

                return Ok(agencyPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des points d'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpGet("api/agency-points/{id}")]
        public async Task<IActionResult> GetAgencyPointById(Guid id)
        {
            try
            {
                var point = await _context.AgencyPoints
                    .Include(p => p.Agency)
                    .Include(p => p.Users)
                    .Include(p => p.Reservations)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (point == null)
                {
                    return NotFound(new { message = "Point d'agence non trouvé" });
                }

                var operatorsCount = 0;
                foreach (var u in point.Users)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    if (roles.Contains("AgencyOperator")) operatorsCount++;
                }

                var response = new
                {
                    point.Id,
                    point.Name,
                    point.Email,
                    point.Phone,
                    point.Address,
                    point.IsActive,
                    point.CreatedAt,
                    point.AgencyId,
                    AgencyName = point.Agency.Name,
                    UsersCount = point.Users.Count,
                    OperatorsCount = operatorsCount,
                    ReservationsCount = point.Reservations.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du point d'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPost("api/agency-points")]
        public async Task<IActionResult> CreateAgencyPoint([FromBody] CreateAgencyPointDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Vérifier si l'agence existe
                var agency = await _context.Agencies.FindAsync(dto.AgencyId);
                if (agency == null)
                {
                    return BadRequest(new { message = "Agence non trouvée" });
                }

                // Vérifier si le point d'agence existe déjà
                if (await _context.AgencyPoints.AnyAsync(ap => ap.Name == dto.Name && ap.AgencyId == dto.AgencyId))
                {
                    return BadRequest(new { message = "Un point d'agence avec ce nom existe déjà dans cette agence" });
                }

                var agencyPoint = new AgencyPoint
                {
                    AgencyId = dto.AgencyId,
                    Name = dto.Name,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AgencyPoints.Add(agencyPoint);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAgencyPoints), new { id = agencyPoint.Id }, agencyPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du point d'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPut("api/agency-points/{id}/toggle-status")]
        public async Task<IActionResult> ToggleAgencyPointStatus(Guid id, [FromBody] ToggleStatusDto dto)
        {
            try
            {
                var agencyPoint = await _context.AgencyPoints.FindAsync(id);
                if (agencyPoint == null)
                {
                    return NotFound(new { message = "Point d'agence non trouvé" });
                }
                agencyPoint.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Point d'agence {(dto.IsActive ? "activé" : "désactivé")} avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification du statut du point d'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPut("api/agency-points/{id}")]
        public async Task<IActionResult> UpdateAgencyPoint(Guid id, [FromBody] UpdateAgencyPointDto dto)
        {
            try
            {
                var agencyPoint = await _context.AgencyPoints.FindAsync(id);
                if (agencyPoint == null)
                {
                    return NotFound(new { message = "Point d'agence non trouvé" });
                }

                if (!string.IsNullOrWhiteSpace(dto.Name)) agencyPoint.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.Address)) agencyPoint.Address = dto.Address;
                if (!string.IsNullOrWhiteSpace(dto.Phone)) agencyPoint.Phone = dto.Phone;
                if (dto.Email != null) agencyPoint.Email = dto.Email;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Point d'agence mis à jour", agencyPoint.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du point d'agence");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }
        #endregion

        #region User Management
        [HttpGet("api/users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Agency)
                    .Include(u => u.AgencyPoint);

                var total = await query.CountAsync();
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.PhoneNumber,
                        u.IsActive,
                        u.IsSuperAdmin,
                        u.CreatedAt,
                        u.LastLoginAt,
                        agencyName = u.Agency != null ? u.Agency.Name : null,
                        agencyPointName = u.AgencyPoint != null ? u.AgencyPoint.Name : null,
                        roles = _userManager.GetRolesAsync(u).Result
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = users,
                    total,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des utilisateurs");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPost("api/users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Vérifier si l'utilisateur existe déjà
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                {
                    return BadRequest(new { message = "Un utilisateur avec cet email existe déjà" });
                }

                // Vérifier les permissions selon le rôle
                if (dto.Role == "AgencyGlobalAdmin" || dto.Role == "AgencyPointAdmin" || dto.Role == "AgencyOperator")
                {
                    if (dto.AgencyId == null)
                    {
                        return BadRequest(new { message = "L'ID de l'agence est requis pour ce rôle" });
                    }

                    var agency = await _context.Agencies.FindAsync(dto.AgencyId);
                    if (agency == null)
                    {
                        return BadRequest(new { message = "Agence non trouvée" });
                    }
                }

                if (dto.Role == "AgencyPointAdmin" || dto.Role == "AgencyOperator")
                {
                    if (dto.AgencyPointId == null)
                    {
                        return BadRequest(new { message = "L'ID du point d'agence est requis pour ce rôle" });
                    }

                    var agencyPoint = await _context.AgencyPoints.FindAsync(dto.AgencyPointId);
                    if (agencyPoint == null)
                    {
                        return BadRequest(new { message = "Point d'agence non trouvé" });
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    AgencyId = dto.AgencyId,
                    AgencyPointId = dto.AgencyPointId,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, dto.Role);
                    return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
                }

                return BadRequest(new { message = "Erreur lors de la création de l'utilisateur", errors = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'utilisateur");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPut("api/users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    // Vérifier collision email
                    var existing = await _userManager.FindByEmailAsync(dto.Email);
                    if (existing != null && existing.Id != user.Id)
                    {
                        return BadRequest(new { message = "Un utilisateur avec cet email existe déjà" });
                    }
                    user.Email = dto.Email;
                    user.UserName = dto.Email;
                }
                if (dto.FirstName != null) user.FirstName = dto.FirstName;
                if (dto.LastName != null) user.LastName = dto.LastName;
                if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
                if (dto.AgencyId.HasValue) user.AgencyId = dto.AgencyId;
                if (dto.AgencyPointId.HasValue) user.AgencyPointId = dto.AgencyPointId;

                // Mettre à jour le rôle si fourni
                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Retourner un DTO cohérent avec la liste
                    var roles = await _userManager.GetRolesAsync(user);
                    var response = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        user.IsActive,
                        user.IsSuperAdmin,
                        user.CreatedAt,
                        user.LastLoginAt,
                        agencyName = user.AgencyId.HasValue ? (await _context.Agencies.Where(a => a.Id == user.AgencyId).Select(a => a.Name).FirstOrDefaultAsync()) : null,
                        agencyPointName = user.AgencyPointId.HasValue ? (await _context.AgencyPoints.Where(p => p.Id == user.AgencyPointId).Select(p => p.Name).FirstOrDefaultAsync()) : null,
                        roles
                    };
                    return Ok(response);
                }

                return BadRequest(new { message = "Erreur lors de la mise à jour de l'utilisateur", errors = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'utilisateur");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }

        [HttpPut("api/users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id, [FromBody] ToggleStatusDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                user.IsActive = dto.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new { message = $"Utilisateur {(dto.IsActive ? "activé" : "désactivé")} avec succès" });
                }

                return BadRequest(new { message = "Erreur lors de la modification du statut", errors = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification du statut de l'utilisateur");
                return StatusCode(500, new { message = "Une erreur est survenue", error = ex.Message });
            }
        }
        #endregion

        // Views - Agencies/Users/AgencyPoints
        [HttpGet("Agencies")]
        public IActionResult Agencies()
        {
            return View();
        }

        [HttpGet("Agencies/{id}")]
        public async Task<IActionResult> AgencyDetails(Guid id)
        {
            var agency = await _context.Agencies
                .Include(a => a.AgencyPoints)
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (agency == null) return NotFound();

            var users = await _context.Users
                .Where(u => u.AgencyId == id)
                .ToListAsync();

            var rolesMap = new Dictionary<string, int> { { "SuperAdmin", 0 }, { "AgencyGlobalAdmin", 0 }, { "AgencyPointAdmin", 0 }, { "AgencyOperator", 0 } };
            int activeUsers = 0, inactiveUsers = 0;
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                foreach (var r in roles)
                {
                    if (rolesMap.ContainsKey(r)) rolesMap[r] += 1;
                }
                if (u.IsActive) activeUsers++; else inactiveUsers++;
            }

            var vm = new
            {
                agency.Id,
                agency.Name,
                agency.Email,
                agency.Phone,
                agency.Address,
                agency.IsActive,
                agency.CreatedAt,
                AgencyPoints = agency.AgencyPoints.Select(p => new { p.Id, p.Name, p.Email, p.Phone, p.Address, p.IsActive }).ToList(),
                UsersStats = new { Active = activeUsers, Inactive = inactiveUsers, Roles = rolesMap }
            };

            return Json(vm);
        }

        [HttpGet("Users")]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet("AgencyPoints")]
        [HttpGet("agency-points")]
        public IActionResult AgencyPoints()
        {
            return View();
        }

        [HttpGet("Dashboard")]
        [Authorize(Policy = "SuperAdminOnly")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }

    #region DTOs
    public class ToggleStatusDto
    {
        public bool IsActive { get; set; }
    }
    #endregion
    
}
