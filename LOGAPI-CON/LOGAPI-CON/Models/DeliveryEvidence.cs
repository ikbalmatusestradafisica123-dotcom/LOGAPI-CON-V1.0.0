namespace LOGAPI_CON.Models;

public sealed class DeliveryEvidence
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string EvidenceType { get; set; } = "Delivery";
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Comments { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
