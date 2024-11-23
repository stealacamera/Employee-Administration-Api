using EmployeeAdministration.Domain.Entities;

namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    System.Threading.Tasks.Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Delete(TEntity entity);
}

public interface IBaseRepository<TEntity, TKey> : IBaseRepository<TEntity>
    where TEntity : BaseEntity<TKey>
    where TKey : IComparable<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<bool> DoesInstanceExistAsync(TKey id, CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity> : IBaseRepository<TEntity, int> where TEntity : Entity
{
}