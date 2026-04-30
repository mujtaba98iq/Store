using Domain.Data;

namespace Domain.Users;

public class User : IAuditableEntity
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public required string RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenRevokeAt { get; set; }

    public DateTime CreatedAt  { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById  { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }
}
