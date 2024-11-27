namespace EmployeeAdministration.Application.Common.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; private set; }

    public ValidationException(string property, string message)
        => Errors = new() { { property, [message] } };

    public ValidationException(Dictionary<string, string[]> errors)
        => Errors = errors;
}
