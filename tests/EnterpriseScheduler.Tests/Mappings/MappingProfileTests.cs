using AutoMapper;
using EnterpriseScheduler.Mappings;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;
using EnterpriseScheduler.Models.DTOs.Users;

namespace EnterpriseScheduler.Tests.Mappings
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void UserRequest_To_User_MapsCorrectly()
        {
            // Arrange
            var request = new UserRequest
            {
                Name = "Alice",
                TimeZone = "UTC"
            };

            // Act
            var user = _mapper.Map<User>(request);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(request.TimeZone, user.TimeZone);
            Assert.Equal(request.Name, user.Name);
        }

        [Fact]
        public void User_To_UserResponse_MapsCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Bob",
                TimeZone = "Europe/Berlin"
            };

            // Act
            var response = _mapper.Map<UserResponse>(user);

            // Assert
            Assert.Equal(user.Id, response.Id);
            Assert.Equal(user.Name, response.Name);
            Assert.Equal(user.TimeZone, response.TimeZone);
        }

        [Fact]
        public void MeetingRequest_To_Meeting_MapsCorrectly()
        {
            // Arrange
            var request = new MeetingRequest
            {
                Title = "Test Meeting",
                StartTime = new DateTimeOffset(2025, 6, 4, 10, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 6, 4, 11, 0, 0, TimeSpan.Zero),
                ParticipantIds = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var meeting = _mapper.Map<Meeting>(request);

            // Assert
            Assert.Equal(request.Title, meeting.Title);
            Assert.Equal(request.StartTime, meeting.StartTime);
            Assert.Equal(request.EndTime, meeting.EndTime);
        }

        [Fact]
        public void Meeting_To_MeetingResponse_MapsParticipantIds()
        {
            // Arrange
            var participant1 = new User
            {
                Id = Guid.NewGuid(),
                Name = "Charlie",
                TimeZone = "UTC"
            };
            var participant2 = new User
            {
                Id = Guid.NewGuid(),
                Name = "Dana",
                TimeZone = "UTC"
            };
            var meeting = new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Sync",
                StartTime = new DateTimeOffset(2025, 6, 4, 12, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2025, 6, 4, 13, 0, 0, TimeSpan.Zero),
                Participants = new List<User> { participant1, participant2 }
            };

            // Act
            var response = _mapper.Map<MeetingResponse>(meeting);

            // Assert
            Assert.Equal(meeting.Id, response.Id);
            Assert.Equal(meeting.Title, response.Title);
            Assert.Equal(meeting.StartTime, response.StartTime);
            Assert.Equal(meeting.EndTime, response.EndTime);
            Assert.Equal(new[] { participant1.Id, participant2.Id }, response.ParticipantIds);
        }

        [Fact]
        public void PaginatedResult_Maps_GenericTypes()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var paged = new PaginatedResult<User>
            {
                Items = new List<User>
                {
                    new User
                    {
                        Id = userId,
                        Name = "A",
                        TimeZone = "UTC"
                    }
                },
                TotalCount = 1,
                PageNumber = 2,
                PageSize = 10
            };

            // Act
            var result = _mapper.Map<PaginatedResult<UserResponse>>(paged);

            // Assert
            Assert.Single(result.Items);
            var mappedUser = result.Items.First();
            Assert.Equal(userId, mappedUser.Id);
            Assert.Equal("A", mappedUser.Name);
            Assert.Equal("UTC", mappedUser.TimeZone);
            Assert.Equal(paged.TotalCount, result.TotalCount);
            Assert.Equal(paged.PageNumber, result.PageNumber);
            Assert.Equal(paged.PageSize, result.PageSize);
        }
    }
}
