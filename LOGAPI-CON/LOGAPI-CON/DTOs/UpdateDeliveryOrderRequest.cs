using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs;

public sealed class UpdateDeliveryOrderRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Customer { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal Weight { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Sector { get; set; } = string.Empty;

    public DateTime? ScheduledDate { get; set; }
}