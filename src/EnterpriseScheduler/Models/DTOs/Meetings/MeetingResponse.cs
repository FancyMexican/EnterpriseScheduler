namespace EnterpriseScheduler.Models.DTOs.Meetings;

public class MeetingResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }
    public ICollection<Guid> ParticipantIds { get; set; } = new List<Guid>();
}
