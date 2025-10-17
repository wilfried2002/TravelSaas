using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelSaaS.Data;
using TravelSaaS.Models.DTOs;
using TravelSaaS.Models.Entities;

namespace TravelSaaS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "AgencyPointAdmin")]
    public class AgencyPointAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyPointAdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<(Guid? AgencyId, Guid? AgencyPointId)> GetCurrentUserScope()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return (null, null);

            var user = await _userManager.FindByIdAsync(userId);
            return (user?.AgencyId, user?.AgencyPointId);
        }

        // === GESTION DES OPÉRATEURS DU POINT D'AGENCE ===

        [HttpPost("operators")]
        public async Task<IActionResult> CreateOperator([FromBody] CreateUserDto dto)
        {
            var (currentAgencyId, currentAgencyPointId) = await GetCurrentUserScope();
            if (currentAgencyId == null || currentAgencyPointId == null)
                return Forbid("Utilisateur non associé à un point d'agence");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Un PointAdmin ne peut créer que des Operators
            if (dto.Role != "AgencyOperator")
                return BadRequest("Vous ne pouvez créer que des opérateurs (AgencyOperator)");

            // Forcer l'agence et le point d'agence à ceux de l'utilisateur connecté
            dto.AgencyId = currentAgencyId.Value;
            dto.AgencyPointId = currentAgencyPointId.Value;

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                AgencyId = dto.AgencyId,
                AgencyPointId = dto.AgencyPointId,
                CreatedById = currentUserId,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "AgencyOperator");

            var response = await GetUserResponseDto(user);
            return CreatedAtAction(nameof(GetOperator), new { id = user.Id }, response);
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetMyOperators()
        {
            var (currentAgencyId, currentAgencyPointId) = await GetCurrentUserScope();
            if (currentAgencyId == null || currentAgencyPointId == null)
                return Forbid("Utilisateur non associé à un point d'agence");

            var users = await _context.Users
                .Include(u => u.Agency)
                .Include(u => u.AgencyPoint)
                .Where(u => u.AgencyId == currentAgencyId && u.AgencyPointId == currentAgencyPointId)
                .ToListAsync();

            var responses = new List<UserResponseDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Ne retourner que les opérateurs
                if (roles.Contains("AgencyOperator"))
                {
                    responses.Add(await GetUserResponseDto(user));
                }
            }

            return Ok(responses);
        }

        [HttpGet("operators/{id}")]
        public async Task<IActionResult> GetOperator(string id)
        {
            var (currentAgencyId, currentAgencyPointId) = await GetCurrentUserScope();
            if (currentAgencyId == null || currentAgencyPointId == null)
                return Forbid("Utilisateur non associé à un point d'agence");

            var user = await _context.Users
                .Include(u => u.Agency)
                .Include(u => u.AgencyPoint)
                .FirstOrDefaultAsync(u => u.Id == id && 
                                        u.AgencyId == currentAgencyId && 
                                        u.AgencyPointId == currentAgencyPointId);

            if (user == null)
                return NotFound();

            // Vérifier que c'est bien un opérateur
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("AgencyOperator"))
                return NotFound();

            var response = await GetUserResponseDto(user);
            return Ok(response);
        }

        [HttpPut("operators/{id}")]
        public async Task<IActionResult> UpdateOperator(string id, [FromBody] UpdateUserDto dto)
        {
            var (currentAgencyId, currentAgencyPointId) = await GetCurrentUserScope();
            if (currentAgencyId == null || currentAgencyPointId == null)
                return Forbid("Utilisateur non associé à un point d'agence");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && 
                                        u.AgencyId == currentAgencyId && 
                                        u.AgencyPointId == currentAgencyPointId);

            if (user == null)
                return NotFound();

            // Vérifier que c'est bien un opérateur
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("AgencyOperator"))
                return NotFound();

            // Interdire le changement de rôle
            if (!string.IsNullOrEmpty(dto.Role) && dto.Role != "AgencyOperator")
                return BadRequest("Modification du rôle non autorisée");

            // Interdire le changement d'agence ou de point d'agence
            if (dto.AgencyId.HasValue || dto.AgencyPointId.HasValue)
                return BadRequest("Modification d'agence ou de point d'agence non autorisée");

            // Mise à jour des propriétés autorisées
            if (!string.IsNullOrEmpty(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                user.LastName = dto.LastName;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var response = await GetUserResponseDto(user);
            return Ok(response);
        }

        [HttpDelete("operators/{id}")]
        public async Task<IActionResult> SuspendOperator(string id)
        {
            var (currentAgencyId, currentAgencyPointId) = await GetCurrentUserScope();
            if (currentAgencyId == null || currentAgencyPointId == null)
                return Forbid("Utilisateur non associé à un point d'agence");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && 
                                        u.AgencyId == currentAgencyId && 
                                        u.AgencyPointId == currentAgencyPointId);

            if (user == null)
                return NotFound();

            // Vérifier que c'est bien un opérateur
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("AgencyOperator"))
                return NotFound();

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Opérateur suspendu avec succès" });
        }

        // === STATISTIQUES DU POINT D'AGENCE ===

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var (currentAgencyId, currentAgencyPointId) = await GetCurrentUserScope();
            if (currentAgencyId == null || currentAgencyPointId == null)
                return Forbid("Utilisateur non associé à un point d'agence");

            var agencyPoint = await _context.AgencyPoints
                .Include(ap => ap.Agency)
                .Include(ap => ap.Users)
                .Include(ap => ap.Reservations)
                .Include(ap => ap.Travels)
                .FirstOrDefaultAsync(ap => ap.Id == currentAgencyPointId);

            if (agencyPoint == null)
                return NotFound();

            var operators = new List<UserResponseDto>();
            foreach (var user in agencyPoint.Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("AgencyOperator"))
                {
                    operators.Add(await GetUserResponseDto(user));
                }
            }

            var stats = new
            {
                AgencyPoint = new AgencyPointResponseDto
                {
                    Id = agencyPoint.Id,
                    Name = agencyPoint.Name,
                    Address = agencyPoint.Address,
                    Phone = agencyPoint.Phone,
                    Email = agencyPoint.Email,
                    AgencyId = agencyPoint.AgencyId,
                    AgencyName = agencyPoint.Agency.Name,
                    IsActive = agencyPoint.IsActive,
                    CreatedAt = agencyPoint.CreatedAt,
                    UsersCount = operators.Count
                },
                Operators = operators,
                Stats = new
                {
                    TotalOperators = operators.Count,
                    ActiveOperators = operators.Count(o => o.IsActive),
                    TotalReservations = agencyPoint.Reservations.Count,
                    PendingReservations = agencyPoint.Reservations.Count(r => r.Status == "Pending"),
                    ConfirmedReservations = agencyPoint.Reservations.Count(r => r.Status == "Confirmed"),
                    TotalTravels = agencyPoint.Travels.Count,
                    ActiveTravels = agencyPoint.Travels.Count(t => t.IsActive)
                }
            };

            return Ok(stats);
        }

        // === MÉTHODES UTILITAIRES ===

        private async Task<UserResponseDto> GetUserResponseDto(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToArray(),
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                AgencyId = user.AgencyId,
                AgencyName = user.Agency?.Name,
                AgencyPointId = user.AgencyPointId,
                AgencyPointName = user.AgencyPoint?.Name,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
