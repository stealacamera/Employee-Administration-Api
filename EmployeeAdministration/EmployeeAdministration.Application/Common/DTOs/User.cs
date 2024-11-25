using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Application.Common.DTOs;

public record BriefUser(
    [Required] int Id,
    [Required, StringLength(ValidationUtils.EmailLength)] string Email,
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName,
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName,
    DateTime? DeletedAt = null);

public record User(
    [Required] int Id, 
    [Required, StringLength(ValidationUtils.EmailLength)] string Email, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName,
    [Required] Roles Role,
    string? ProfilePictureName = null,
    DateTime? DeletedAt = null);

public record UserProfile(
    [Required] User User,
    [Required] IList<Project> Projects);

public record LoggedInUser(
    [Required] User User,
    [Required] Tokens Tokens);

public record VerifyCredentialsRequest(
    [Required, EmailAddress, StringLength(80)] string Email, 
    [Required, StringLength(80)] string Password);

public record CreateUserRequest(
    [Required] Roles Role,
    [Required, EmailAddress, StringLength(80)] string Email, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName,
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName, 
    [Required, StringLength(ValidationUtils.UserPasswordLength)] string Password, 
    [FileExtensions(Extensions = ValidationUtils.AcceptableFileExtensions)] IFormFile? ProfilePicture = null);

public record UpdateUserRequest
{
    [StringLength(ValidationUtils.UserNameLength)]
    public string? FirstName { get; private set; } = null;

    [StringLength(ValidationUtils.UserNameLength)]
    public string? LastName { get; private set; } = null;

    [FileExtensions(Extensions = ValidationUtils.AcceptableFileExtensions)]
    public IFormFile? ProfilePicture { get; private set; } = null;

    public UpdateUserRequest(string? firstName = null, string? lastName = null, IFormFile? profilePicture = null)
    {
        ProfilePicture = profilePicture;

        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
    }
}