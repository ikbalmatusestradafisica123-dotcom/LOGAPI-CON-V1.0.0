using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.DTOs
{
    public sealed class LoginRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Username { get; init; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Password {  get; init; } = string.Empty; 
    }
}
