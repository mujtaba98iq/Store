namespace Domain.Users;

public interface IUsersRepository
{
    Task<User> Create(User user);
    Task<User> Update(User user);
    Task<User?> FindById(Guid id);
    Task<List<User>> FindByFilters(UserFilters userFilters);
    Task<int> GetTotalCountByFilters(UserFilters userFilters);
    Task<User?> FindByUsername(string username);
}
