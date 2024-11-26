using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Application.Common.Exceptions;
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
            throw new ValidationException(GroupIdentityErrors(result.Errors));

        return user;
    }

    public async System.Threading.Tasks.Task AddToRoleAsync(User user, Roles role, CancellationToken cancellationToken = default)
        => await _userManager.AddToRoleAsync(user, Enum.GetName(role)!);

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
            query = query.Where(e => e.RolesIds.Contains((int)filterByRole));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _userManager.FindByEmailAsync(email);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<Roles> GetUserRoleAsync(User user, CancellationToken cancellationToken = default)
        => Enum.Parse<Roles>((await _userManager.GetRolesAsync(user))[0]);

    public async Task<bool> IsEmailInUseAsync(string email, bool includeDeletedUsers = false, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return false;
        else if (!includeDeletedUsers && user.DeletedAt != null)
            return false;

        return true;
    }

    public async Task<bool> IsUserInRoleAsync(User user, Roles role, CancellationToken cancellationToken = default)
        => await _userManager.IsInRoleAsync(user, Enum.GetName(role)!);

    public async Task<bool> VerifyCredentialsAsync(User user, string password, CancellationToken cancellationToken = default)
        => await _userManager.CheckPasswordAsync(user, password);

    private Dictionary<string, string[]> GroupIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var groupedErrors = new Dictionary<string, string[]>();

        foreach (var error in errors)
        {
            string errorTitle;

            if (error.Code.Contains("Password"))
                errorTitle = "Password";
            else if (error.Code.Contains("Role"))
                errorTitle = "Role";
            else if (error.Code.Contains("UserName"))
                errorTitle = "Username";
            else if (error.Code.Contains("Email"))
                errorTitle = "Email";
            else
                errorTitle = "Other";

            if (groupedErrors.ContainsKey(errorTitle))
                groupedErrors[errorTitle].Append(error.Description);
            else
                groupedErrors.Add(errorTitle, [error.Description]);
        }

        return groupedErrors;
    }
}
