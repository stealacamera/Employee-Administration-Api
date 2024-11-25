using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Application.Common.DTOs;

public record Task(
    [Required] int Id,
    [Required] BriefUser? Appointer,
    [Required] BriefUser Appointee,
    [Required, StringLength(150)] string Name,
    [Required] bool IsCompleted,
    [Required] DateTime CreatedAt,
    [StringLength(350)] string? Description = null);