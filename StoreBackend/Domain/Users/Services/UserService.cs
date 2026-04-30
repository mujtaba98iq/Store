using Domain.Exeptions;
using Sheard.Type;

namespace Domain.Users;

public class UserService(IUsersRepository usersRepository) : IUserService
{
    public async Task<User> Create(CreateUserParams createUserParams)
    {
        var existsUser = usersRepository.FindByUsername(createUserParams.Username);

        if (existsUser != null) 
            throw new ResourceAlreadyExistsException(createUserParams.Username);

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

        user.Username = updateUserParams.Username ?? user.Username;
        user.Password = updateUserParams.Password != null ? BCrypt.Net.BCrypt.HashPassword(updateUserParams.Password) : user.Password;
        user.Role = updateUserParams.Role ?? user.Role;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedById = updateUserParams.UpdatedById ?? "1";

        return await usersRepository.Update(user);
    }
}
