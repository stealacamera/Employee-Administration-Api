using System.ComponentModel.DataAnnotations;
using EmployeeAdministration.Application.Common.Validation;

namespace EmployeeAdministration.Application.Common.DTOs;

public record Task(
    [Required] int Id,
    [Required] BriefUser? Appointer,
    [Required] BriefUser Appointee,
    [Required, StringLength(ValidationUtils.TaskNameLength)] string Name,
    [Required] bool IsCompleted,
    [Required] DateTime CreatedAt,
    [StringLength(ValidationUtils.TaskDescriptionLength)] string? Description = null);

public record CreateTaskRequest(
    [Required, Range(1, int.MaxValue)] int AppointeeId,
    [Required, StringLength(ValidationUtils.TaskNameLength)] string Name,
    [StringLength(ValidationUtils.TaskDescriptionLength)] string? Description = null);

public record UpdateTaskRequest(
    [StringLength(ValidationUtils.TaskNameLength)] string? Name = null,
    [StringLength(ValidationUtils.TaskDescriptionLength)] string? Description = null,
    bool? IsCompleted = null);