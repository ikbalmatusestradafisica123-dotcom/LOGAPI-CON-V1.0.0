namespace LOGAPI_CON.Models;

public sealed class PartialDelivery
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal OrderedWeight { get; set; }

    public decimal DeliveredWeight { get; set; }

    public decimal PendingWeight { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Comments { get; set; }

    public DateTime? RescheduledDate { get; set; }

    public string RegisteredByUserId { get; set; } = string.Empty;

    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;
}
