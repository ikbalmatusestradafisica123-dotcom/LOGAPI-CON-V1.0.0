using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LOGAPI_CON.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IAuthUserRepository _users;
        private readonly IPasswordHasher<AuthUser> _passwordHasher;
        private readonly IConfiguration _configuration;


        public AuthService(IAuthUserRepository users, IPasswordHasher<AuthUser> passwordHasher, IConfiguration configuration)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _users.FindByUsernameAsync(request.Username.Trim(), cancellationToken);

            if (user is null || !user.IsActive)
                return null;

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
                return null;

            var issuer = _configuration["Auth:Jwt:Issuer"];
            var audience = _configuration["Auth:Jwt:Audience"];
            var signingKey = _configuration["Auth:Jwt:SigningKey"];

            if(string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("Falta la configuracion JWT en User Secrets.");
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, expires: expiresAt, signingCredentials: credentials);

            return new LoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAt,
                Username = user.Username,
                Role = user.Role
            };
        }
    }
}
