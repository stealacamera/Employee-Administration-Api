using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class UserRolesRepository : IUserRolesRepository
{
    private readonly UserManager<User> _userManager;
    private readonly IDistributedCache _distributedCache;

    public UserRolesRepository(UserManager<User> userManager, IDistributedCache distributedCache)
    {
        _userManager = userManager;
        _distributedCache = distributedCache;
    }

    public async Task<bool> IsUserInRoleAsync(int userId, Roles role, CancellationToken cancellationToken = default)
    {
        var cachedUserRole = await _distributedCache.GetStringAsync(CacheKeys.UserRole(userId), cancellationToken);
        Roles userRole;

        if (cachedUserRole == null)
        {
            var user = (await _userManager.FindByIdAsync(userId.ToString()))!;
            userRole = await GetRoleAsync(user, cancellationToken);

            await _distributedCache.SetStringAsync(
                CacheKeys.UserRole(user.Id),
                JsonConvert.SerializeObject(userRole),
                cancellationToken);
        }
        else
            userRole = JsonConvert.DeserializeObject<Roles>(cachedUserRole);

        return userRole == role;
    }

    public async Task<Roles> GetUserRoleAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cachedUserRole = await _distributedCache.GetStringAsync(CacheKeys.UserRole(userId), cancellationToken);
        Roles userRole;

        if (cachedUserRole == null)
        {
            var user = (await _userManager.FindByIdAsync(userId.ToString()))!;
            userRole = await GetRoleAsync(user, cancellationToken);

            // Cache user role
            await _distributedCache.SetStringAsync(
                CacheKeys.UserRole(user.Id),
                JsonConvert.SerializeObject(userRole),
                cancellationToken);
        }
        else
            userRole = JsonConvert.DeserializeObject<Roles>(cachedUserRole);

        return userRole;
    }

    public async System.Threading.Tasks.Task AddToRoleAsync(int userId, Roles role, CancellationToken cancellationToken = default)
    {
        var user = (await _userManager.FindByIdAsync(userId.ToString()))!;
        await _userManager.AddToRoleAsync(user, Enum.GetName(role)!);

        await _distributedCache.SetStringAsync(
            CacheKeys.UserRole(user.Id),
            JsonConvert.SerializeObject(role),
            cancellationToken);
    }


    // Helper functions
    private async Task<Roles> GetRoleAsync(User user, CancellationToken cancellationToken)
        => Enum.Parse<Roles>((await _userManager.GetRolesAsync(user))[0]);
}
