using EmployeeAdministration.Application.Common.Validation;
using EmployeeAdministration.Application.Common.Validation.ValidationAttributes;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using FileExtensionsAttribute = EmployeeAdministration.Application.Common.Validation.ValidationAttributes.FileExtensionsAttribute;

namespace EmployeeAdministration.Application.Common.DTOs;

public record BriefUser(
    [Required] int Id,
    [Required, StringLength(ValidationUtils.UserEmailLength)] string Email,
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName,
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName,
    DateTime? DeletedAt = null);

public record User(
    [Required] int Id, 
    [Required, StringLength(ValidationUtils.UserEmailLength)] string Email, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName,
    [Required] Roles Role,
    [Url] string? ProfilePictureUrl = null,
    DateTime? DeletedAt = null);

public record UserProfile(
    [Required] User User,
    [Required] IList<Project> Projects);

public record LoggedInUser(
    [Required] User User,
    [Required] Tokens Tokens);

public record VerifyCredentialsRequest(
    [Required, EmailAddress, StringLength(ValidationUtils.UserEmailLength)] string Email, 
    [Required, StringLength(ValidationUtils.UserPasswordLength)] string Password);

public record CreateUserRequest(
    [Required] Roles Role,
    [Required, EmailAddress, StringLength(80)] string Email, 
    [Required, StringLength(ValidationUtils.UserNameLength)] string FirstName,
    [Required, StringLength(ValidationUtils.UserNameLength)] string LastName, 

    [Required, 
     StringLength(ValidationUtils.UserPasswordLength), 
     RegularExpression(ValidationUtils.PasswordRegex, ErrorMessage = ValidationUtils.PasswordRegexErrorMessage)] 
    string Password, 
    
    [MaxFileSize(ValidationUtils.MaxImageBytes), 
     FileExtensions(ValidationUtils.AcceptableFileExtensions)] 
    IFormFile? ProfilePicture = null);

public record UpdateUserRequest(
    [StringLength(ValidationUtils.UserNameLength)] string? FirstName = null,
    [StringLength(ValidationUtils.UserNameLength)] string? Surname = null,

    [MaxFileSize(ValidationUtils.MaxImageBytes),
     FileExtensions(ValidationUtils.AcceptableFileExtensions)]
    IFormFile? ProfilePicture = null);

public record UpdatePasswordRequest(
    [Required, StringLength(ValidationUtils.UserPasswordLength)] string CurrentPassword, 
    [Required, 
     StringLength(ValidationUtils.UserPasswordLength), 
     RegularExpression(ValidationUtils.PasswordRegex, ErrorMessage = ValidationUtils.PasswordRegexErrorMessage)] 
    string NewPassword);