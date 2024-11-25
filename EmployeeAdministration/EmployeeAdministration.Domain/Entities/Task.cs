namespace EmployeeAdministration.Domain.Entities;

public class Task : Entity
{
    public int ProjectId { get; set; }
    public int AppointeeEmployeeId { get; set; }
    public int AppointerUserId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;
}
