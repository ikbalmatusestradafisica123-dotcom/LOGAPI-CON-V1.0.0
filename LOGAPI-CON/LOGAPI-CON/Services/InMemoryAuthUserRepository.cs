
using System.Collections.Concurrent;
using LOGAPI_CON.Models;
using Microsoft.AspNetCore.Identity;

namespace LOGAPI_CON.Services;

public sealed class InMemoryAuthUserRepository : IAuthUserRepository
{
    private readonly ConcurrentDictionary<string, AuthUser> _users =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly IPasswordHasher<AuthUser> _passwordHasher;

    public InMemoryAuthUserRepository(
        IConfiguration configuration,
        IPasswordHasher<AuthUser> passwordHasher)
    {
        _passwordHasher = passwordHasher;

        var username = configuration["Auth:DemoUser:Username"];
        var password = configuration["Auth:DemoUser:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Faltan las credenciales locales en User Secrets.");
        }

        var demoUser = new AuthUser
        {
            Id = "local-demo-user",
            Username = username.Trim(),
            Role = "Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        demoUser.PasswordHash =
            _passwordHasher.HashPassword(demoUser, password);

        _users.TryAdd(demoUser.Username, demoUser);
    }

    public Task<AuthUser?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _users.TryGetValue(username.Trim(), out var user);

        return Task.FromResult(
            user is null ? null : Copy(user));
    }

    public Task<AuthUser?> FindByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = _users.Values.FirstOrDefault(
            item => item.Id == id);

        return Task.FromResult(
            user is null ? null : Copy(user));
    }

    public Task<IReadOnlyList<AuthUser>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<AuthUser> result = _users.Values
            .Select(Copy)
            .OrderBy(user => user.Username)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<bool> CreateAsync(
        AuthUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var newUser = Copy(user);
        newUser.Username = newUser.Username.Trim();

        var created = _users.TryAdd(newUser.Username, newUser);

        return Task.FromResult(created);
    }

    public Task<bool> UpdateAsync(AuthUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var username = user.Username.Trim();

        if (!_users.TryGetValue(username, out var current))
            return Task.FromResult(false);

        if (current.Id != user.Id)
            return Task.FromResult(false);

        var updated = _users.TryUpdate(
            username,
            Copy(user),
            current);

        return Task.FromResult(updated);
    }
    private static AuthUser Copy(AuthUser user)
    {
        return new AuthUser
        {
            Id = user.Id,
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
