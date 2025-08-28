using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelSaaS.Models.DTOs;
using TravelSaaS.Models.Entities;
using TravelSaaS.Services;

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
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await _authService.GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized();
        }

    }
}
