namespace EmployeeAdministration.Application.Common.Exceptions;

public class InvalidPasswordException : BaseException
{
    public InvalidPasswordException() : base("Password provided is incorrect") { }
}
