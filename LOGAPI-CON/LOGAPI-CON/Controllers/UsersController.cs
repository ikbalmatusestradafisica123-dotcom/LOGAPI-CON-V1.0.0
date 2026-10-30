
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Models;
using LOGAPI_CON.Security;
using LOGAPI_CON.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AppPolicies.CanManageUsers)]
public class UsersController : ControllerBase
{
    private readonly IAuthUserRepository _users;
    private readonly IPasswordHasher<AuthUser> _passwordHasher;

    public UsersController(
        IAuthUserRepository users,
        IPasswordHasher<AuthUser> passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        return Ok(users.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFound(new { message = "Usuario no encontrado." });

        return Ok(ToResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        if (string.IsNullOrWhiteSpace(username))
            return BadRequest(new { message = "El usuario es obligatorio." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "La contraseña es obligatoria." });

        if (!IsKnownRole(request.Role))
            return BadRequest(new { message = "El rol indicado no existe." });

        if (!CanAssignRole(request.Role))
            return Forbid();

        var existing = await _users.FindByUsernameAsync(
            username, cancellationToken);

        if (existing is not null)
            return Conflict(new { message = "El nombre de usuario ya existe." });

        var user = new AuthUser
        {
            Id = Guid.NewGuid().ToString("N"),
            Username = username,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user, request.Password);

        var created = await _users.CreateAsync(user, cancellationToken);

        if (!created)
            return Conflict(new { message = "No se pudo crear el usuario." });

        var response = ToResponse(user);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            response);
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult<UserResponse>> UpdateRole(
        string id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsKnownRole(request.Role))
            return BadRequest(new { message = "El rol indicado no existe." });

        if (!CanAssignRole(request.Role))
            return Forbid();

        var user = await _users.FindByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFound(new { message = "Usuario no encontrado." });

        if (user.Id == CurrentUserId())
            return BadRequest(new
            {
                message = "No puedes modificar tu propio rol."
            });

        user.Role = request.Role;

        var updated = await _users.UpdateAsync(user, cancellationToken);

        if (!updated)
            return NotFound(new { message = "No se pudo actualizar el usuario." });

        return Ok(ToResponse(user));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<UserResponse>> UpdateStatus(
        string id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFound(new { message = "Usuario no encontrado." });

        if (user.Id == CurrentUserId())
            return BadRequest(new
            {
                message = "No puedes desactivar tu propia cuenta."
            });

        if (!request.IsActive && user.Role == AppRoles.IT)
        {
            var allUsers = await _users.GetAllAsync(cancellationToken);

            var activeIT = allUsers.Count(u =>
                u.Role == AppRoles.IT && u.IsActive);

            if (activeIT <= 1)
                return Conflict(new
                {
                    message = "No puedes desactivar la última cuenta activa de IT."
                });
        }

        user.IsActive = request.IsActive;

        var updated = await _users.UpdateAsync(user, cancellationToken);

        if (!updated)
            return NotFound(new { message = "No se pudo actualizar el usuario." });

        return Ok(ToResponse(user));
    }

    private bool CanAssignRole(string role)
    {
        if (User.IsInRole(AppRoles.IT))
            return role != AppRoles.Owner;

        if (User.IsInRole(AppRoles.Owner))
            return role != AppRoles.Owner && role != AppRoles.IT;

        return User.IsInRole(AppRoles.Administrator)
            && role != AppRoles.IT
            && role != AppRoles.Administrator
            && role != AppRoles.Owner;
    }

    private static bool IsKnownRole(string role)
    {
        return AppRoles.All.Contains(role, StringComparer.Ordinal);
    }

    private string? CurrentUserId()
    {
        return User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    private static UserResponse ToResponse(AuthUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAt
        };
    }
}
