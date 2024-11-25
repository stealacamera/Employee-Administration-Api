namespace EmployeeAdministration.Domain.Entities;

public class Project : Entity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}