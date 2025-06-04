using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;

namespace EnterpriseScheduler.Interfaces.Services;

public interface IMeetingService
{
    Task<PaginatedResult<MeetingResponse>> GetMeetingsPaginated(int page, int pageSize);
    Task<IEnumerable<MeetingResponse>> GetUserMeetings(Guid userId);
    Task<MeetingResponse> GetMeeting(Guid id);
    Task<MeetingResponse> CreateMeeting(MeetingRequest meetingRequest);
    Task<MeetingResponse> UpdateMeeting(Guid id, MeetingRequest meetingRequest);
    Task DeleteMeeting(Guid id);
}
