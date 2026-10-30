namespace LOGAPI_CON.Models;

public class DeliveryOrder
{
    public int Id { get; set; }
    public string? ExternalFolio { get; set; }
    public string Customer { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public bool IsPriority { get; set; }
    public string? PriorityReason { get; set; }
}