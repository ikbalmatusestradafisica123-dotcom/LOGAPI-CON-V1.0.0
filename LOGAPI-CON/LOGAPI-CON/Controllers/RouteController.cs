
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Security;
using LOGAPI_CON.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AppPolicies.CanPlanRoutes)]
public class RouteController : ControllerBase
{
    private readonly RoutePlanningService _routePlanningService;

    public RouteController(RoutePlanningService routePlanningService)
    {
        _routePlanningService = routePlanningService;
    }

    [HttpPost("plan")]
    public async Task<IActionResult> PlanRoute(
        [FromBody] PlanRouteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("La solicitud es obligatoria");
        }

        if (request.Date == default)
        {
            return BadRequest("Debes indicar una fecha válida");
        }

        if (request.MaxTonnagePerDay <= 0)
        {
            return BadRequest("El máximo de toneladas planeadas por día debe ser mayor a cero.");
        }

        var result = await _routePlanningService.CreatePlanAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}
