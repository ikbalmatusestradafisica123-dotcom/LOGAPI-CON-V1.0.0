using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services
{
    public interface IAuthUserRepository
    {
        Task<AuthUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AuthUser?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuthUser>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> CreateAsync(AuthUser user, CancellationToken cancelationToken = default);
        Task<bool> UpdateAsync(AuthUser user, CancellationToken cancellationToken = default);
    }
}
