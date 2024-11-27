using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly UserManager<User> _userManager;
    private readonly IDistributedCache _distributedCache;

    public UsersRepository(UserManager<User> userManager, IDistributedCache distributedCache)
    {
        _userManager = userManager;
        _distributedCache = distributedCache;
    }

    public async Task<User> AddAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new IdentityException(GroupIdentityErrors(result.Errors));

        return user;
    }

    public async System.Threading.Tasks.Task AddToRoleAsync(User user, Roles role, CancellationToken cancellationToken = default)
    {
        await _userManager.AddToRoleAsync(user, Enum.GetName(role)!);

        await _distributedCache.SetStringAsync(
            CacheKeys.UserRole(user.Id), 
            JsonConvert.SerializeObject(role), 
            cancellationToken);
    }

    public async Task<bool> DoesUserExistAsync(int id, bool includeDeletedUsers = false, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
            return false;
        else if (!includeDeletedUsers && user.DeletedAt != null)
            return false;

        return true;
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeDeletedUsers = false, Roles? filterByRole = null, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users;

        if (!includeDeletedUsers)
            query = query.Where(e => e.DeletedAt != null);

        if (filterByRole != null)
            query = query.Where(e => e.Roles.Any(e => e.RoleId == (int)filterByRole));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _userManager.FindByEmailAsync(email);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<Roles> GetUserRoleAsync(User user, CancellationToken cancellationToken = default)
    {
        var cachedUserRole = await _distributedCache.GetStringAsync($"{CacheKeys.UserRole}-${user.Id}", cancellationToken);
        Roles userRole;

        if (cachedUserRole == null)
        {
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

    public async Task<bool> IsEmailInUseAsync(string email, bool includeDeletedUsers = false, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return false;
        else if (!includeDeletedUsers && user.DeletedAt != null)
            return false;

        return true;
    }

    public async Task<bool> IsPasswordCorrectAsync(User user, string password, CancellationToken cancellationToken = default)
        => await _userManager.CheckPasswordAsync(user, password);

    public async Task<bool> IsUserInRoleAsync(User user, Roles role, CancellationToken cancellationToken = default)
    {
        var cachedUserRole = await _distributedCache.GetStringAsync(CacheKeys.UserRole(user.Id), cancellationToken);
        Roles userRole;

        if(cachedUserRole == null)
        {
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

    public async System.Threading.Tasks.Task UpdatePassword(User user, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

        if (!result.Succeeded)
            throw new IdentityException(GroupIdentityErrors(result.Errors));
    }

    public async Task<bool> VerifyCredentialsAsync(User user, string password, CancellationToken cancellationToken = default)
        => await _userManager.CheckPasswordAsync(user, password);


    // Helper functions
    private async Task<Roles> GetRoleAsync(User user, CancellationToken cancellationToken)
        => Enum.Parse<Roles>((await _userManager.GetRolesAsync(user))[0]);
}
