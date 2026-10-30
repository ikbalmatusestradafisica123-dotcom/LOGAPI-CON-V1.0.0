using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs;

public sealed class CreateUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 12)]
    public string Password {  get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
