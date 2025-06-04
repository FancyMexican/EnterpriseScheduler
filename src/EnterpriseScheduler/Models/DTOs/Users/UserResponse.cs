namespace EnterpriseScheduler.Models.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TimeZone { get; set; }
}
