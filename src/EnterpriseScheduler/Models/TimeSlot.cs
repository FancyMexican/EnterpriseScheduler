namespace EnterpriseScheduler.Models;

public class TimeSlot
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public override string ToString()
    {
        return $"{StartTime:g} UTC - {EndTime:g} UTC";
    }
}
