using EmployeeAdministration.Application.Abstractions.Interfaces;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Application.Services;

internal class UsersService : BaseService, IUsersService
{
    private readonly UserManager<Domain.Entities.User> _userManager;

    public UsersService(IServiceProvider serviceProvider) : base(serviceProvider)
        => _userManager = serviceProvider.GetRequiredService<UserManager<Domain.Entities.User>>();

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existingEmail = await _userManager.FindByEmailAsync(request.Email);

        // Check if the email is currently in use
        if (existingEmail != null && existingEmail.DeletedAt == null)
            throw new ValidationException("Email is in use by an existing account");

        var newUser = new Domain.Entities.User
        {
            Email = request.Email,
            EmailConfirmed = true,
            FirstName = request.FirstName,
            Surname = request.LastName,
            ProfilePictureName = request.ProfilePicture?.Name // TODO add service helper for media
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
            throw new ValidationException(GroupIdentityErrors(result.Errors));

        return new User(newUser.Id, newUser.Email, newUser.FirstName, newUser.Surname);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        // If user is an employee:
        // Check if there are open tasks assigned to user
        // Remove memberships from projects
        bool isUserEmployee = await _userManager.IsInRoleAsync(user, nameof(Roles.Employee));

        if(isUserEmployee && await _workUnit.TasksRepository
                                            .DoesUserHaveOpenTasksAsync(userId, cancellationToken))
            throw new UncompletedTasksAssignedToEntityException(nameof(User));

        await WrapInTransactionAsync(async () =>
        {
            if(isUserEmployee)
            {
                await _workUnit.ProjectMembersRepository
                               .DeleteAllMembershipsForUserAsync(userId, cancellationToken);
                
                await _workUnit.SaveChangesAsync();
            }

            user.DeletedAt = DateTime.UtcNow;
            await _workUnit.SaveChangesAsync();
        });
    }

    public async Task<User> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var requester = await _userManager.FindByIdAsync(request.RequesterId.ToString());
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (requester == null || requester.DeletedAt != null)
            throw new UnauthorizedException();
        else if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        // Check if the user is an employee, they're updating themselves
        if (await _userManager.IsInRoleAsync(requester, nameof(Roles.Employee)) && 
            requester.Id != request.UserId)
            throw new UnauthorizedException();

        // Update user profile
        if(request.FirstName != null)
            user.FirstName = request.FirstName;
        if(request.LastName != null)
            user.Surname = request.LastName;
        if (request.ProfilePicture != null)
            ; // TODO

        await _workUnit.SaveChangesAsync();
        return new User(user.Id, user.Email!, user.FirstName, user.Surname);
    }

    public async Task<bool> VerifyCredentialsAsync(VerifyCredentialsRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        return await _userManager.CheckPasswordAsync(user, request.Password);
    }

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
