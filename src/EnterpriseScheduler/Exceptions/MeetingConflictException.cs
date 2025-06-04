using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Exceptions;

public class MeetingConflictException : Exception
{
    public IEnumerable<TimeSlot> AvailableSlots { get; }

    public MeetingConflictException(IEnumerable<TimeSlot> availableSlots)
        : base($"Conflicts with existing meetings. Here are the next available slots in the next 7 days (in UTC):\n{string.Join("\n", availableSlots.Select(s => $"- {s.StartTime:yyyy-MM-ddTHH:mm:sszzz} to {s.EndTime:yyyy-MM-ddTHH:mm:sszzz}"))}")
    {
        AvailableSlots = availableSlots;
    }
}
