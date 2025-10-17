using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelSaaS.Controllers
{
    [Authorize(Roles = "AgencyPointAdmin")]
    [Route("PointAdmin")]
    public class PointAdminDashboardController : Controller
    {
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
