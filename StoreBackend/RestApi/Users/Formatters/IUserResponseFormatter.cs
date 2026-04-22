using Domain.Users;

namespace RestApi.Users;

public interface IUserResponseFormatter
{
    UserResponse One(User user);
    UserListResponse Many(IEnumerable<User> users, int totalCount);
}
