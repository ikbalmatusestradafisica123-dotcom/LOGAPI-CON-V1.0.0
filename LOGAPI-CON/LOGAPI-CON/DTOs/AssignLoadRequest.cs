using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs;

public sealed class AssignLoadRequest
{
    [Required]
    public DateTime RouteDate { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> OrderIds { get; set; } = [];
}
