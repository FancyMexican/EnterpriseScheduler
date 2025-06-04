using System.ComponentModel.DataAnnotations;
using EnterpriseScheduler.Models.Validation;

namespace EnterpriseScheduler.Tests.Models.Validation
{
    public class ValidTimeZoneAttributeTests
    {
        private readonly ValidTimeZoneAttribute _attribute = new ValidTimeZoneAttribute();

        [Theory]
        [InlineData("America/New_York")]
        [InlineData("Europe/London")]
        [InlineData("Asia/Tokyo")]
        [InlineData("UTC")]
        public void IsValid_ValidTimeZones_ReturnsSuccess(string timeZoneId)
        {
            var result = _attribute.GetValidationResult(timeZoneId, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsValid_NullOrEmpty_ReturnsSuccess(string? timeZoneId)
        {
            var result = _attribute.GetValidationResult(timeZoneId, new ValidationContext(new object()));
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData("Not/AZone")]
        [InlineData("Fake/Zone")]
        [InlineData("InvalidTimeZone")]
        public void IsValid_InvalidTimeZones_ReturnsError(string timeZoneId)
        {
            var result = _attribute.GetValidationResult(timeZoneId, new ValidationContext(new object()));
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Contains("not a valid timezone identifier", result!.ErrorMessage);
        }
    }
}
