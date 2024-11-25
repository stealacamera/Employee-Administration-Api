using EmployeeAdministration.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Application.Services;

internal abstract class BaseService
{
    protected static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
    protected readonly IWorkUnit _workUnit;

    protected BaseService(IWorkUnit workUnit)
        => _workUnit = workUnit;

    protected BaseService(IServiceProvider serviceProvider)
        =>_workUnit = serviceProvider.GetRequiredService<IWorkUnit>();

    protected async Task WrapInTransactionAsync(Func<Task> asyncFunc)
    {
        await _asyncLock.WaitAsync();

        using var transaction = await _workUnit.BeginTransactionAsync();

        try
        {
            await asyncFunc();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    protected async Task<T> WrapInTransactionAsync<T>(Func<Task<T>> asyncFunc)
    {
        await _asyncLock.WaitAsync();

        using var transaction = await _workUnit.BeginTransactionAsync();
        T result;

        try
        {
            result = await asyncFunc();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            _asyncLock.Release();
        }

        return result;
    }
}
