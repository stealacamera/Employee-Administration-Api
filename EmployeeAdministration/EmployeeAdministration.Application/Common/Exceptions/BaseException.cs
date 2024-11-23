namespace EmployeeAdministration.Application.Common.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException(string message): base(message) { }
    protected BaseException() { }
}
