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
    [Authorize(Policy = "GlobalAdminOnly")]
    public class AgencyGlobalAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyGlobalAdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Guid?> GetCurrentUserAgencyId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var user = await _userManager.FindByIdAsync(userId);
            return user?.AgencyId;
        }

        // === GESTION DES POINTS D'AGENCE (dans son agence seulement) ===

        [HttpPost("agency-points")]
        public async Task<IActionResult> CreateAgencyPoint([FromBody] CreateAgencyPointDto dto)
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            // Forcer l'agence à celle de l'utilisateur connecté
            dto.AgencyId = currentAgencyId.Value;

            var agency = await _context.Agencies.FindAsync(dto.AgencyId);
            if (agency == null)
                return BadRequest("Agence introuvable");

            var agencyPoint = new AgencyPoint
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                AgencyId = dto.AgencyId
            };

            _context.AgencyPoints.Add(agencyPoint);
            await _context.SaveChangesAsync();

            var response = new AgencyPointResponseDto
            {
                Id = agencyPoint.Id,
                Name = agencyPoint.Name,
                Address = agencyPoint.Address,
                Phone = agencyPoint.Phone,
                Email = agencyPoint.Email,
                AgencyId = agencyPoint.AgencyId,
                AgencyName = agency.Name,
                IsActive = agencyPoint.IsActive,
                CreatedAt = agencyPoint.CreatedAt,
                UsersCount = 0
            };

            return CreatedAtAction(nameof(GetAgencyPoint), new { id = agencyPoint.Id }, response);
        }

        [HttpGet("agency-points")]
        public async Task<IActionResult> GetMyAgencyPoints()
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var agencyPoints = await _context.AgencyPoints
                .Include(ap => ap.Agency)
                .Include(ap => ap.Users)
                .Where(ap => ap.AgencyId == currentAgencyId)
                .Select(ap => new AgencyPointResponseDto
                {
                    Id = ap.Id,
                    Name = ap.Name,
                    Address = ap.Address,
                    Phone = ap.Phone,
                    Email = ap.Email,
                    AgencyId = ap.AgencyId,
                    AgencyName = ap.Agency.Name,
                    IsActive = ap.IsActive,
                    CreatedAt = ap.CreatedAt,
                    UsersCount = ap.Users.Count
                })
                .ToListAsync();

            return Ok(agencyPoints);
        }

        [HttpGet("agency-points/{id}")]
        public async Task<IActionResult> GetAgencyPoint(Guid id)
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var agencyPoint = await _context.AgencyPoints
                .Include(ap => ap.Agency)
                .Include(ap => ap.Users)
                .FirstOrDefaultAsync(ap => ap.Id == id && ap.AgencyId == currentAgencyId);

            if (agencyPoint == null)
                return NotFound();

            var response = new AgencyPointResponseDto
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
                UsersCount = agencyPoint.Users.Count
            };

            return Ok(response);
        }

        // === GESTION DES UTILISATEURS DE L'AGENCE ===

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Un GlobalAdmin ne peut créer que des PointAdmin et des Operators
            if (dto.Role != "AgencyPointAdmin" && dto.Role != "AgencyOperator")
                return BadRequest("Rôle non autorisé. Vous ne pouvez créer que des AgencyPointAdmin ou AgencyOperator");

            // Forcer l'agence à celle de l'utilisateur connecté
            dto.AgencyId = currentAgencyId.Value;

            // Vérifier que le point d'agence appartient à l'agence si spécifié
            if (dto.AgencyPointId.HasValue)
            {
                var agencyPoint = await _context.AgencyPoints
                    .FirstOrDefaultAsync(ap => ap.Id == dto.AgencyPointId.Value && ap.AgencyId == currentAgencyId);
                if (agencyPoint == null)
                    return BadRequest("Point d'agence introuvable dans votre agence");
            }

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

            await _userManager.AddToRoleAsync(user, dto.Role);

            var response = await GetUserResponseDto(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetMyAgencyUsers()
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var users = await _context.Users
                .Include(u => u.Agency)
                .Include(u => u.AgencyPoint)
                .Where(u => u.AgencyId == currentAgencyId)
                .ToListAsync();

            var responses = new List<UserResponseDto>();
            foreach (var user in users)
            {
                responses.Add(await GetUserResponseDto(user));
            }

            return Ok(responses);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var user = await _context.Users
                .Include(u => u.Agency)
                .Include(u => u.AgencyPoint)
                .FirstOrDefaultAsync(u => u.Id == id && u.AgencyId == currentAgencyId);

            if (user == null)
                return NotFound();

            var response = await GetUserResponseDto(user);
            return Ok(response);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.AgencyId == currentAgencyId);

            if (user == null)
                return NotFound();

            // Vérifier que le rôle est autorisé si spécifié
            if (!string.IsNullOrEmpty(dto.Role) && dto.Role != "AgencyPointAdmin" && dto.Role != "AgencyOperator")
                return BadRequest("Rôle non autorisé");

            // Mise à jour des propriétés
            if (!string.IsNullOrEmpty(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                user.LastName = dto.LastName;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            // Vérifier que le point d'agence appartient à l'agence
            if (dto.AgencyPointId.HasValue)
            {
                var agencyPoint = await _context.AgencyPoints
                    .FirstOrDefaultAsync(ap => ap.Id == dto.AgencyPointId.Value && ap.AgencyId == currentAgencyId);
                if (agencyPoint == null)
                    return BadRequest("Point d'agence introuvable dans votre agence");
                user.AgencyPointId = dto.AgencyPointId.Value;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Mise à jour du rôle si spécifié
            if (!string.IsNullOrEmpty(dto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            var response = await GetUserResponseDto(user);
            return Ok(response);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> SuspendUser(string id)
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.AgencyId == currentAgencyId);

            if (user == null)
                return NotFound();

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Utilisateur suspendu avec succès" });
        }

        // === STATISTIQUES DE L'AGENCE ===

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var currentAgencyId = await GetCurrentUserAgencyId();
            if (currentAgencyId == null)
                return Forbid("Utilisateur non associé à une agence");

            var agency = await _context.Agencies
                .Include(a => a.Users)
                .Include(a => a.AgencyPoints)
                .Include(a => a.Reservations)
                .Include(a => a.Travels)
                .FirstOrDefaultAsync(a => a.Id == currentAgencyId);

            if (agency == null)
                return NotFound();

            var stats = new
            {
                Agency = new AgencyResponseDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Address = agency.Address,
                    Phone = agency.Phone,
                    Email = agency.Email,
                    IsActive = agency.IsActive,
                    CreatedAt = agency.CreatedAt,
                    UsersCount = agency.Users.Count,
                    AgencyPointsCount = agency.AgencyPoints.Count
                },
                Stats = new
                {
                    TotalUsers = agency.Users.Count,
                    ActiveUsers = agency.Users.Count(u => u.IsActive),
                    TotalAgencyPoints = agency.AgencyPoints.Count,
                    ActiveAgencyPoints = agency.AgencyPoints.Count(ap => ap.IsActive),
                    TotalReservations = agency.Reservations.Count,
                    PendingReservations = agency.Reservations.Count(r => r.Status == "Pending"),
                    TotalTravels = agency.Travels.Count,
                    ActiveTravels = agency.Travels.Count(t => t.IsActive)
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
