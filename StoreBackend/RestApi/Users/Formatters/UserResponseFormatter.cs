using Domain.Users;

namespace RestApi.Users;

public class UserResponseFormatter : IUserResponseFormatter
{
    public UserListResponse Many(IEnumerable<User> users, int totalCount)
    {
        var userResults = users.Select(One).ToList();

        return new UserListResponse
        {
            Data = userResults,
            TotalCount = totalCount
        };
    }

    public UserResponse One(User user)
    {
        return new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedById = user.CreatedById,
            UpdatedById = user.UpdatedById
        };
    }
}
