namespace EnterpriseScheduler.Models;

public class Meeting
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }

    public ICollection<User> Participants { get; set; } = new List<User>();
}
