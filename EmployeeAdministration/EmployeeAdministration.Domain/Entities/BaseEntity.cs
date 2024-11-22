using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class BaseEntity<TKey> : BaseEntity where TKey : IComparable<TKey>
{
    public TKey Id { get; set; }
}

public abstract class Entity: BaseEntity<int>
{
}