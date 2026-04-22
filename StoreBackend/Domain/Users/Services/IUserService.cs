using Sheard.Type;

namespace Domain.Users;

public interface IUserService
{
    Task<User> Create(CreateUserParams createUserParams);
    Task<User> Update(UpdateUserParams updateUserParams);
    Task<User?> FindById(Guid id);
    Task<PaginationResult<User>> Search(UserFilters userFilters);
}
