using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelSaaS.Controllers
{
    [Authorize(Roles = "AgencyOperator")]
    [Route("Operator")]
    public class OperatorDashboardController : Controller
    {
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
