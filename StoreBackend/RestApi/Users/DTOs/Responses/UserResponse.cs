namespace RestApi.Users;

public class UserResponse
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
