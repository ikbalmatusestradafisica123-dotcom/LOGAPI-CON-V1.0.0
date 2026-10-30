using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs;
public class UpdateUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
