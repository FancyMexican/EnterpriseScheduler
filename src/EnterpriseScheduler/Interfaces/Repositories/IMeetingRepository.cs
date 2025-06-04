using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;

namespace EnterpriseScheduler.Interfaces.Repositories;

public interface IMeetingRepository
{
    Task<PaginatedResult<Meeting>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Meeting>> GetUserMeetings(Guid userId);
    Task<IEnumerable<Meeting>> GetMeetingsInTimeRange(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<Guid> participantIds);
    Task<Meeting> GetByIdAsync(Guid id);
    Task AddAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Meeting meeting);
}
