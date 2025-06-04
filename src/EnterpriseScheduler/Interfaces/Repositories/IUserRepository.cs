using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;

namespace EnterpriseScheduler.Interfaces.Repositories;

public interface IUserRepository
{
    Task<PaginatedResult<User>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<User> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}
