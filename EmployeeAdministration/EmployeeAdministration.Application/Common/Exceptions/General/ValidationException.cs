namespace EmployeeAdministration.Application.Common.Exceptions.General;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; private set; }

    public static ValidationException GenerateExceptionForEmptyRequest() =>
        throw new ValidationException("Other", "Please specify at least one of the fields");

    public ValidationException(string property, string message)
        => Errors = new() { { property, [message] } };

    public ValidationException(Dictionary<string, string[]> errors)
        => Errors = errors;
}
