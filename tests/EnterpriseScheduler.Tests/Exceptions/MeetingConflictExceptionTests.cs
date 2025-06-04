using EnterpriseScheduler.Exceptions;
using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Tests.Exceptions
{
    public class MeetingConflictExceptionTests
    {
        [Fact]
        public void Constructor_SetsAvailableSlotsAndMessage()
        {
            // Arrange
            var slots = new List<TimeSlot>
            {
                new TimeSlot { StartTime = new DateTimeOffset(2025, 6, 4, 10, 0, 0, TimeSpan.Zero), EndTime = new DateTimeOffset(2025, 6, 4, 11, 0, 0, TimeSpan.Zero) },
                new TimeSlot { StartTime = new DateTimeOffset(2025, 6, 5, 14, 0, 0, TimeSpan.Zero), EndTime = new DateTimeOffset(2025, 6, 5, 15, 0, 0, TimeSpan.Zero) }
            };

            // Act
            var ex = new MeetingConflictException(slots);

            // Assert
            Assert.Equal(slots, ex.AvailableSlots);
            foreach (var slot in slots)
            {
                var expected = $"- {slot.StartTime:yyyy-MM-ddTHH:mm:sszzz} to {slot.EndTime:yyyy-MM-ddTHH:mm:sszzz}";
                Assert.Contains(expected, ex.Message);
            }
            Assert.Contains("Conflicts with existing meetings", ex.Message);
        }
    }
}
