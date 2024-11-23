using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Application.Common.DTOs;

public record User(
    [Required] int Id, 
    [Required, StringLength(ValidationUtils.EmailLength)] string Email, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName);

public record VerifyCredentialsRequest(
    [Required, EmailAddress, StringLength(80)] string Email, 
    [Required, StringLength(80)] string Password);

public record CreateUserRequest(
    [Required, EmailAddress, StringLength(80)] string Email, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName,
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName, 
    [Required, StringLength(ValidationUtils.UserPasswordLength)] string Password, 
    [FileExtensions(Extensions = ValidationUtils.AcceptableFileExtensions)] IFormFile? ProfilePicture);

public record UpdateUserRequest
{
    [Required, Range(1, int.MaxValue)]
    public int RequesterId { get; private set; }

    [Required, Range(1, int.MaxValue)] 
    public int UserId { get; private set; }

    [StringLength(ValidationUtils.UserNameLength)] 
    public string? FirstName { get; private set; }
    
    [StringLength(ValidationUtils.UserNameLength)] 
    public string? LastName { get; private set; }

    [FileExtensions(Extensions = ValidationUtils.AcceptableFileExtensions)] 
    public IFormFile? ProfilePicture { get; private set; }

    public UpdateUserRequest(int requesterId, int userId, string? firstName, string? lastName, IFormFile? profilePicture)
    {
        RequesterId = requesterId;
        UserId = userId;
        ProfilePicture = profilePicture;

        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
    }
}