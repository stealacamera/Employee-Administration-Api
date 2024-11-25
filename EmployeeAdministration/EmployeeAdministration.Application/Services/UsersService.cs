using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Interfaces;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Application.Services;

internal class UsersService : BaseService, IUsersService
{
    private readonly IJwtProvider _jwtProvider;

    public UsersService(IServiceProvider serviceProvider) : base(serviceProvider)
        => _jwtProvider = serviceProvider.GetRequiredService<IJwtProvider>();

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Check if the email is currently in use
        if(await _workUnit.UsersRepository
                          .IsEmailInUseAsync(request.Password, cancellationToken: cancellationToken))
            throw new ValidationException("Email is in use by an existing account");

        return await WrapInTransactionAsync(async () =>
        {
            // Create user
            var newUser = new Domain.Entities.User
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                FirstName = request.FirstName,
                Surname = request.LastName,
                ProfilePictureName = request.ProfilePicture?.Name // TODO add service helper for media
            };

            await _workUnit.UsersRepository
                           .AddAsync(newUser, request.Password, cancellationToken);

            // Attach role to new user
            await _workUnit.UsersRepository
                           .AddToRoleAsync(newUser, request.Role, cancellationToken);

            return new User(
                newUser.Id, newUser.Email, 
                newUser.FirstName, newUser.Surname, request.Role);
        });
    }

    public async System.Threading.Tasks.Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByIdAsync(userId, cancellationToken);

        if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        // If user is an employee, check if there are open tasks assigned to user
        bool isUserEmployee = await _workUnit.UsersRepository
                                             .IsUserInRoleAsync(user, Roles.Employee, cancellationToken);

        if(isUserEmployee && await _workUnit.TasksRepository
                                            .DoesUserHaveOpenTasksAsync(userId, cancellationToken: cancellationToken))
            throw new UncompletedTasksAssignedToEntityException(nameof(User));

        // Delete user & associated entities
        await WrapInTransactionAsync(async () =>
        {
            if(isUserEmployee)
            {
                await _workUnit.ProjectMembersRepository
                               .DeleteAllForUserAsync(userId, cancellationToken);
                
                await _workUnit.SaveChangesAsync();
            }

            user.ProfilePictureName = null; // TODO delete pfp file 
            user.DeletedAt = DateTime.UtcNow;

            await _workUnit.SaveChangesAsync();
        });
    }

    public async Task<IList<User>> GetAllAsync(int requesterId, CancellationToken cancellationToken = default)
    {
        var requester = await _workUnit.UsersRepository
                                       .GetByIdAsync(requesterId);

        if (requester == null)
            throw new UnauthorizedException();

        // Restrict result if requester is not administrator
        // Get only currently employeed employees
        bool isRequesterAdmin = await _workUnit.UsersRepository
                                               .IsUserInRoleAsync(requester, Roles.Administrator, cancellationToken);

        var users = await _workUnit.UsersRepository
                                   .GetAllAsync(
                                        includeDeletedUsers: isRequesterAdmin, 
                                        filterByRole: !isRequesterAdmin ? Roles.Employee : null,
                                        cancellationToken);

        return users.Select(async e => 
                    {
                        var userRole = await _workUnit.UsersRepository
                                                .GetUserRoleAsync(e, cancellationToken);

                        return new User(
                            e.Id, e.Email,
                            e.FirstName, e.Surname,
                            userRole, e.ProfilePictureName, e.DeletedAt);
                    })
                    .Select(e => e.Result)
                    .ToList();
    }

    public async Task<bool> DoesUserEmailExistAsync(
        string email, 
        bool includeDeletedEntities = false,
        CancellationToken cancellationToken = default)
        => await _workUnit.UsersRepository
                          .IsEmailInUseAsync(email, includeDeletedEntities, cancellationToken);

    public async Task<UserProfile> GetProfileByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByIdAsync(id, cancellationToken);

        if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        var userRole = await _workUnit.UsersRepository
                                      .GetUserRoleAsync(user, cancellationToken);

        var userModel = new User(
            user.Id, user.Email, user.FirstName, user.Surname, 
            userRole, user.ProfilePictureName);

        var membershipProjectIds = (await _workUnit.ProjectMembersRepository
                                         .GetAllForUserAsync(user.Id, cancellationToken))
                                         .Select(e => e.ProjectId)
                                         .ToArray();

        var projects = await _workUnit.ProjectsRepository
                                      .GetAllByIdsAsync(membershipProjectIds, cancellationToken);

        return new UserProfile(userModel, ConvertProjects(projects, userModel, cancellationToken));
    }

    public async Task<User> UpdateUserAsync(
        int requesterId, 
        int userId, 
        UpdateUserRequest request, 
        CancellationToken cancellationToken = default)
    {
        var requester = await _workUnit.UsersRepository.GetByIdAsync(requesterId, cancellationToken);
        var user = await _workUnit.UsersRepository.GetByIdAsync(userId, cancellationToken);

        if (requester == null || requester.DeletedAt != null)
            throw new UnauthorizedException();
        else if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        // Check if the user is an employee, they're updating themselves
        bool isUserEmployee = await _workUnit.UsersRepository
                                             .IsUserInRoleAsync(requester, Roles.Employee, cancellationToken);
        
        if (isUserEmployee && requester.Id != userId)
            throw new UnauthorizedException();

        // Update user profile
        if(request.FirstName != null)
            user.FirstName = request.FirstName;
        if(request.LastName != null)
            user.Surname = request.LastName;
        if (request.ProfilePicture != null)
            ; // TODO

        await _workUnit.SaveChangesAsync();

        var userRole = await _workUnit.UsersRepository
                                      .GetUserRoleAsync(user, cancellationToken);

        return new User(
            user.Id, user.Email!, 
            user.FirstName, user.Surname,
            userRole, user.ProfilePictureName);
    }

    public async Task<LoggedInUser?> VerifyCredentialsAsync(
        VerifyCredentialsRequest request, 
        CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || user.DeletedAt != null)
            throw new EntityNotFoundException(nameof(User));

        if (!await _workUnit.UsersRepository
                            .VerifyCredentialsAsync(user, request.Password, cancellationToken))
            return null;

        var userRole = await _workUnit.UsersRepository
                                      .GetUserRoleAsync(user, cancellationToken);

        return new LoggedInUser(
            new User(user.Id, user.Email, user.FirstName, user.Surname, userRole, user.ProfilePictureName),
            new Tokens(_jwtProvider.GenerateToken(user.Id, user.Email), _jwtProvider.GenerateRefreshToken()));
    }

    // Helper functions
    private IList<Project> ConvertProjects(
        IEnumerable<Domain.Entities.Project> models,
        User user,
        CancellationToken cancellationToken)
    {
        return models.Select(async e =>
                    {
                        var tasks = await _workUnit.TasksRepository
                                                   .GetAllForUserAsync(user.Id, e.Id, cancellationToken);

                        var taskModels = tasks.Select(async e => await ConvertTaskAsync(e, user, cancellationToken))
                                              .Select(e => e.Result)
                                              .ToList();

                        return new Project(e.Id, e.Name, taskModels);
                    })
                     .Select(e => e.Result)
                     .ToList();
    }

    private async Task<Common.DTOs.Task> ConvertTaskAsync(
        Domain.Entities.Task model, 
        User appointee, 
        CancellationToken cancellationToken)
    {
        User? appointer = null;

        // Add appointer user if the task isn't self-assigned
        if (model.AppointeeEmployeeId != model.AppointerUserId)
        {
            var appointerEntity = await _workUnit.UsersRepository
                                                 .GetByIdAsync(model.AppointerUserId, cancellationToken);

            if (appointerEntity == null)
                throw new EntityNotFoundException(nameof(User));

            var appointerRole = await _workUnit.UsersRepository
                                          .GetUserRoleAsync(appointerEntity, cancellationToken);

            appointer = new User(
                appointerEntity.Id, appointerEntity.Email,
                appointerEntity.FirstName, appointerEntity.Surname,
                appointerRole, appointerEntity.ProfilePictureName);
        }

        return new Common.DTOs.Task(
            model.Id, appointer, appointee,
            model.Name, model.IsCompleted,
            model.CreatedAt, model.Description);
    }
}
