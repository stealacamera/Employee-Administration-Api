using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Interfaces;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Common.Exceptions.General;
using EmployeeAdministration.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Infrastructure.Services;

internal class UsersService : BaseService, IUsersService
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IImagesStorageService _imagesService;

    public UsersService(
        IWorkUnit workUnit,
        IJwtProvider jwtProvider,
        IImagesStorageService imagesService)
        : base(workUnit)
    {
        _jwtProvider = jwtProvider;
        _imagesService = imagesService;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Check if the email is currently in use
        if (await _workUnit.UsersRepository
                          .IsEmailInUseAsync(request.Email, includeDeletedUsers: true, cancellationToken))
            throw new ValidationException("Email", "Email is in use by an existing account");

        string? profilePictureId = null;

        return await WrapInTransactionAsync(
            async () =>
            {
                if (request.ProfilePicture != null)
                    profilePictureId = await _imagesService.SaveFileAsync(request.ProfilePicture, cancellationToken);

                // Create user
                var newUser = new Domain.Entities.User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                    FirstName = request.FirstName,
                    Surname = request.LastName,
                    ProfilePictureName = profilePictureId
                };

                await _workUnit.UsersRepository.AddAsync(newUser, request.Password, cancellationToken);
                await _workUnit.SaveChangesAsync();

                // Attach role to new user
                await _workUnit.UserRolesRepository.AddToRoleAsync(newUser.Id, request.Role, cancellationToken);
                await _workUnit.SaveChangesAsync();

                string? profilePictureUrl = profilePictureId != null ?
                                            await _imagesService.GetFileUrlAsync(profilePictureId, cancellationToken) :
                                            null;

                return new User(
                    newUser.Id, newUser.Email,
                    newUser.FirstName, newUser.Surname,
                    request.Role, profilePictureUrl);
            },
            async () =>
            {
                if (profilePictureId != null)
                    await _imagesService.DeleteFileAsync(profilePictureId, cancellationToken);
            });
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByIdAsync(userId, cancellationToken: cancellationToken);

        if (user == null)
            throw new EntityNotFoundException(nameof(User));

        // If user is an employee, check if there are open tasks assigned to user
        bool isUserEmployee = await _workUnit.UserRolesRepository
                                             .IsUserInRoleAsync(user.Id, Roles.Employee, cancellationToken);

        if (isUserEmployee && await _workUnit.TasksRepository
                                            .DoesUserHaveOpenTasksAsync(userId, cancellationToken: cancellationToken))
            throw new UncompletedTasksAssignedToEntityException(nameof(User));

        // Delete user & associated entities
        await WrapInTransactionAsync(async () =>
        {
            var profilePictureId = user.ProfilePictureName;

            if (isUserEmployee)
            {
                await _workUnit.ProjectMembersRepository.DeleteAllForUserAsync(userId, cancellationToken);
                await _workUnit.SaveChangesAsync();
            }

            user.ProfilePictureName = null;
            user.DeletedAt = DateTime.UtcNow;

            await _workUnit.SaveChangesAsync();

            if (profilePictureId != null)
                await _imagesService.DeleteFileAsync(profilePictureId, cancellationToken);
        });
    }

    public async Task<IList<User>> GetAllAsync(
        Roles? filterByRole = null,
        bool includeDeletedUsers = false,
        CancellationToken cancellationToken = default)
    {
        var users = await _workUnit.UsersRepository
                                   .GetAllAsync(includeDeletedUsers, filterByRole, cancellationToken);

        return users.Select(async e => await ConvertUserToModelAsync(e, cancellationToken))
                    .Select(e => e.Result)
                    .ToList();
    }

    public async Task<bool> DoesUserEmailExistAsync(
        string email,
        bool includeDeletedEntities = false,
        CancellationToken cancellationToken = default)
        => await _workUnit.UsersRepository
                          .IsEmailInUseAsync(email, includeDeletedEntities, cancellationToken);

    public async Task<UserProfile> GetProfileByIdAsync(int requesterId, CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByIdAsync(requesterId, cancellationToken: cancellationToken);

        if (user == null)
            throw new UnauthorizedException();

        // Get user details
        var userRole = await _workUnit.UserRolesRepository
                                      .GetUserRoleAsync(user.Id, cancellationToken);

        var profilePictureId = user.ProfilePictureName != null ?
                               await _imagesService.GetFileUrlAsync(user.ProfilePictureName, cancellationToken) :
                               null;

        var userModel = new User(
            user.Id, user.Email, user.FirstName, user.Surname,
            userRole, profilePictureId);

        var briefUser = new BriefUser(user.Id, user.Email, user.FirstName, user.Surname, user.DeletedAt);

        // Get user projects and tasks
        var membershipProjectIds = (await _workUnit.ProjectMembersRepository
                                                   .GetAllForUserAsync(user.Id, cancellationToken))
                                                   .Select(e => e.ProjectId)
                                                   .ToArray();

        var projects = (await _workUnit.ProjectsRepository
                                       .GetAllByIdsAsync(membershipProjectIds, cancellationToken))
                                       .Select(async e => await ConvertProjectToModelAsync(e, briefUser, cancellationToken))
                                       .Select(e => e.Result)
                                       .ToList();

        return new UserProfile(userModel, projects);
    }

    public async Task<User> UpdateUserAsync(
        int requesterId,
        int userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProfilePicture == null && request.FirstName == null && request.Surname == null)
            ValidationException.GenerateExceptionForEmptyRequest();

        var requester = await _workUnit.UsersRepository.GetByIdAsync(requesterId, cancellationToken: cancellationToken);
        var user = await _workUnit.UsersRepository.GetByIdAsync(userId, cancellationToken: cancellationToken);

        await ValidateUpdateRequesterAsync(requester, user, cancellationToken);

        // Update user profile
        await WrapInTransactionAsync(async () =>
        {
            var oldProfilePictureId = user.ProfilePictureName;

            if (request.FirstName != null)
                user.FirstName = request.FirstName;
            if (request.Surname != null)
                user.Surname = request.Surname;
            if (request.ProfilePicture != null)
            {

                var profilePictureId = await _imagesService.SaveFileAsync(request.ProfilePicture, cancellationToken);
                user.ProfilePictureName = profilePictureId;
            }

            await _workUnit.SaveChangesAsync();
        
            if (oldProfilePictureId != null)
                await _imagesService.DeleteFileAsync(oldProfilePictureId, cancellationToken);
        });

        var userRole = await _workUnit.UserRolesRepository
                                      .GetUserRoleAsync(user.Id, cancellationToken);

        var profilePictureUrl = user.ProfilePictureName != null ?
                                await _imagesService.GetFileUrlAsync(user.ProfilePictureName, cancellationToken) :
                                null;

        return new User(
            user.Id, user.Email!,
            user.FirstName, user.Surname,
            userRole, profilePictureUrl);
    }

    public async Task<LoggedInUser?> VerifyCredentialsAsync(
        VerifyCredentialsRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByEmailAsync(request.Email, cancellationToken: cancellationToken);

        if (user == null)
            return null;

        var areCorrectCredentials = await _workUnit.UsersRepository
                                                   .VerifyCredentialsAsync(user, request.Password, cancellationToken);

        if (!areCorrectCredentials)
            return null;

        // Get user details
        var userRole = await _workUnit.UserRolesRepository
                                      .GetUserRoleAsync(user.Id, cancellationToken);

        var profilePictureUrl = user.ProfilePictureName != null ?
                                await _imagesService.GetFileUrlAsync(user.ProfilePictureName, cancellationToken) :
                                null;

        _jwtProvider.UpdateRefreshToken(user);
        await _workUnit.SaveChangesAsync();

        return new LoggedInUser(
            new User(user.Id, user.Email, user.FirstName, user.Surname, userRole, profilePictureUrl),
            new Tokens(_jwtProvider.GenerateToken(user.Id, user.Email), user.RefreshToken));
    }

    public async Task UpdatePasswordAsync(
        int requesterId,
        UpdatePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var requester = await _workUnit.UsersRepository.GetByIdAsync(requesterId, cancellationToken: cancellationToken);

        if (requester == null)
            throw new UnauthorizedException();
        else if (!await _workUnit.UsersRepository
                                 .IsPasswordCorrectAsync(requester, request.CurrentPassword, cancellationToken))
            throw new InvalidPasswordException();

        await _workUnit.UsersRepository
                       .UpdatePassword(
                            requester,
                            request.CurrentPassword,
                            request.NewPassword,
                            cancellationToken);

        await _workUnit.SaveChangesAsync();
    }


    // Helper functions
    private async Task ValidateUpdateRequesterAsync(
        Domain.Entities.User? requester,
        Domain.Entities.User? user,
        CancellationToken cancellationToken)
    {
        if (requester == null)
            throw new UnauthorizedException();
        else if (user == null)
            throw new EntityNotFoundException(nameof(User));

        // Check if the user is an employee, they're updating themselves
        bool isRequesterEmployee = await _workUnit.UserRolesRepository
                                                  .IsUserInRoleAsync(requester.Id, Roles.Employee, cancellationToken);

        if (isRequesterEmployee && requester.Id != user.Id)
            throw new UnauthorizedException();
    }

    private async Task<User> ConvertUserToModelAsync(Domain.Entities.User user, CancellationToken cancellationToken)
    {
        var userRole = await _workUnit.UserRolesRepository
                                                .GetUserRoleAsync(user.Id, cancellationToken);

        var profilePictureUrl = user.ProfilePictureName != null ?
                                await _imagesService.GetFileUrlAsync(user.ProfilePictureName, cancellationToken) :
                                null;

        return new User(
            user.Id, user.Email,
            user.FirstName, user.Surname,
            userRole, profilePictureUrl, user.DeletedAt);
    }

    private async Task<Project> ConvertProjectToModelAsync(
        Domain.Entities.Project model,
        BriefUser user,
        CancellationToken cancellationToken)
    {
        var tasks = await _workUnit.TasksRepository
                                    .GetAllForUserAsync(user.Id, model.Id, cancellationToken);

        var taskModels = tasks.Select(async e => await ConvertTaskToModelAsync(e, user, cancellationToken))
                                .Select(e => e.Result)
                                .ToList();

        return new Project(model.Id, model.Name, ProjectStatuses.FromId(model.StatusId), taskModels);

    }

    private async Task<Application.Common.DTOs.Task> ConvertTaskToModelAsync(
        Domain.Entities.Task model,
        BriefUser appointee,
        CancellationToken cancellationToken)
    {
        BriefUser? appointer = null;

        // Add appointer user if the task isn't self-assigned
        if (model.AppointeeEmployeeId != model.AppointerUserId)
        {
            var appointerEntity = await _workUnit.UsersRepository
                                                 .GetByIdAsync(model.AppointerUserId, cancellationToken: cancellationToken);

            if (appointerEntity == null)
                throw new EntityNotFoundException(nameof(User));

            var appointerRole = await _workUnit.UserRolesRepository
                                               .GetUserRoleAsync(appointerEntity.Id, cancellationToken);

            appointer = new BriefUser(
                appointerEntity.Id, appointerEntity.Email,
                appointerEntity.FirstName, appointerEntity.Surname);
        }

        return new Application.Common.DTOs.Task(
            model.Id, appointer, appointee,
            model.Name, model.IsCompleted,
            model.CreatedAt, model.Description);
    }
}
