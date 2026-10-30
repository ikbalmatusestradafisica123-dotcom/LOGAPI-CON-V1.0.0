using System.ComponentModel.DataAnnotations;
using LOGAPI_CON.Models;

namespace LOGAPI_CON.DTOs;
public sealed class ChangeOrderStatusRequest
{
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus NewStatus { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TicketReference { get; set; }

    [Required]
    public bool IsRollback { get; set; }
}
