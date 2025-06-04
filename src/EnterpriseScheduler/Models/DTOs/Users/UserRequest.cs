using EnterpriseScheduler.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseScheduler.Models.DTOs.Users;

public class UserRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    [ValidTimeZone]
    public required string TimeZone { get; set; }
}
