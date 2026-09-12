using Domain.Exeptions;
using Sheard.Type;

namespace Domain.Users;

public class UserService(IUsersRepository usersRepository) : IUserService
{
    public async Task<User> Create(CreateUserParams createUserParams)
    {
        var existsUser = await usersRepository.FindByUsername(createUserParams.Username);

        if (existsUser != null) 
            throw new ResourceAlreadyExistsException("User", $"username {createUserParams.Username}");

        var password = BCrypt.Net.BCrypt.HashPassword(createUserParams.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = createUserParams.Username,
            Password = password,
            Role = createUserParams.Role,
            CreatedAt = DateTime.UtcNow,
            CreatedById = createUserParams.CreatedById,
            RefreshTokenHash = string.Empty,
            RefreshTokenExpiresAt = null,
            RefreshTokenRevokeAt = null
        };

        user.CreatedById = createUserParams.CreatedById ?? "1";
        return await usersRepository.Create(user);
    }

    public async Task<User?> FindById(Guid id)
    {
        return await usersRepository.FindById(id);
    }

    public async Task<PaginationResult<User>> Search(UserFilters userFilters)
    {
        var users = await usersRepository.FindByFilters(userFilters);
        var totalCount = await usersRepository.GetTotalCountByFilters(userFilters);

        return new PaginationResult<User>
        {
            TotalCount = totalCount,
            Data = users
        };
    }

    public async Task<User> Update(UpdateUserParams updateUserParams)
    {
        var user = await usersRepository.FindById(updateUserParams.Id) ?? throw new ResourceNotFoundException("User", $"User with ID {updateUserParams.Id} not found");

        // Renaming onto a username somebody else holds has to be refused here: the
        // login looks accounts up by username, so two of them could not be told apart.
        if (updateUserParams.Username is not null && updateUserParams.Username != user.Username)
        {
            var clash = await usersRepository.FindByUsername(updateUserParams.Username);

            if (clash is not null && clash.Id != user.Id)
                throw new ResourceAlreadyExistsException("User", $"username {updateUserParams.Username}");
        }

        user.Username = updateUserParams.Username ?? user.Username;
        user.Password = updateUserParams.Password != null ? BCrypt.Net.BCrypt.HashPassword(updateUserParams.Password) : user.Password;
        user.Role = updateUserParams.Role ?? user.Role;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedById = updateUserParams.UpdatedById ?? "1";

        return await usersRepository.Update(user);
    }

    /// <inheritdoc />
    public async Task<bool> Delete(DeleteUserParams deleteUserParams)
    {
        // FindById reaches rows that were already soft-deleted, so a second delete
        // has to be turned away rather than silently restamped.
        var user = await usersRepository.FindById(deleteUserParams.Id);
        if (user is null || user.DeletedAt is not null)
        {
            return false;
        }

        // Soft delete: the rows this account created keep pointing at it, and every
        // listing already skips rows with a DeletedAt.
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedById = deleteUserParams.DeletedById;

        await usersRepository.Update(user);
        return true;
    }
}
