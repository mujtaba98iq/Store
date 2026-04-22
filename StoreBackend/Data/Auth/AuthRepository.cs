using Domain.Auth;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Data.Auth;

public class AuthRepository(ApplicationDbContext dbContext) : IAuthRepository
{
    public async Task<User?> FindByUsername(string username)
    {
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null);
    }
}
