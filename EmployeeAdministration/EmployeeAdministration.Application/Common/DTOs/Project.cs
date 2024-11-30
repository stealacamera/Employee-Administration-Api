using System.ComponentModel.DataAnnotations;
using EmployeeAdministration.Application.Common.Validation;
using EmployeeAdministration.Domain.Enums;

namespace EmployeeAdministration.Application.Common.DTOs;

public record BriefProject(
    [Required] int Id,
    [Required, StringLength(ValidationUtils.ProjectNameLength)] string Name,
    [StringLength(ValidationUtils.ProjectDescriptionLength)] string? Description = null);

public record Project(
    [Required] int Id,
    [Required, StringLength(ValidationUtils.ProjectNameLength)] string Name,
    [Required] IList<Task> Tasks);

public record ComprehensiveProject(
    [Required] int Id,
    [Required, StringLength(ValidationUtils.ProjectNameLength)] string Name,
    [Required] ProjectStatuses Status,
    [Required] IList<Task> Tasks,
    [Required] IList<BriefUser> Members,
    [StringLength(ValidationUtils.ProjectDescriptionLength)] string? Description = null);

public record CreateProjectRequest(
    [Required, StringLength(ValidationUtils.ProjectNameLength)] string Name,
    [StringLength(ValidationUtils.ProjectDescriptionLength)] string? Description = null,
    [MaxLength(100)] int[]? EmployeeIds = null);

public record UpdateProjectRequest(
    [StringLength(ValidationUtils.ProjectNameLength)] string? Name = null,
    [StringLength(ValidationUtils.ProjectDescriptionLength)] string? Description = null,
    ProjectStatuses? Status = null);