namespace Domain.Users;

public class CreateUserParams
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public string? CreatedById { get; set; }
}
