using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IKPIDashboardService _dashboardService;

        public DashboardController(IKPIDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKPIs()
        {
            var response = await _dashboardService.GetDashboardKPIsAsync();


            return Ok(response.Data);
        }
    }
}