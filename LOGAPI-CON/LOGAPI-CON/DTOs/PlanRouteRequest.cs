using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs;

public class PlanRouteRequest
{
    [Required]
    public DateTime Date { get; set; }
    [Required]
    [Range(typeof(decimal), "0.01", "9999999999" )]
    public int MaxTonnagePerDay { get; set; }
}
