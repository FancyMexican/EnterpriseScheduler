using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Users;

namespace EnterpriseScheduler.Interfaces.Services;

public interface IUserService
{
    Task<PaginatedResult<UserResponse>> GetUsersPaginated(int page, int pageSize);
    Task<UserResponse> GetUser(Guid id);
    Task<UserResponse> CreateUser(UserRequest user);
    Task<UserResponse> UpdateUser(Guid id, UserRequest user);
    Task DeleteUser(Guid id);
}
