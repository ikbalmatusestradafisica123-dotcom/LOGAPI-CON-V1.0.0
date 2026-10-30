namespace LOGAPI_CON.Models;

public class RoutePlan
{
    public int VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public decimal VehicleCapacity { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal RemainingCapacity { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public List<RouteStop> Stops { get; set; } = [];
}
