using Microsoft.EntityFrameworkCore;
using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Models.Common;

namespace EnterpriseScheduler.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<User>> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Users.AsNoTracking();
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

        return user ?? throw new KeyNotFoundException($"User with ID {id} not found");
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        // Find and delete meetings where this user is the only participant
        var meetingsToDelete = await _context.Meetings
            .Where(m => m.Participants.Count == 1 && m.Participants.Any(p => p.Id == user.Id))
            .ToListAsync();
        _context.Meetings.RemoveRange(meetingsToDelete);

        // Delete the user
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
