namespace Domain.Users;

public class DeleteUserParams
{
    public required Guid Id { get; set; }
    public required string DeletedById { get; set; }
}
