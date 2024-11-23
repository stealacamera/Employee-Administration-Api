namespace EmployeeAdministration.Application.Common.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException() : base("You are unauthorized to perform this action")
    {
    }
}
