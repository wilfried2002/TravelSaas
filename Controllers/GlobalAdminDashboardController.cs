using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelSaaS.Data;
using TravelSaaS.Models.Entities;
using TravelSaaS.Models.DTOs;

namespace TravelSaaS.Controllers
{
    [Authorize(Policy = "GlobalAdminOnly")]
    [Route("GlobalAdmin")]
    public class GlobalAdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GlobalAdminDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("AgencyPoints")]
        public IActionResult AgencyPoints()
        {
            // Réutilise la même interface que le Super Admin
            return View("~/Views/SuperAdmin/AgencyPoints.cshtml");
        }

        [HttpGet("api/agency-points/{id}")]
        public async Task<IActionResult> GetAgencyPointById(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AgencyId == null) return NotFound(new { message = "Agence du GlobalAdmin introuvable" });

            var point = await _context.AgencyPoints
                .Include(p => p.Agency)
                .Include(p => p.Users)
                .Include(p => p.Reservations)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == user.AgencyId);

            if (point == null) return NotFound(new { message = "Point d'agence non trouvé" });

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
                ReservationsCount = point.Reservations.Count
            };

            return Ok(response);
        }

        // API - Agencies (limité à l'agence du GlobalAdmin)
        [HttpGet("api/agencies")]
        public async Task<IActionResult> GetAgencies()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AgencyId == null) return Ok(Array.Empty<object>());

            var a = await _context.Agencies
                .Where(x => x.Id == user.AgencyId)
                .Select(x => new {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Phone,
                    x.Address,
                    x.IsActive,
                    x.CreatedAt,
                    agencyPointsCount = x.AgencyPoints.Count,
                    usersCount = x.Users.Count
                })
                .FirstOrDefaultAsync();

            return Ok(a != null ? new[] { a } : Array.Empty<object>());
        }

        // API - Agency Points (filtrés par l'agence du GlobalAdmin)
        [HttpGet("api/agency-points")]
        public async Task<IActionResult> GetAgencyPoints()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AgencyId == null) return Ok(Array.Empty<object>());

            var points = await _context.AgencyPoints
                .Include(ap => ap.Agency)
                .Include(ap => ap.Users)
                .Where(ap => ap.AgencyId == user.AgencyId)
                .Select(ap => new {
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

            return Ok(points);
        }

        [HttpPost("api/agency-points")]
        public async Task<IActionResult> CreateAgencyPoint([FromBody] CreateAgencyPointDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AgencyId == null)
                return BadRequest(new { message = "Agence du GlobalAdmin introuvable" });

            // Forcer l'affectation à l'agence du Global Admin
            var agencyId = user.AgencyId.Value;

            // Unicité nom dans l'agence
            var exists = await _context.AgencyPoints.AnyAsync(ap => ap.AgencyId == agencyId && ap.Name == dto.Name);
            if (exists) return BadRequest(new { message = "Un point avec ce nom existe déjà dans votre agence" });

            var agencyPoint = new AgencyPoint
            {
                AgencyId = agencyId,
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

        [HttpPut("api/agency-points/{id}/toggle-status")]
        public async Task<IActionResult> ToggleAgencyPointStatus(Guid id, [FromBody] ToggleStatusDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AgencyId == null)
                return BadRequest(new { message = "Agence du GlobalAdmin introuvable" });

            var point = await _context.AgencyPoints.FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == user.AgencyId);
            if (point == null) return NotFound(new { message = "Point non trouvé dans votre agence" });

            point.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Point {(dto.IsActive ? "activé" : "désactivé")} avec succès" });
        }

        [HttpPut("api/agency-points/{id}")]
        public async Task<IActionResult> UpdateAgencyPoint(Guid id, [FromBody] UpdateAgencyPointDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.AgencyId == null)
                return BadRequest(new { message = "Agence du GlobalAdmin introuvable" });

            var point = await _context.AgencyPoints.FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == user.AgencyId);
            if (point == null) return NotFound(new { message = "Point non trouvé dans votre agence" });

            if (!string.IsNullOrWhiteSpace(dto.Name)) point.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Address)) point.Address = dto.Address;
            if (!string.IsNullOrWhiteSpace(dto.Phone)) point.Phone = dto.Phone;
            if (dto.Email != null) point.Email = dto.Email;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Point mis à jour" });
        }
    }
}