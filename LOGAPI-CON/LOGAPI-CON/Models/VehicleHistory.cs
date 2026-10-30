namespace LOGAPI_CON.Models;
public class VehicleHistory
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeByUser { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}
