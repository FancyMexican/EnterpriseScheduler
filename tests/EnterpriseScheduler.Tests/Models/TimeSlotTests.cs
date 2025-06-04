using System;
using EnterpriseScheduler.Models;
using Xunit;

namespace EnterpriseScheduler.Tests.Models
{
    public class TimeSlotTests
    {
        [Fact]
        public void ToString_ReturnsExpectedFormat()
        {
            var start = new DateTimeOffset(2025, 6, 4, 9, 0, 0, TimeSpan.Zero);
            var end = new DateTimeOffset(2025, 6, 4, 10, 0, 0, TimeSpan.Zero);
            var slot = new TimeSlot { StartTime = start, EndTime = end };
            var expected = $"{start:g} UTC - {end:g} UTC";
            Assert.Equal(expected, slot.ToString());
        }

        [Fact]
        public void Properties_CanBeSetAndRetrieved()
        {
            var start = DateTimeOffset.Now;
            var end = start.AddHours(1);
            var slot = new TimeSlot { StartTime = start, EndTime = end };
            Assert.Equal(start, slot.StartTime);
            Assert.Equal(end, slot.EndTime);
        }
    }
}
