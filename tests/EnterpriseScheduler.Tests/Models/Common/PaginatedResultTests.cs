using System.Collections.Generic;
using EnterpriseScheduler.Models.Common;
using Xunit;

namespace EnterpriseScheduler.Tests.Models.Common
{
    public class PaginatedResultTests
    {
        [Fact]
        public void TotalPages_CalculatesCorrectly()
        {
            var result = new PaginatedResult<string>
            {
                TotalCount = 25,
                PageSize = 10
            };
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void Items_CanBeSetAndRetrieved()
        {
            var items = new List<string> { "a", "b", "c" };
            var result = new PaginatedResult<string> { Items = items };
            Assert.Equal(items, result.Items);
        }
    }
}
