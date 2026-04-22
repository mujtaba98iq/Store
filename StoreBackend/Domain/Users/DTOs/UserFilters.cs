using Sheard.Type;

namespace Domain.Users;

public class UserFilters : ListingOptions
{
    public string? Username { get; set; }
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public UserOrderBy? OrderBy { get; set; } = UserOrderBy.CreatedAt;
}
