using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services;

public interface IMapsService
{
    Task<RouteResult> CalculateRouteAsync(List<string> addresses);
}
