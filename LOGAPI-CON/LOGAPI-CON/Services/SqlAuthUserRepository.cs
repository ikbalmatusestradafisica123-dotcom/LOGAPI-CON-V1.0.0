using LOGAPI_CON.Data;
using LOGAPI_CON.Models;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Services;

public sealed class SqlAuthUserRepository : IAuthUserRepository
{
    private readonly AppDbContext _db;

    public SqlAuthUserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthUser?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();

        return await _db.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Username == normalizedUsername,
                cancellationToken);
    }

    public async Task<AuthUser?> FindByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _db.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AuthUser>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.AuthUsers
            .AsNoTracking()
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CreateAsync(
        AuthUser user,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = user.Username.Trim();

        var exists = await _db.AuthUsers.AnyAsync(
            item => item.Username == normalizedUsername,
            cancellationToken);

        if (exists)
        {
            return false;
        }

        user.Username = normalizedUsername;

        _db.AuthUsers.Add(user);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(
        AuthUser user,
        CancellationToken cancellationToken = default)
    {
        var current = await _db.AuthUsers
            .FirstOrDefaultAsync(
                item => item.Id == user.Id,
                cancellationToken);

        if (current is null)
        {
            return false;
        }

        current.Username = user.Username.Trim();
        current.Role = user.Role;
        current.IsActive = user.IsActive;
        current.PasswordHash = user.PasswordHash;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
