namespace EmployeeAdministration.Application.Common.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]>? Errors { get; private set; } = null;

    public ValidationException(string message) : base(message) { }

    public ValidationException(Dictionary<string, string[]> errors)
        => Errors = errors;
}
