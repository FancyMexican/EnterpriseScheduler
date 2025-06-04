using AutoMapper;
using EnterpriseScheduler.Constants;
using EnterpriseScheduler.Exceptions;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;
using TimeZoneConverter;

namespace EnterpriseScheduler.Services;

public class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public MeetingService(IMeetingRepository meetingRepository, IUserRepository userRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<MeetingResponse>> GetMeetingsPaginated(int page, int pageSize)
    {
        if (page < PaginationConstants.MinPage) page = PaginationConstants.MinPage;
        if (pageSize < PaginationConstants.MinPageSize) pageSize = PaginationConstants.MinPageSize;
        if (pageSize > PaginationConstants.MaxPageSize) pageSize = PaginationConstants.MaxPageSize;

        var result = await _meetingRepository.GetPaginatedAsync(page, pageSize);

        return _mapper.Map<PaginatedResult<MeetingResponse>>(result);
    }

    public async Task<IEnumerable<MeetingResponse>> GetUserMeetings(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var timeZoneInfo = TZConvert.GetTimeZoneInfo(user.TimeZone);

        var meetings = await _meetingRepository.GetUserMeetings(userId);
        var meetingResponses = _mapper.Map<IEnumerable<MeetingResponse>>(meetings);

        foreach (var meeting in meetingResponses)
        {
            meeting.StartTime = TimeZoneInfo.ConvertTime(meeting.StartTime, timeZoneInfo);
            meeting.EndTime = TimeZoneInfo.ConvertTime(meeting.EndTime, timeZoneInfo);
        }

        return meetingResponses;
    }

    public async Task<MeetingResponse> GetMeeting(Guid id)
    {
        var meeting = await _meetingRepository.GetByIdAsync(id);

        return _mapper.Map<MeetingResponse>(meeting);
    }

    public async Task<MeetingResponse> CreateMeeting(MeetingRequest meetingRequest)
    {
        ConvertToUtc(meetingRequest);
        ValidateMeetingTimes(meetingRequest);

        var participants = await ValidateAndGetParticipants(meetingRequest.ParticipantIds);
        var participantIds = participants.Select(p => p.Id).ToList();

        var conflicts = await CheckForConflicts(meetingRequest.StartTime, meetingRequest.EndTime, participantIds);
        if (conflicts.Any())
        {
            var availableSlots = await FindAvailableSlots(meetingRequest.StartTime, meetingRequest.EndTime, participantIds);
            throw new MeetingConflictException(availableSlots);
        }

        var meeting = _mapper.Map<Meeting>(meetingRequest);
        meeting.Id = Guid.NewGuid();
        meeting.Participants = participants;

        await _meetingRepository.AddAsync(meeting);

        return _mapper.Map<MeetingResponse>(meeting);
    }

    public async Task<MeetingResponse> UpdateMeeting(Guid id, MeetingRequest meetingRequest)
    {
        ConvertToUtc(meetingRequest);
        ValidateMeetingTimes(meetingRequest);

        var existingMeeting = await _meetingRepository.GetByIdAsync(id);

        var participants = await ValidateAndGetParticipants(meetingRequest.ParticipantIds);
        var participantIds = participants.Select(p => p.Id).ToList();

        var conflicts = await CheckForConflicts(meetingRequest.StartTime, meetingRequest.EndTime, participantIds);
        if (conflicts.Any())
        {
            var availableSlots = await FindAvailableSlots(meetingRequest.StartTime, meetingRequest.EndTime, participantIds);
            throw new MeetingConflictException(availableSlots);
        }

        _mapper.Map(meetingRequest, existingMeeting);
        existingMeeting.Participants = participants;

        await _meetingRepository.UpdateAsync(existingMeeting);

        return _mapper.Map<MeetingResponse>(existingMeeting);
    }

    public async Task DeleteMeeting(Guid id)
    {
        var meeting = await _meetingRepository.GetByIdAsync(id);
        await _meetingRepository.DeleteAsync(meeting);
    }

    private void ConvertToUtc(MeetingRequest request)
    {
        request.StartTime = request.StartTime.ToUniversalTime();
        request.EndTime = request.EndTime.ToUniversalTime();
    }

    private void ValidateMeetingTimes(MeetingRequest meetingRequest)
    {
        if (meetingRequest.StartTime >= meetingRequest.EndTime)
        {
            throw new ArgumentException("Start time must be before end time.");
        }
    }

    private async Task<List<User>> ValidateAndGetParticipants(IEnumerable<Guid> participantIds)
    {
        if (!participantIds.Any())
        {
            throw new ArgumentException("At least one participant is required for a meeting.");
        }

        var participants = await _userRepository.GetByIdsAsync(participantIds);
        if (!participants.Any())
        {
            throw new ArgumentException("At least one valid participant ID is required.");
        }

        return participants.ToList();
    }

    private async Task<IEnumerable<Meeting>> CheckForConflicts(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<Guid> participantIds)
    {
        return await _meetingRepository.GetMeetingsInTimeRange(startTime, endTime, participantIds);
    }

    private async Task<IEnumerable<TimeSlot>> FindAvailableSlots(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<Guid> participantIds)
    {
        var duration = endTime - startTime;
        var availableSlots = new List<TimeSlot>();
        var searchEndTime = startTime.AddDays(7);

        var meetings = await GetOrderedMeetingsInRange(startTime, searchEndTime, participantIds);

        if (!meetings.Any())
        {
            return new[] { CreateTimeSlot(startTime, duration) };
        }

        FindSlotsBetweenMeetings(meetings, duration, availableSlots);

        if (availableSlots.Count < 3)
        {
            FindSlotsAfterLastMeeting(meetings.Last(), duration, availableSlots);
        }

        return availableSlots;
    }

    private async Task<List<Meeting>> GetOrderedMeetingsInRange(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<Guid> participantIds)
    {
        var meetings = await _meetingRepository.GetMeetingsInTimeRange(startTime, endTime, participantIds);
        return meetings.OrderBy(m => m.StartTime).ToList();
    }

    private void FindSlotsBetweenMeetings(List<Meeting> meetings, TimeSpan duration, List<TimeSlot> availableSlots)
    {
        for (int i = 0; i < meetings.Count - 1 && availableSlots.Count < 3; i++)
        {
            var currentMeeting = meetings[i];
            var nextMeeting = meetings[i + 1];

            FindConsecutiveSlotsInGap(currentMeeting.EndTime, nextMeeting.StartTime, duration, availableSlots);
        }
    }

    private void FindSlotsAfterLastMeeting(Meeting lastMeeting, TimeSpan duration, List<TimeSlot> availableSlots)
    {
        var currentSlotStart = lastMeeting.EndTime;
        while (availableSlots.Count < 3)
        {
            availableSlots.Add(CreateTimeSlot(currentSlotStart, duration));
            currentSlotStart = currentSlotStart.Add(duration);
        }
    }

    private void FindConsecutiveSlotsInGap(DateTimeOffset gapStart, DateTimeOffset gapEnd, TimeSpan duration, List<TimeSlot> availableSlots)
    {
        var currentSlotStart = gapStart;
        while (currentSlotStart.Add(duration) <= gapEnd && availableSlots.Count < 3)
        {
            availableSlots.Add(CreateTimeSlot(currentSlotStart, duration));
            currentSlotStart = currentSlotStart.Add(duration);
        }
    }

    private TimeSlot CreateTimeSlot(DateTimeOffset startTime, TimeSpan duration)
    {
        return new TimeSlot
        {
            StartTime = startTime.ToUniversalTime(),
            EndTime = startTime.Add(duration).ToUniversalTime()
        };
    }
}
