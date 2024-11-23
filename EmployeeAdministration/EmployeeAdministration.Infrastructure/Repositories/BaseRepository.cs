using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbSet<TEntity> _set;
    protected readonly IQueryable<TEntity> _untrackedSet;

    protected BaseRepository(AppDbContext dbContext)
    {
        _set = dbContext.Set<TEntity>();
        _untrackedSet = _set.AsNoTracking();
    }

    public async System.Threading.Tasks.Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Delete(TEntity entity)
        => _set.Remove(entity);
}

internal abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IBaseRepository<TEntity, TKey> 
    where TEntity : BaseEntity<TKey> 
    where TKey : IComparable<TKey>
{
    protected BaseRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<bool> DoesInstanceExistAsync(TKey id, CancellationToken cancellationToken = default)
        => await _set.FindAsync(id, cancellationToken) != null;

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => await _set.FindAsync(id, cancellationToken);
}

internal abstract class Repository<TEntity> : BaseRepository<TEntity, int>, IRepository<TEntity>
    where TEntity : Entity
{
    protected Repository(AppDbContext dbContext) : base(dbContext) { }
}