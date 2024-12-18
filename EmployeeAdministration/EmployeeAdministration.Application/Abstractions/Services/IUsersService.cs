﻿using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;

namespace EmployeeAdministration.Application.Abstractions.Interfaces;

public interface IUsersService
{
    Task<UserProfile> GetProfileByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IList<User>> GetAllAsync(
        Roles? filterByRole = null,
        bool includeDeletedUsers = false,
        CancellationToken cancellationToken = default);

    Task<bool> DoesUserEmailExistAsync(
        string email, 
        bool includeDeletedEntities = false, 
        CancellationToken cancellationToken = default);

    Task<LoggedInUser?> VerifyCredentialsAsync(
        VerifyCredentialsRequest request,
        CancellationToken cancellationToken = default);

    Task<User> CreateUserAsync(
        CreateUserRequest request, 
        CancellationToken cancellationToken = default);
    
    System.Threading.Tasks.Task DeleteUserAsync(
        int userId, 
        CancellationToken cancellationToken = default);
    
    Task<User> UpdateUserAsync(
        int requesterId, 
        int userId, 
        UpdateUserRequest request, 
        CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task UpdatePasswordAsync(
        int requesterId,
        UpdatePasswordRequest request,
        CancellationToken cancellationToken = default);
}
