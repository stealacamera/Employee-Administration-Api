using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public abstract class BaseEntity
{
    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class BaseEntity<TKey> : BaseEntity where TKey : IComparable<TKey>
{
    [Key]
    public TKey Id { get; set; } = default!;
}

public abstract class Entity: BaseEntity<int>
{
}