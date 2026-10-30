namespace LOGAPI_CON.Models;

public class RouteStop
{
    public int Sequence { get; set; }
    public int OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}
