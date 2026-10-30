using LOGAPI_CON.Models;

namespace LOGAPI_CON.DTOs;

public class PlanRouteResponse
{
    public DateTime InitialDate { get; set; }
    public List<RoutePlan> RoutePlans { get; set; } = [];
    public List<DeliveryOrder> Orders { get; set; } = [];
    public List<DeliveryOrder> UnassignedOrders { get; set; } = [];
}
