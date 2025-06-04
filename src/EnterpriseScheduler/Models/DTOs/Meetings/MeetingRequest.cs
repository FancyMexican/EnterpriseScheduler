using System.ComponentModel.DataAnnotations;

namespace EnterpriseScheduler.Models.DTOs.Meetings;

public class MeetingRequest
{
    [Required]
    [StringLength(120)]
    public required string Title { get; set; }

    [Required]
    public required DateTimeOffset StartTime { get; set; }

    [Required]
    public required DateTimeOffset EndTime { get; set; }

    [Required]
    public ICollection<Guid> ParticipantIds { get; set; } = new List<Guid>();
}
