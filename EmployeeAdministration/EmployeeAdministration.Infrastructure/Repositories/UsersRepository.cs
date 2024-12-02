using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Application.Common.Exceptions.General;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly UserManager<User> _userManager;

    public UsersRepository(UserManager<User> userManager)
        => _userManager = userManager;

    public async Task<User> AddAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new IdentityException(result);

        return user;
    }

    public async Task<bool> DoesUserExistAsync(
        int id, 
        bool includeDeletedUsers = false, 
        CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, excludeDeletedUser: !includeDeletedUsers, cancellationToken);
        return user != null;
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeDeletedUsers = false, Roles? filterByRole = null, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users;

        if (!includeDeletedUsers)
            query = query.Where(e => e.DeletedAt == null);

        if (filterByRole != null)
            query = query.Where(e => e.Roles.Any(e => e.RoleId == (int)filterByRole));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email, 
        bool excludeDeletedUser = true, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (excludeDeletedUser && user != null && user.DeletedAt != null)
            return null;

        return user;
    }

    public async Task<User?> GetByIdAsync(
        int id,
        bool excludeDeletedUser = true,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (excludeDeletedUser && user != null && user.DeletedAt != null)
            return null;

        return user;
    }

    public async Task<bool> IsEmailInUseAsync(string email, bool includeDeletedUsers = false, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || (!includeDeletedUsers && user.DeletedAt != null))
            return false;

        return true;
    }

    public async Task<bool> IsPasswordCorrectAsync(User user, string password, CancellationToken cancellationToken = default)
        => await _userManager.CheckPasswordAsync(user, password);

    public async System.Threading.Tasks.Task UpdatePassword(User user, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }

    public async Task<bool> VerifyCredentialsAsync(User user, string password, CancellationToken cancellationToken = default)
        => await _userManager.CheckPasswordAsync(user, password);
}
