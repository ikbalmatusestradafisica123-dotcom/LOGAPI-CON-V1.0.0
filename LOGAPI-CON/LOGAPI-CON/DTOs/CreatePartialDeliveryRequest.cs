using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs;

public sealed class CreatePartialDeliveryRequest
{
    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal DeliveredWeight { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Comments { get; set; }

    public DateTime? RescheduledDate { get; set; }
}
