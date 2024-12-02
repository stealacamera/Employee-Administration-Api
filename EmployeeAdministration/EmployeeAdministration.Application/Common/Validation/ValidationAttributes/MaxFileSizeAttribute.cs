using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Application.Common.Validation.ValidationAttributes;

public sealed class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize) => _maxFileSize = maxFileSize;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;

        if (file == null)
            return ValidationResult.Success;

        return file.Length <= _maxFileSize ?
               ValidationResult.Success :
               new ValidationResult($"File exceeds maximum length of {_maxFileSize / 1_000_000} MB");
    }
}
