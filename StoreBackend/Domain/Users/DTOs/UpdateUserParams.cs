namespace Domain.Users;

public class UpdateUserParams
{
    public required Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? UpdatedById { get; set; }
}
