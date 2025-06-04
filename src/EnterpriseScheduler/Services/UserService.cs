using AutoMapper;
using EnterpriseScheduler.Constants;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Users;
using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<UserResponse>> GetUsersPaginated(int page, int pageSize)
        {
            if (page < PaginationConstants.MinPage) page = PaginationConstants.MinPage;
            if (pageSize < PaginationConstants.MinPageSize) pageSize = PaginationConstants.MinPageSize;
            if (pageSize > PaginationConstants.MaxPageSize) pageSize = PaginationConstants.MaxPageSize;

            var result = await _userRepository.GetPaginatedAsync(page, pageSize);

            return _mapper.Map<PaginatedResult<UserResponse>>(result);
        }

        public async Task<UserResponse> GetUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> CreateUser(UserRequest userRequest)
        {
            var user = _mapper.Map<User>(userRequest);
            user.Id = Guid.NewGuid();

            await _userRepository.AddAsync(user);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> UpdateUser(Guid id, UserRequest userRequest)
        {
            var user = await _userRepository.GetByIdAsync(id);
            _mapper.Map(userRequest, user);

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task DeleteUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            await _userRepository.DeleteAsync(user);
        }
    }
}
