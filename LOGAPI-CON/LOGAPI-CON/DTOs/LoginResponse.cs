namespace LOGAPI_CON.DTOs
{
    public sealed class LoginResponse
    {
        public required string AccessToken { get; init; }
        public required string TokenType { get; init; }
        public required DateTime ExpiresAtUtc  { get; init; }
        public required string Username { get; init; }
        public required string Role { get; init; }
    }
}
