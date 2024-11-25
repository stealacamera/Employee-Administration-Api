namespace EmployeeAdministration.Application.Common.Exceptions;

public class NonEmployeeUserException : BaseException
{
    public NonEmployeeUserException() : base("User provided is not an employee")
    {
    }
}
