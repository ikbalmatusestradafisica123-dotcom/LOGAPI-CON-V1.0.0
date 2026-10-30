using LOGAPI_CON.DTOs;

namespace LOGAPI_CON.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
