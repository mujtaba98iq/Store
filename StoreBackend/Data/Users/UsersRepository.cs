using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Users;

public class UsersRepository(ApplicationDbContext dbContext) : IUsersRepository
{
    public async Task<User> Create(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<List<User>> FindByFilters(UserFilters userFilters)
    {
        var query = dbContext.Users.AsNoTracking()
            .Where(g => g.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, userFilters);
        query = ApplyOrdering(query, userFilters);
        query = ApplyPagination(query, userFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<User> ApplyPagination(IQueryable<User> query, UserFilters userFilters)
    {
        var page = userFilters.Page <= 0 ? 1 : userFilters.Page;
        var pageSize = userFilters.PageSize <= 0 ? 10 : userFilters.PageSize;
        var skip = (page - 1) * pageSize;
        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<User> ApplyOrdering(IQueryable<User> query, UserFilters userFilters)
    {
        var userOrderBy = userFilters.OrderBy ?? UserOrderBy.CreatedAt;
        var orderDirection = userFilters.OrderByDirection ?? OrderDirection.Desc;

        return userOrderBy switch
        {
            UserOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(g => g.CreatedAt)
                : query.OrderByDescending(g => g.CreatedAt),
            UserOrderBy.Username => orderDirection == OrderDirection.Asc
                ? query.OrderBy(g => g.Username)
                : query.OrderByDescending(g => g.Username),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static IQueryable<User> ApplyFilters(IQueryable<User> query, UserFilters userFilters)
    {
        if (userFilters.UserId != null)
        {
            query = query.Where(p => p.Id == userFilters.UserId);
        }

        if (!string.IsNullOrEmpty(userFilters.Username))
        {
            query = query.Where(c => EF.Functions.Like(c.Username.ToLower(), $"%{userFilters.Username.ToLower()}%"));
        }

        if (!string.IsNullOrEmpty(userFilters.Role))
        {
            query = query.Where(c => EF.Functions.Like(c.Role.ToLower(), $"%{userFilters.Role.ToLower()}%"));
        }

        return query;
    }

    public async Task<User?> FindById(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        return user;
    }

    public async Task<User> Update(User user)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<int> GetTotalCountByFilters(UserFilters userFilters)
    {
        var query = dbContext.Users.AsNoTracking().Where(g => g.DeletedAt == null).AsQueryable();
        query = ApplyFilters(query, userFilters);
        return await query.CountAsync();
    }

    public async Task<User?> FindByUsername(string username)
    {
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null);
    }
}
