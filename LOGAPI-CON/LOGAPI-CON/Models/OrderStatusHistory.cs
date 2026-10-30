namespace LOGAPI_CON.Models;
public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? TicketReference { get; set; }
    public string Reason { get; set; } = string.Empty;
}
