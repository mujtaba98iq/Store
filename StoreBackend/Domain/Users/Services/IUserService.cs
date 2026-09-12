using Sheard.Type;

namespace Domain.Users;

public interface IUserService
{
    Task<User> Create(CreateUserParams createUserParams);
    Task<User> Update(UpdateUserParams updateUserParams);
    Task<User?> FindById(Guid id);
    Task<PaginationResult<User>> Search(UserFilters userFilters);

    /// <summary>Soft-deletes a user. False when it does not exist, or was already deleted.</summary>
    Task<bool> Delete(DeleteUserParams deleteUserParams);
}
